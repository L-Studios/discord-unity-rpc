using System;
using System.Collections.Generic;

namespace LStudios.DiscordUnityRpc
{
    internal static class EditorToolResolver
    {
        // Matched by full type name so optional packages (Timeline, Shader Graph, VFX Graph...)
        // never become compile-time dependencies.
        private static readonly Dictionary<string, EditorToolKind> KnownTools =
            new Dictionary<string, EditorToolKind>(StringComparer.Ordinal)
            {
                { "UnityEditor.Graphs.AnimatorControllerTool", EditorToolKind.Animator },
                { "UnityEditor.AnimationWindow", EditorToolKind.Animation },
                { "UnityEditor.Timeline.TimelineWindow", EditorToolKind.Timeline },
                { "UnityEditor.ShaderGraph.Drawing.MaterialGraphEditWindow", EditorToolKind.ShaderGraph },
                { "UnityEditor.VFX.UI.VFXViewWindow", EditorToolKind.VfxGraph },
                { "UnityEditor.Tilemaps.GridPaintPaletteWindow", EditorToolKind.TilePalette },
                { "UnityEditor.ProfilerWindow", EditorToolKind.Profiler },
                { "UnityEditor.U2D.Sprites.SpriteEditorWindow", EditorToolKind.SpriteEditor },
                { "Unity.UI.Builder.Builder", EditorToolKind.UIBuilder }
            };

        private const string SceneViewType = "UnityEditor.SceneView";
        private const string GameViewType = "UnityEditor.GameView";

        /// <summary>
        /// Only known tools and the Scene/Game views change the result. Utility windows such as
        /// the Inspector, Hierarchy, or Console keep the previous tool so presence does not flicker
        /// while the user clicks around.
        /// </summary>
        internal static EditorToolKind Resolve(
            string focusedWindowTypeName,
            bool terrainSelected,
            EditorToolKind previous)
        {
            if (string.IsNullOrEmpty(focusedWindowTypeName))
            {
                return previous;
            }

            EditorToolKind tool;
            if (KnownTools.TryGetValue(focusedWindowTypeName, out tool))
            {
                return tool;
            }

            if (string.Equals(focusedWindowTypeName, SceneViewType, StringComparison.Ordinal))
            {
                return terrainSelected ? EditorToolKind.Terrain : EditorToolKind.None;
            }

            return string.Equals(focusedWindowTypeName, GameViewType, StringComparison.Ordinal)
                ? EditorToolKind.None
                : previous;
        }
    }
}
