namespace LStudios.DiscordUnityRpc
{
    internal sealed class EditorContextSnapshot
    {
        internal EditorContextSnapshot(
            string projectName,
            string sceneName,
            string prefabName,
            string unityVersion,
            EditorActivityKind activityKind)
        {
            ProjectName = projectName ?? string.Empty;
            SceneName = sceneName ?? string.Empty;
            PrefabName = prefabName ?? string.Empty;
            UnityVersion = unityVersion ?? string.Empty;
            ActivityKind = activityKind;
        }

        internal string ProjectName { get; private set; }
        internal string SceneName { get; private set; }
        internal string PrefabName { get; private set; }
        internal string UnityVersion { get; private set; }
        internal EditorActivityKind ActivityKind { get; private set; }
    }
}
