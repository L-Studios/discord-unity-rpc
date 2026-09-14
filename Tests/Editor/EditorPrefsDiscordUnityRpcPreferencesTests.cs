using System.Collections.Generic;
using NUnit.Framework;

namespace LStudios.DiscordUnityRpc.Tests
{
    internal sealed class EditorPrefsDiscordUnityRpcPreferencesTests
    {
        [Test]
        public void NewProjectDefaultsToDisabledAndPrivacyFieldsSelected()
        {
            var preferences = new EditorPrefsDiscordUnityRpcPreferences(
                "C:/Projects/One",
                new FakeKeyValueStore());

            var options = preferences.Current;

            Assert.That(options.Enabled, Is.False);
            Assert.That(options.ShowProjectName, Is.True);
            Assert.That(options.ShowSceneName, Is.True);
            Assert.That(options.ShowPrefabName, Is.True);
            Assert.That(options.ShowElapsedTime, Is.True);
            Assert.That(options.LogLevel, Is.EqualTo(DiscordUnityRpcLogLevel.Errors));
        }

        [Test]
        public void EnablingOneProjectDoesNotEnableAnotherProject()
        {
            var store = new FakeKeyValueStore();
            var first = new EditorPrefsDiscordUnityRpcPreferences("C:/Projects/One/", store);
            var second = new EditorPrefsDiscordUnityRpcPreferences("C:/Projects/Two", store);

            var enabled = first.Current;
            enabled.Enabled = true;
            first.Save(enabled);

            Assert.That(first.Current.Enabled, Is.True);
            Assert.That(second.Current.Enabled, Is.False);
            Assert.That(first.KeyPrefix, Is.Not.EqualTo(second.KeyPrefix));
            Assert.That(first.KeyPrefix, Does.Not.Contain("Projects"));
        }

        [Test]
        public void SavingRaisesChangedExactlyOnce()
        {
            var preferences = new EditorPrefsDiscordUnityRpcPreferences(
                "C:/Projects/One",
                new FakeKeyValueStore());
            var calls = 0;
            preferences.Changed += delegate { calls++; };

            preferences.Save(preferences.Current);

            Assert.That(calls, Is.EqualTo(1));
        }

        private sealed class FakeKeyValueStore : IKeyValueStore
        {
            private readonly Dictionary<string, object> values = new Dictionary<string, object>();

            public bool GetBool(string key, bool defaultValue)
            {
                object value;
                return values.TryGetValue(key, out value) ? (bool)value : defaultValue;
            }

            public int GetInt(string key, int defaultValue)
            {
                object value;
                return values.TryGetValue(key, out value) ? (int)value : defaultValue;
            }

            public string GetString(string key, string defaultValue)
            {
                object value;
                return values.TryGetValue(key, out value) ? (string)value : defaultValue;
            }

            public void SetBool(string key, bool value)
            {
                values[key] = value;
            }

            public void SetInt(string key, int value)
            {
                values[key] = value;
            }

            public void SetString(string key, string value)
            {
                values[key] = value;
            }
        }
    }
}
