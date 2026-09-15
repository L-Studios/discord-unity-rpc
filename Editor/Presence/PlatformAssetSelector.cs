namespace LStudios.DiscordUnityRpc
{
    internal static class PlatformAssetSelector
    {
        internal static string Select(EditorPlatformKind platform)
        {
            switch (platform)
            {
                case EditorPlatformKind.Windows:
                    return DiscordApplication.WindowsLogo;
                case EditorPlatformKind.Linux:
                    return DiscordApplication.LinuxLogo;
                case EditorPlatformKind.Android:
                    return DiscordApplication.AndroidLogo;
                case EditorPlatformKind.IOS:
                    return DiscordApplication.IosLogo;
                case EditorPlatformKind.WebGL:
                    return DiscordApplication.WebGlLogo;
                default:
                    return string.Empty;
            }
        }
    }
}
