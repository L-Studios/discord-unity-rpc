using NUnit.Framework;
using UnityEditor;

namespace LStudios.DiscordUnityRpc.Tests
{
    internal sealed class EditorActivityContextTests
    {
        [TestCase("UnityEditor.Graphs.AnimatorControllerTool", EditorToolKind.Animator)]
        [TestCase("UnityEditor.AnimationWindow", EditorToolKind.Animation)]
        [TestCase("UnityEditor.Timeline.TimelineWindow", EditorToolKind.Timeline)]
        [TestCase("UnityEditor.ShaderGraph.Drawing.MaterialGraphEditWindow", EditorToolKind.ShaderGraph)]
        [TestCase("UnityEditor.VFX.UI.VFXViewWindow", EditorToolKind.VfxGraph)]
        [TestCase("UnityEditor.Tilemaps.GridPaletteWindow", EditorToolKind.TilePalette)]
        [TestCase("UnityEditor.ProfilerWindow", EditorToolKind.Profiler)]
        [TestCase("UnityEditor.U2D.Sprites.SpriteEditorWindow", EditorToolKind.SpriteEditor)]
        [TestCase("Unity.UI.Builder.Builder", EditorToolKind.UIBuilder)]
        public void KnownToolWindowSelectsTool(string typeName, EditorToolKind expected)
        {
            Assert.That(EditorToolResolver.Resolve(typeName, false, EditorToolKind.None), Is.EqualTo(expected));
        }

        [Test]
        public void SceneViewClearsToolUnlessTerrainIsSelected()
        {
            Assert.That(
                EditorToolResolver.Resolve("UnityEditor.SceneView", false, EditorToolKind.Animator),
                Is.EqualTo(EditorToolKind.None));
            Assert.That(
                EditorToolResolver.Resolve("UnityEditor.SceneView", true, EditorToolKind.Animator),
                Is.EqualTo(EditorToolKind.Terrain));
        }

        [Test]
        public void GameViewClearsTool()
        {
            Assert.That(
                EditorToolResolver.Resolve("UnityEditor.GameView", true, EditorToolKind.Timeline),
                Is.EqualTo(EditorToolKind.None));
        }

        [TestCase("UnityEditor.InspectorWindow")]
        [TestCase("UnityEditor.SceneHierarchyWindow")]
        [TestCase("")]
        [TestCase(null)]
        public void UtilityOrMissingWindowKeepsPreviousTool(string typeName)
        {
            Assert.That(
                EditorToolResolver.Resolve(typeName, false, EditorToolKind.Timeline),
                Is.EqualTo(EditorToolKind.Timeline));
        }

        [TestCase(BuildTarget.StandaloneWindows, EditorPlatformKind.Windows)]
        [TestCase(BuildTarget.StandaloneWindows64, EditorPlatformKind.Windows)]
        [TestCase(BuildTarget.StandaloneOSX, EditorPlatformKind.MacOS)]
        [TestCase(BuildTarget.StandaloneLinux64, EditorPlatformKind.Linux)]
        [TestCase(BuildTarget.Android, EditorPlatformKind.Android)]
        [TestCase(BuildTarget.iOS, EditorPlatformKind.IOS)]
        [TestCase(BuildTarget.WebGL, EditorPlatformKind.WebGL)]
        [TestCase(BuildTarget.tvOS, EditorPlatformKind.Other)]
        [TestCase(BuildTarget.NoTarget, EditorPlatformKind.Unknown)]
        public void MapsBuildTargetToPlatform(BuildTarget target, EditorPlatformKind expected)
        {
            Assert.That(EditorPlatformResolver.FromBuildTarget(target), Is.EqualTo(expected));
        }

        [Test]
        public void BuildMonitorEndRaisesChangedOnlyWhileBuilding()
        {
            var calls = 0;
            System.Action handler = delegate { calls++; };
            BuildActivityMonitor.Changed += handler;
            try
            {
                BuildActivityMonitor.Begin(EditorPlatformKind.Android);
                Assert.That(BuildActivityMonitor.IsBuilding, Is.True);
                Assert.That(BuildActivityMonitor.BuildPlatform, Is.EqualTo(EditorPlatformKind.Android));

                BuildActivityMonitor.End();
                BuildActivityMonitor.End();

                Assert.That(BuildActivityMonitor.IsBuilding, Is.False);
                Assert.That(calls, Is.EqualTo(2));
            }
            finally
            {
                BuildActivityMonitor.Changed -= handler;
                BuildActivityMonitor.End();
            }
        }
    }
}
