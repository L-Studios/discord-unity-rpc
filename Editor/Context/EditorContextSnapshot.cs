namespace LStudios.DiscordUnityRpc
{
    internal sealed class EditorContextSnapshot
    {
        internal EditorContextSnapshot(
            string projectName,
            string sceneName,
            string prefabName,
            string unityVersion,
            EditorActivityKind activityKind,
            EditorToolKind activeTool = EditorToolKind.None,
            EditorPlatformKind platform = EditorPlatformKind.Unknown)
        {
            ProjectName = projectName ?? string.Empty;
            SceneName = sceneName ?? string.Empty;
            PrefabName = prefabName ?? string.Empty;
            UnityVersion = unityVersion ?? string.Empty;
            ActivityKind = activityKind;
            ActiveTool = activeTool;
            Platform = platform;
        }

        internal string ProjectName { get; private set; }
        internal string SceneName { get; private set; }
        internal string PrefabName { get; private set; }
        internal string UnityVersion { get; private set; }
        internal EditorActivityKind ActivityKind { get; private set; }
        internal EditorToolKind ActiveTool { get; private set; }
        internal EditorPlatformKind Platform { get; private set; }

        internal EditorContextSnapshot WithActivityKind(EditorActivityKind activityKind)
        {
            return new EditorContextSnapshot(
                ProjectName,
                SceneName,
                PrefabName,
                UnityVersion,
                activityKind,
                ActiveTool,
                Platform);
        }
    }
}
