using System.Globalization;
using NUnit.Framework;

namespace LStudios.DiscordUnityRpc.Tests
{
    internal sealed class DiscordPresenceFormatterTests
    {
        [TestCase("6000.0.20f1", "unity6-logo")]
        [TestCase("2022.3.62f1", "unity-logo")]
        [TestCase("2021.1.0f1", "unity-logo")]
        [TestCase("2020.3.48f1", "unity-logo-old")]
        [TestCase("2019.4.40f1", "unity-logo-old")]
        public void SelectsExpectedLargeImage(string version, string expected)
        {
            Assert.That(UnityVersionAssetSelector.Select(version), Is.EqualTo(expected));
        }

        [Test]
        public void CompilingFormatsNeutralProjectWhenProjectNameIsHidden()
        {
            var snapshot = Snapshot(EditorActivityKind.Compiling);
            var options = new DiscordUnityRpcOptions { ShowProjectName = false };

            var payload = new DiscordPresenceFormatter().Format(snapshot, options, 1234L);

            Assert.That(payload.Details, Is.EqualTo("Working in Unity"));
            Assert.That(payload.State, Is.EqualTo("Compiling scripts"));
        }

        [Test]
        public void PrefabModeUsesVisibleProjectAndPrefabNames()
        {
            var snapshot = Snapshot(EditorActivityKind.EditingPrefab);
            var options = new DiscordUnityRpcOptions();

            var payload = new DiscordPresenceFormatter().Format(snapshot, options, 1234L);

            Assert.That(payload.Details, Is.EqualTo("Working on SecretGame"));
            Assert.That(payload.State, Is.EqualTo("Editing prefab PlayerCard"));
        }

        [Test]
        public void HiddenSceneUsesNeutralEditingState()
        {
            var snapshot = Snapshot(EditorActivityKind.EditingScene);
            var options = new DiscordUnityRpcOptions { ShowSceneName = false };

            var payload = new DiscordPresenceFormatter().Format(snapshot, options, 1234L);

            Assert.That(payload.State, Is.EqualTo("Editing a scene"));
        }

        [Test]
        public void InvalidOrNonHttpsButtonsAreOmitted()
        {
            var options = new DiscordUnityRpcOptions
            {
                ButtonOneLabel = "Repository",
                ButtonOneUrl = "http://example.com",
                ButtonTwoLabel = "L.Studios",
                ButtonTwoUrl = "https://github.com/L-Studios"
            };

            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.EditingScene), options, 1234L);

            Assert.That(payload.Buttons, Has.Length.EqualTo(1));
            Assert.That(payload.Buttons[0].Label, Is.EqualTo("L.Studios"));
            Assert.That(payload.Buttons[0].Url, Is.EqualTo("https://github.com/L-Studios"));
        }

        [Test]
        public void TruncationDoesNotSplitSurrogatePair()
        {
            var projectName = new string('a', 116) + "😀tail";
            var snapshot = new EditorContextSnapshot(
                projectName,
                "MainMenu",
                "PlayerCard",
                "6000.3.24f1",
                EditorActivityKind.EditingScene);

            var payload = new DiscordPresenceFormatter().Format(
                snapshot, new DiscordUnityRpcOptions(), 1234L);

            Assert.That(new StringInfo(payload.Details).LengthInTextElements, Is.EqualTo(128));
            Assert.That(payload.Details.EndsWith("😀"), Is.True);
        }

        [Test]
        public void HiddenElapsedTimeOmitsTimestamp()
        {
            var options = new DiscordUnityRpcOptions { ShowElapsedTime = false };

            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.Playing), options, 1234L);

            Assert.That(payload.StartTimestamp, Is.Null);
        }

        [Test]
        public void VisibleElapsedTimeKeepsSessionStart()
        {
            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.Playing), new DiscordUnityRpcOptions(), 1234L);

            Assert.That(payload.StartTimestamp, Is.EqualTo(1234L));
        }

        private static EditorContextSnapshot Snapshot(EditorActivityKind kind)
        {
            return new EditorContextSnapshot(
                "SecretGame",
                "MainMenu",
                "PlayerCard",
                "6000.3.24f1",
                kind);
        }
    }
}
