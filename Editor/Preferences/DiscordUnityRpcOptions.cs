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
        internal DiscordUnityRpcOptions()
        {
            ShowProjectName = true;
            ShowSceneName = true;
            ShowPrefabName = true;
            ShowElapsedTime = true;
            LogLevel = DiscordUnityRpcLogLevel.Errors;
            ButtonOneLabel = string.Empty;
            ButtonOneUrl = string.Empty;
            ButtonTwoLabel = string.Empty;
            ButtonTwoUrl = string.Empty;
        }

        internal bool Enabled { get; set; }
        internal bool ShowProjectName { get; set; }
        internal bool ShowSceneName { get; set; }
        internal bool ShowPrefabName { get; set; }
        internal bool ShowElapsedTime { get; set; }
        internal DiscordUnityRpcLogLevel LogLevel { get; set; }
        internal string ButtonOneLabel { get; set; }
        internal string ButtonOneUrl { get; set; }
        internal string ButtonTwoLabel { get; set; }
        internal string ButtonTwoUrl { get; set; }

        internal DiscordUnityRpcOptions Copy()
        {
            return new DiscordUnityRpcOptions
            {
                Enabled = Enabled,
                ShowProjectName = ShowProjectName,
                ShowSceneName = ShowSceneName,
                ShowPrefabName = ShowPrefabName,
                ShowElapsedTime = ShowElapsedTime,
                LogLevel = LogLevel,
                ButtonOneLabel = ButtonOneLabel ?? string.Empty,
                ButtonOneUrl = ButtonOneUrl ?? string.Empty,
                ButtonTwoLabel = ButtonTwoLabel ?? string.Empty,
                ButtonTwoUrl = ButtonTwoUrl ?? string.Empty
            };
        }
    }
}
