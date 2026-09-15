using UnityEditor;

namespace LStudios.DiscordUnityRpc
{
    internal static class EditorPlatformResolver
    {
        internal static EditorPlatformKind FromBuildTarget(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return EditorPlatformKind.Windows;
                case BuildTarget.StandaloneOSX:
                    return EditorPlatformKind.MacOS;
                case BuildTarget.StandaloneLinux64:
                    return EditorPlatformKind.Linux;
                case BuildTarget.Android:
                    return EditorPlatformKind.Android;
                case BuildTarget.iOS:
                    return EditorPlatformKind.IOS;
                case BuildTarget.WebGL:
                    return EditorPlatformKind.WebGL;
                case BuildTarget.NoTarget:
                    return EditorPlatformKind.Unknown;
                default:
                    return EditorPlatformKind.Other;
            }
        }

        internal static string GetLabel(EditorPlatformKind platform)
        {
            switch (platform)
            {
                case EditorPlatformKind.Windows:
                    return "Windows";
                case EditorPlatformKind.MacOS:
                    return "macOS";
                case EditorPlatformKind.Linux:
                    return "Linux";
                case EditorPlatformKind.Android:
                    return "Android";
                case EditorPlatformKind.IOS:
                    return "iOS";
                case EditorPlatformKind.WebGL:
                    return "WebGL";
                default:
                    return string.Empty;
            }
        }
    }
}
