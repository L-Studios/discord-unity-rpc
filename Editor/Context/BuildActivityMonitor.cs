using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class BuildActivityMonitor :
        IPreprocessBuildWithReport,
        IPostprocessBuildWithReport,
        IActiveBuildTargetChanged
    {
        internal static event Action Changed;

        internal static bool IsBuilding { get; private set; }
        internal static EditorPlatformKind BuildPlatform { get; private set; }

        // Run before other preprocessors so presence switches to "Building" as early as possible.
        public int callbackOrder { get { return int.MinValue; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            Begin(EditorPlatformResolver.FromBuildTarget(report.summary.platform));
        }

        // Not called for failed or cancelled builds; UnityEditorContextTracker ends those by polling.
        public void OnPostprocessBuild(BuildReport report)
        {
            End();
        }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            RaiseChanged();
        }

        internal static void Begin(EditorPlatformKind platform)
        {
            IsBuilding = true;
            BuildPlatform = platform;
            RaiseChanged();
        }

        internal static void End()
        {
            if (!IsBuilding)
            {
                return;
            }

            IsBuilding = false;
            BuildPlatform = EditorPlatformKind.Unknown;
            RaiseChanged();
        }

        private static void RaiseChanged()
        {
            var handler = Changed;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
