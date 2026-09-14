using System;

namespace LStudios.DiscordUnityRpc
{
    internal static class UnityVersionAssetSelector
    {
        internal static string Select(string unityVersion)
        {
            var text = unityVersion ?? string.Empty;
            var separator = text.IndexOf('.');
            var majorText = separator < 0 ? text : text.Substring(0, separator);
            int major;

            if (!int.TryParse(majorText, out major))
            {
                return DiscordApplication.LegacyUnityLogo;
            }

            if (major >= 6000)
            {
                return DiscordApplication.Unity6Logo;
            }

            return major >= 2021
                ? DiscordApplication.ModernUnityLogo
                : DiscordApplication.LegacyUnityLogo;
        }
    }
}
