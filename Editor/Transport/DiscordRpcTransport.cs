using System;
using DiscordRPC;
using DiscordRPC.Message;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class DiscordRpcTransport : IDiscordRpcTransport
    {
        private static readonly DateTime UnixEpochUtc =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private DiscordRpcClient client;
        private bool disposed;

        public bool IsConnected { get; private set; }
        public event Action Connected;
        public event Action Disconnected;

        public void Initialize(string applicationId)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (client != null && client.IsInitialized && IsConnected)
            {
                return;
            }

            DisposeClient();
            client = new DiscordRpcClient(applicationId, -1, null, false, null);
            client.SkipIdenticalPresence = true;
            client.OnReady += OnReady;
            client.OnClose += OnClose;
            client.OnConnectionEstablished += OnConnectionEstablished;
            client.OnConnectionFailed += OnConnectionFailed;
            client.Initialize();
        }

        public void Invoke()
        {
            if (client != null && client.IsInitialized)
            {
                client.Invoke();
            }
        }

        public void SetPresence(PresencePayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            if (client == null || !client.IsInitialized)
            {
                throw new InvalidOperationException("Discord RPC has not been initialized.");
            }

            client.SetPresence(CreateRichPresence(payload));
        }

        public void ClearPresence()
        {
            if (client != null && client.IsInitialized)
            {
                client.ClearPresence();
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            DisposeClient();
            Connected = null;
            Disconnected = null;
        }

        internal static RichPresence CreateRichPresence(PresencePayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            var presence = new RichPresence
            {
                Details = payload.Details,
                State = payload.State,
                Assets = new Assets
                {
                    LargeImageKey = payload.LargeImageKey,
                    LargeImageText = payload.LargeImageText
                }
            };

            if (payload.StartTimestamp.HasValue)
            {
                presence.Timestamps = new Timestamps
                {
                    Start = UnixEpochUtc.AddSeconds(payload.StartTimestamp.Value)
                };
            }

            var sourceButtons = payload.Buttons;
            if (sourceButtons.Length > 0)
            {
                var buttons = new Button[sourceButtons.Length];
                for (var index = 0; index < sourceButtons.Length; index++)
                {
                    buttons[index] = new Button
                    {
                        Label = sourceButtons[index].Label,
                        Url = sourceButtons[index].Url
                    };
                }

                presence.Buttons = buttons;
            }

            return presence;
        }

        private void OnReady(object sender, ReadyMessage message)
        {
            RaiseConnected();
        }

        private void OnConnectionEstablished(object sender, ConnectionEstablishedMessage message)
        {
            RaiseConnected();
        }

        private void OnClose(object sender, CloseMessage message)
        {
            RaiseDisconnected();
        }

        private void OnConnectionFailed(object sender, ConnectionFailedMessage message)
        {
            RaiseDisconnected();
        }

        private void RaiseConnected()
        {
            if (IsConnected)
            {
                return;
            }

            IsConnected = true;
            var handler = Connected;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseDisconnected()
        {
            if (!IsConnected && client == null)
            {
                return;
            }

            IsConnected = false;
            var handler = Disconnected;
            if (handler != null)
            {
                handler();
            }
        }

        private void DisposeClient()
        {
            IsConnected = false;
            if (client == null)
            {
                return;
            }

            client.OnReady -= OnReady;
            client.OnClose -= OnClose;
            client.OnConnectionEstablished -= OnConnectionEstablished;
            client.OnConnectionFailed -= OnConnectionFailed;
            client.Dispose();
            client = null;
        }
    }
}
