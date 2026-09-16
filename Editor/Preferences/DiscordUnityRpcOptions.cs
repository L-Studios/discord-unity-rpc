using System;

namespace LStudios.DiscordUnityRpc
{
    internal enum DiscordUnityRpcLogLevel
    {
        Off,
        Errors,
        Verbose
    }

    internal sealed class DiscordUnityRpcOptions
    {
        internal const int DefaultIdleTimeoutMinutes = 5;
        internal const int MaxIdleTimeoutMinutes = 120;
        internal const string DefaultButtonLabel = "Get Unity Rich Presence";
        internal const string DefaultButtonUrl = "https://github.com/L-Studios/discord-unity-rpc";

        internal DiscordUnityRpcOptions()
        {
            ShowProjectName = true;
            ShowSceneName = true;
            ShowPrefabName = true;
            ShowElapsedTime = true;
            ShowActiveTool = true;
            ShowBuildTarget = true;
            IdleTimeoutMinutes = DefaultIdleTimeoutMinutes;
            LogLevel = DiscordUnityRpcLogLevel.Errors;
            ButtonOneLabel = DefaultButtonLabel;
            ButtonOneUrl = DefaultButtonUrl;
            ButtonTwoLabel = string.Empty;
            ButtonTwoUrl = string.Empty;
        }

        internal bool Enabled { get; set; }
        internal bool ShowProjectName { get; set; }
        internal bool ShowSceneName { get; set; }
        internal bool ShowPrefabName { get; set; }
        internal bool ShowElapsedTime { get; set; }
        internal bool ShowActiveTool { get; set; }
        internal bool ShowBuildTarget { get; set; }

        /// <summary>Minutes Unity must stay unfocused before presence shows Idle; 0 disables it.</summary>
        internal int IdleTimeoutMinutes { get; set; }

        internal DiscordUnityRpcLogLevel LogLevel { get; set; }
        internal string ButtonOneLabel { get; set; }
        internal string ButtonOneUrl { get; set; }
        internal string ButtonTwoLabel { get; set; }
        internal string ButtonTwoUrl { get; set; }

        internal static int ClampIdleTimeout(int minutes)
        {
            return Math.Max(0, Math.Min(MaxIdleTimeoutMinutes, minutes));
        }

        internal DiscordUnityRpcOptions Copy()
        {
            return new DiscordUnityRpcOptions
            {
                Enabled = Enabled,
                ShowProjectName = ShowProjectName,
                ShowSceneName = ShowSceneName,
                ShowPrefabName = ShowPrefabName,
                ShowElapsedTime = ShowElapsedTime,
                ShowActiveTool = ShowActiveTool,
                ShowBuildTarget = ShowBuildTarget,
                IdleTimeoutMinutes = IdleTimeoutMinutes,
                LogLevel = LogLevel,
                ButtonOneLabel = ButtonOneLabel ?? string.Empty,
                ButtonOneUrl = ButtonOneUrl ?? string.Empty,
                ButtonTwoLabel = ButtonTwoLabel ?? string.Empty,
                ButtonTwoUrl = ButtonTwoUrl ?? string.Empty
            };
        }
    }
}
