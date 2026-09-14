using NUnit.Framework;

namespace LStudios.DiscordUnityRpc.Tests
{
    internal sealed class PrefabStageCompatibilityTests
    {
        [TestCase(true, true, true, EditorActivityKind.Compiling)]
        [TestCase(false, true, true, EditorActivityKind.Playing)]
        [TestCase(false, false, true, EditorActivityKind.EditingPrefab)]
        [TestCase(false, false, false, EditorActivityKind.EditingScene)]
        public void ResolvesExpectedActivityPriority(
            bool compiling,
            bool playing,
            bool prefabOpen,
            EditorActivityKind expected)
        {
            Assert.That(
                UnityEditorContextTracker.ResolveActivityKind(compiling, playing, prefabOpen),
                Is.EqualTo(expected));
        }

        [Test]
        public void PrefabNameIsEmptyOutsidePrefabMode()
        {
            Assert.That(PrefabStageCompatibility.GetCurrentPrefabName(), Is.EqualTo(string.Empty));
        }
    }
}
