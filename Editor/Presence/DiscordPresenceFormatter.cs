using System;
using System.Collections.Generic;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class DiscordPresenceFormatter
    {
        private const int PresenceTextLimit = 128;
        private const int ButtonLabelLimit = 32;

        internal PresencePayload Format(
            EditorContextSnapshot snapshot,
            DiscordUnityRpcOptions options,
            long sessionStartedAtUnixSeconds)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException("snapshot");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var project = DiscordText.Normalize(snapshot.ProjectName, PresenceTextLimit);
            var scene = DiscordText.Normalize(snapshot.SceneName, PresenceTextLimit);
            var prefab = DiscordText.Normalize(snapshot.PrefabName, PresenceTextLimit);
            var details = options.ShowProjectName && project.Length > 0
                ? "Working on " + project
                : "Working in Unity";

            string state;
            switch (snapshot.ActivityKind)
            {
                case EditorActivityKind.Compiling:
                    state = "Compiling scripts";
                    break;
                case EditorActivityKind.Playing:
                    state = options.ShowSceneName && scene.Length > 0
                        ? "Testing scene " + scene
                        : "Testing a scene";
                    break;
                case EditorActivityKind.EditingPrefab:
                    state = options.ShowPrefabName && prefab.Length > 0
                        ? "Editing prefab " + prefab
                        : "Editing a prefab";
                    break;
                default:
                    state = options.ShowSceneName && scene.Length > 0
                        ? "Editing scene " + scene
                        : "Editing a scene";
                    break;
            }

            var version = DiscordText.Normalize(snapshot.UnityVersion, PresenceTextLimit);
            var imageText = version.Length == 0 ? "Unity Editor" : "Unity " + version;

            return new PresencePayload(
                DiscordText.Normalize(details, PresenceTextLimit),
                DiscordText.Normalize(state, PresenceTextLimit),
                UnityVersionAssetSelector.Select(snapshot.UnityVersion),
                DiscordText.Normalize(imageText, PresenceTextLimit),
                options.ShowElapsedTime ? (long?)sessionStartedAtUnixSeconds : null,
                CreateButtons(options));
        }

        private static PresenceButton[] CreateButtons(DiscordUnityRpcOptions options)
        {
            var buttons = new List<PresenceButton>(2);
            AddButton(buttons, options.ButtonOneLabel, options.ButtonOneUrl);
            AddButton(buttons, options.ButtonTwoLabel, options.ButtonTwoUrl);
            return buttons.ToArray();
        }

        private static void AddButton(List<PresenceButton> buttons, string label, string url)
        {
            if (buttons.Count >= 2)
            {
                return;
            }

            var normalizedLabel = DiscordText.Normalize(label, ButtonLabelLimit);
            Uri parsed;
            if (normalizedLabel.Length == 0
                || !Uri.TryCreate(url, UriKind.Absolute, out parsed)
                || !parsed.IsAbsoluteUri
                || !string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            buttons.Add(new PresenceButton(normalizedLabel, parsed.AbsoluteUri));
        }
    }
}
