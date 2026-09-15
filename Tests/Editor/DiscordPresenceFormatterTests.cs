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

        [TestCase(EditorPlatformKind.Android, "Building for Android")]
        [TestCase(EditorPlatformKind.IOS, "Building for iOS")]
        [TestCase(EditorPlatformKind.Other, "Building a player")]
        public void BuildingStateNamesTargetPlatform(EditorPlatformKind platform, string expected)
        {
            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.Building, EditorToolKind.Animator, platform),
                new DiscordUnityRpcOptions(),
                1234L);

            Assert.That(payload.State, Is.EqualTo(expected));
        }

        [Test]
        public void IdleStateKeepsProjectDetails()
        {
            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.Idle), new DiscordUnityRpcOptions(), 1234L);

            Assert.That(payload.Details, Is.EqualTo("Working on SecretGame"));
            Assert.That(payload.State, Is.EqualTo("Idle"));
        }

        [TestCase(EditorToolKind.Animator, "Editing an Animator")]
        [TestCase(EditorToolKind.Animation, "Animating")]
        [TestCase(EditorToolKind.Timeline, "Editing a Timeline")]
        [TestCase(EditorToolKind.ShaderGraph, "Editing a Shader Graph")]
        [TestCase(EditorToolKind.VfxGraph, "Editing a VFX Graph")]
        [TestCase(EditorToolKind.TilePalette, "Painting tilemaps")]
        [TestCase(EditorToolKind.Terrain, "Editing terrain")]
        [TestCase(EditorToolKind.Profiler, "Profiling")]
        [TestCase(EditorToolKind.SpriteEditor, "Editing sprites")]
        [TestCase(EditorToolKind.UIBuilder, "Designing UI")]
        public void ActiveToolReplacesEditingState(EditorToolKind tool, string expected)
        {
            var formatter = new DiscordPresenceFormatter();
            var options = new DiscordUnityRpcOptions();

            Assert.That(
                formatter.Format(Snapshot(EditorActivityKind.EditingScene, tool), options, 1234L).State,
                Is.EqualTo(expected));
            Assert.That(
                formatter.Format(Snapshot(EditorActivityKind.EditingPrefab, tool), options, 1234L).State,
                Is.EqualTo(expected));
        }

        [Test]
        public void HiddenActiveToolKeepsSceneState()
        {
            var options = new DiscordUnityRpcOptions { ShowActiveTool = false };

            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.EditingScene, EditorToolKind.Timeline), options, 1234L);

            Assert.That(payload.State, Is.EqualTo("Editing scene MainMenu"));
        }

        [Test]
        public void PlayModeIgnoresActiveTool()
        {
            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.Playing, EditorToolKind.Profiler), new DiscordUnityRpcOptions(), 1234L);

            Assert.That(payload.State, Is.EqualTo("Testing scene MainMenu"));
        }

        [TestCase(EditorPlatformKind.Windows, "windows-logo", "Build target: Windows")]
        [TestCase(EditorPlatformKind.Linux, "linux-logo", "Build target: Linux")]
        [TestCase(EditorPlatformKind.Android, "android-logo", "Build target: Android")]
        [TestCase(EditorPlatformKind.IOS, "ios-logo", "Build target: iOS")]
        [TestCase(EditorPlatformKind.WebGL, "webgl-logo", "Build target: WebGL")]
        [TestCase(EditorPlatformKind.MacOS, "", "")]
        [TestCase(EditorPlatformKind.Other, "", "")]
        [TestCase(EditorPlatformKind.Unknown, "", "")]
        public void BuildTargetSelectsSmallImage(EditorPlatformKind platform, string key, string text)
        {
            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.EditingScene, EditorToolKind.None, platform),
                new DiscordUnityRpcOptions(),
                1234L);

            Assert.That(payload.SmallImageKey, Is.EqualTo(key));
            Assert.That(payload.SmallImageText, Is.EqualTo(text));
        }

        [Test]
        public void HiddenBuildTargetOmitsSmallImage()
        {
            var options = new DiscordUnityRpcOptions { ShowBuildTarget = false };

            var payload = new DiscordPresenceFormatter().Format(
                Snapshot(EditorActivityKind.EditingScene, EditorToolKind.None, EditorPlatformKind.Android),
                options,
                1234L);

            Assert.That(payload.SmallImageKey, Is.Empty);
            Assert.That(payload.SmallImageText, Is.Empty);
        }

        [Test]
        public void MapsSmallImageToDiscordRpc()
        {
            var payload = new PresencePayload(
                "Details", "State", "unity6-logo", "Unity", "android-logo", "Build target: Android", null, null);

            var presence = DiscordRpcTransport.CreateRichPresence(payload);

            Assert.That(presence.Assets.SmallImageKey, Is.EqualTo("android-logo"));
            Assert.That(presence.Assets.SmallImageText, Is.EqualTo("Build target: Android"));
        }

        [Test]
        public void MapsTransportNeutralPayloadToDiscordRpc()
        {
            var payload = new PresencePayload(
                "Working on SecretGame",
                "Editing scene MainMenu",
                "unity6-logo",
                "Unity 6000.3.24f1",
                string.Empty,
                string.Empty,
                1234L,
                new[] { new PresenceButton("Repository", "https://github.com/L-Studios") });

            var presence = DiscordRpcTransport.CreateRichPresence(payload);

            Assert.That(presence.Details, Is.EqualTo(payload.Details));
            Assert.That(presence.State, Is.EqualTo(payload.State));
            Assert.That(presence.Assets.LargeImageKey, Is.EqualTo("unity6-logo"));
            Assert.That(presence.Assets.LargeImageText, Is.EqualTo("Unity 6000.3.24f1"));
            Assert.That(presence.Assets.SmallImageKey, Is.Null.Or.Empty);
            Assert.That(presence.Timestamps.Start.Value.Kind, Is.EqualTo(System.DateTimeKind.Utc));
            Assert.That(presence.Buttons, Has.Length.EqualTo(1));
            Assert.That(presence.Buttons[0].Label, Is.EqualTo("Repository"));
            Assert.That(presence.Buttons[0].Url, Is.EqualTo("https://github.com/L-Studios"));
        }

        [Test]
        public void OmittedTimestampDoesNotCreateDiscordTimestamps()
        {
            var payload = new PresencePayload("Details", "State", "unity-logo", "Unity", null, null, null, null);

            var presence = DiscordRpcTransport.CreateRichPresence(payload);

            Assert.That(presence.Timestamps, Is.Null);
        }

        private static EditorContextSnapshot Snapshot(
            EditorActivityKind kind,
            EditorToolKind tool = EditorToolKind.None,
            EditorPlatformKind platform = EditorPlatformKind.Unknown)
        {
            return new EditorContextSnapshot(
                "SecretGame",
                "MainMenu",
                "PlayerCard",
                "6000.3.24f1",
                kind,
                tool,
                platform);
        }
    }
}
