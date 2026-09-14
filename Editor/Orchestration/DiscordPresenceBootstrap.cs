using UnityEditor;
using UnityEngine;

namespace LStudios.DiscordUnityRpc
{
    [InitializeOnLoad]
    internal static class DiscordPresenceBootstrap
    {
        private static DiscordPresenceController controller;
        private static DiscordPresenceLifetime lifetime;
        private static IDiscordUnityRpcPreferences preferences;
        private static bool shuttingDown;

        static DiscordPresenceBootstrap()
        {
            EditorApplication.delayCall += Initialize;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
            DiscordUnityRpcSettingsProvider.ClearPresenceRequested += ClearNow;
            DiscordUnityRpcSettingsProvider.ConnectionStatusProvider = GetConnectionStatus;
        }

        internal static void ClearNow()
        {
            if (controller != null)
            {
                controller.ClearNow();
            }
        }

        private static void Initialize()
        {
            if (shuttingDown)
            {
                return;
            }

            preferences = DiscordUnityRpcSettingsProvider.Preferences;
            preferences.Changed -= ScheduleRebuild;
            preferences.Changed += ScheduleRebuild;
            Rebuild();
        }

        private static void ScheduleRebuild()
        {
            if (shuttingDown || preferences == null || preferences.Current.Enabled)
            {
                return;
            }

            EditorApplication.delayCall -= Rebuild;
            EditorApplication.delayCall += Rebuild;
        }

        private static void Rebuild()
        {
            EditorApplication.delayCall -= Rebuild;
            CleanupController();

            var context = new UnityEditorContextTracker();
            var transport = new DiscordRpcTransport();
            controller = new DiscordPresenceController(
                context,
                preferences,
                new DiscordPresenceFormatter(),
                transport,
                new UnityEditorClock(),
                LogError);
            lifetime = new DiscordPresenceLifetime(controller.Dispose);
            EditorApplication.update += Tick;
            controller.Start();
        }

        private static void Tick()
        {
            if (controller != null)
            {
                controller.Tick();
            }
        }

        private static string GetConnectionStatus()
        {
            return controller == null ? "Not initialized" : controller.ConnectionStatus;
        }

        private static void Shutdown()
        {
            if (shuttingDown)
            {
                return;
            }

            shuttingDown = true;
            EditorApplication.delayCall -= Initialize;
            EditorApplication.delayCall -= Rebuild;
            AssemblyReloadEvents.beforeAssemblyReload -= Shutdown;
            EditorApplication.quitting -= Shutdown;
            DiscordUnityRpcSettingsProvider.ClearPresenceRequested -= ClearNow;
            DiscordUnityRpcSettingsProvider.ConnectionStatusProvider = null;

            if (preferences != null)
            {
                preferences.Changed -= ScheduleRebuild;
            }

            CleanupController();
        }

        private static void CleanupController()
        {
            EditorApplication.update -= Tick;
            if (lifetime != null)
            {
                lifetime.Cleanup();
            }

            lifetime = null;
            controller = null;
        }

        private static void LogError(string message)
        {
            Debug.LogError("[L.Studios Discord Unity RPC] " + message);
        }
    }
}
