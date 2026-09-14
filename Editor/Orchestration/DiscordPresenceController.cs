using System;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class DiscordPresenceController : IDisposable
    {
        private static readonly double[] RetryDelays = { 5.0, 15.0, 30.0, 60.0 };

        private readonly IEditorContextSource contextSource;
        private readonly IDiscordUnityRpcPreferences preferences;
        private readonly DiscordPresenceFormatter formatter;
        private readonly IDiscordRpcTransport transport;
        private readonly IEditorClock clock;
        private readonly Action<string> errorLogger;

        private DiscordUnityRpcOptions options;
        private EditorContextSnapshot pendingSnapshot;
        private PresencePayload desiredPayload;
        private PresencePayload lastSentPayload;
        private long sessionStartedAt;
        private double publishAt = double.PositiveInfinity;
        private double retryAt = double.PositiveInfinity;
        private int retryAttempt;
        private bool started;
        private bool disposed;
        private bool transportInitialized;
        private bool transportDisposed;
        private bool errorReported;
        private bool manuallyCleared;

        internal DiscordPresenceController(
            IEditorContextSource contextSource,
            IDiscordUnityRpcPreferences preferences,
            DiscordPresenceFormatter formatter,
            IDiscordRpcTransport transport,
            IEditorClock clock,
            Action<string> errorLogger)
        {
            this.contextSource = contextSource ?? throw new ArgumentNullException("contextSource");
            this.preferences = preferences ?? throw new ArgumentNullException("preferences");
            this.formatter = formatter ?? throw new ArgumentNullException("formatter");
            this.transport = transport ?? throw new ArgumentNullException("transport");
            this.clock = clock ?? throw new ArgumentNullException("clock");
            this.errorLogger = errorLogger;
        }

        internal bool IsConnected { get { return transport.IsConnected; } }

        internal string ConnectionStatus
        {
            get
            {
                if (options == null || !options.Enabled)
                {
                    return "Disabled";
                }

                return transport.IsConnected ? "Connected" : "Waiting for Discord";
            }
        }

        internal void Start()
        {
            if (started || disposed)
            {
                return;
            }

            started = true;
            sessionStartedAt = clock.UnixSeconds;
            options = preferences.Current;
            contextSource.ContextChanged += OnContextChanged;
            preferences.Changed += OnPreferencesChanged;
            transport.Connected += OnConnected;
            transport.Disconnected += OnDisconnected;

            if (options.Enabled)
            {
                EnableAndQueue();
            }
        }

        internal void Tick()
        {
            if (disposed || options == null || !options.Enabled || transportDisposed)
            {
                return;
            }

            if (transportInitialized)
            {
                TryInvoke();
            }

            if (clock.TimeSinceStartup >= retryAt)
            {
                TryInitializeTransport();
                ScheduleNextRetry();
            }

            if (pendingSnapshot == null || clock.TimeSinceStartup < publishAt || !transportInitialized)
            {
                return;
            }

            desiredPayload = formatter.Format(pendingSnapshot, options, sessionStartedAt);
            pendingSnapshot = null;
            publishAt = double.PositiveInfinity;
            PublishDesired(false);
        }

        internal void ClearNow()
        {
            if (disposed || !transportInitialized || transportDisposed)
            {
                return;
            }

            TryClear();
            lastSentPayload = null;
            manuallyCleared = true;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            contextSource.ContextChanged -= OnContextChanged;
            preferences.Changed -= OnPreferencesChanged;
            transport.Connected -= OnConnected;
            transport.Disconnected -= OnDisconnected;

            if (!transportDisposed)
            {
                if (transportInitialized)
                {
                    TryClear();
                }

                transport.Dispose();
                transportDisposed = true;
            }

            contextSource.Dispose();
        }

        private void EnableAndQueue()
        {
            if (transportDisposed)
            {
                return;
            }

            TryInitializeTransport();
            QueueContext();
        }

        private void QueueContext()
        {
            pendingSnapshot = contextSource.Capture();
            publishAt = clock.TimeSinceStartup + 1.0;
            manuallyCleared = false;
            errorReported = false;
        }

        private void OnContextChanged()
        {
            if (options != null && options.Enabled)
            {
                QueueContext();
            }
        }

        private void OnPreferencesChanged()
        {
            var previousEnabled = options != null && options.Enabled;
            options = preferences.Current;
            errorReported = false;

            if (!options.Enabled)
            {
                if (previousEnabled)
                {
                    DisableTransport();
                }

                return;
            }

            if (!previousEnabled)
            {
                EnableAndQueue();
            }
            else
            {
                QueueContext();
            }
        }

        private void DisableTransport()
        {
            pendingSnapshot = null;
            desiredPayload = null;
            lastSentPayload = null;
            publishAt = double.PositiveInfinity;
            retryAt = double.PositiveInfinity;

            if (transportInitialized)
            {
                TryClear();
            }

            if (!transportDisposed)
            {
                transport.Dispose();
                transportDisposed = true;
            }

            transportInitialized = false;
        }

        private void OnConnected()
        {
            retryAttempt = 0;
            retryAt = double.PositiveInfinity;
            errorReported = false;

            if (manuallyCleared || options == null || !options.Enabled)
            {
                return;
            }

            if (desiredPayload == null)
            {
                var snapshot = pendingSnapshot ?? contextSource.Capture();
                desiredPayload = formatter.Format(snapshot, options, sessionStartedAt);
            }

            PublishDesired(true);
        }

        private void OnDisconnected()
        {
            if (disposed || options == null || !options.Enabled)
            {
                return;
            }

            retryAttempt = 0;
            ScheduleNextRetry();
        }

        private void TryInitializeTransport()
        {
            try
            {
                transport.Initialize(DiscordApplication.ApplicationId);
                transportInitialized = true;
            }
            catch (Exception exception)
            {
                transportInitialized = false;
                ReportError("Discord RPC initialization failed: " + exception.Message);
                if (double.IsPositiveInfinity(retryAt))
                {
                    ScheduleNextRetry();
                }
            }
        }

        private void TryInvoke()
        {
            try
            {
                transport.Invoke();
            }
            catch (Exception exception)
            {
                ReportError("Discord RPC callback processing failed: " + exception.Message);
                retryAttempt = 0;
                ScheduleNextRetry();
            }
        }

        private void TryClear()
        {
            try
            {
                transport.ClearPresence();
            }
            catch (Exception exception)
            {
                ReportError("Discord RPC presence could not be cleared: " + exception.Message);
            }
        }

        private void PublishDesired(bool force)
        {
            if (desiredPayload == null || (!force && desiredPayload.Equals(lastSentPayload)))
            {
                return;
            }

            try
            {
                transport.SetPresence(desiredPayload);
                lastSentPayload = desiredPayload;
                errorReported = false;
            }
            catch (Exception exception)
            {
                ReportError("Discord RPC presence update failed: " + exception.Message);
                retryAttempt = 0;
                ScheduleNextRetry();
            }
        }

        private void ScheduleNextRetry()
        {
            var index = Math.Min(retryAttempt, RetryDelays.Length - 1);
            retryAt = clock.TimeSinceStartup + RetryDelays[index];
            retryAttempt++;
        }

        private void ReportError(string message)
        {
            if (errorReported || options == null || options.LogLevel == DiscordUnityRpcLogLevel.Off)
            {
                return;
            }

            errorReported = true;
            if (errorLogger != null)
            {
                errorLogger(message);
            }
        }
    }
}
