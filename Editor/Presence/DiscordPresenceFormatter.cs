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
            var toolState = options.ShowActiveTool ? FormatTool(snapshot.ActiveTool) : string.Empty;
            var platformLabel = EditorPlatformResolver.GetLabel(snapshot.Platform);

            string state;
            switch (snapshot.ActivityKind)
            {
                case EditorActivityKind.Building:
                    state = platformLabel.Length > 0
                        ? "Building for " + platformLabel
                        : "Building a player";
                    break;
                case EditorActivityKind.Compiling:
                    state = "Compiling scripts";
                    break;
                case EditorActivityKind.Idle:
                    state = "Idle";
                    break;
                case EditorActivityKind.Playing:
                    state = options.ShowSceneName && scene.Length > 0
                        ? "Testing scene " + scene
                        : "Testing a scene";
                    break;
                case EditorActivityKind.EditingPrefab:
                    if (toolState.Length > 0)
                    {
                        state = toolState;
                    }
                    else
                    {
                        state = options.ShowPrefabName && prefab.Length > 0
                            ? "Editing prefab " + prefab
                            : "Editing a prefab";
                    }
                    break;
                default:
                    if (toolState.Length > 0)
                    {
                        state = toolState;
                    }
                    else
                    {
                        state = options.ShowSceneName && scene.Length > 0
                            ? "Editing scene " + scene
                            : "Editing a scene";
                    }
                    break;
            }

            var version = DiscordText.Normalize(snapshot.UnityVersion, PresenceTextLimit);
            var imageText = version.Length == 0 ? "Unity Editor" : "Unity " + version;
            var smallImageKey = options.ShowBuildTarget
                ? PlatformAssetSelector.Select(snapshot.Platform)
                : string.Empty;
            var smallImageText = smallImageKey.Length == 0
                ? string.Empty
                : "Build target: " + platformLabel;

            return new PresencePayload(
                DiscordText.Normalize(details, PresenceTextLimit),
                DiscordText.Normalize(state, PresenceTextLimit),
                UnityVersionAssetSelector.Select(snapshot.UnityVersion),
                DiscordText.Normalize(imageText, PresenceTextLimit),
                smallImageKey,
                smallImageText,
                options.ShowElapsedTime ? (long?)sessionStartedAtUnixSeconds : null,
                CreateButtons(options));
        }

        private static string FormatTool(EditorToolKind tool)
        {
            switch (tool)
            {
                case EditorToolKind.Animator:
                    return "Editing an Animator";
                case EditorToolKind.Animation:
                    return "Animating";
                case EditorToolKind.Timeline:
                    return "Editing a Timeline";
                case EditorToolKind.ShaderGraph:
                    return "Editing a Shader Graph";
                case EditorToolKind.VfxGraph:
                    return "Editing a VFX Graph";
                case EditorToolKind.TilePalette:
                    return "Painting tilemaps";
                case EditorToolKind.Terrain:
                    return "Editing terrain";
                case EditorToolKind.Profiler:
                    return "Profiling";
                case EditorToolKind.SpriteEditor:
                    return "Editing sprites";
                case EditorToolKind.UIBuilder:
                    return "Designing UI";
                default:
                    return string.Empty;
            }
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
