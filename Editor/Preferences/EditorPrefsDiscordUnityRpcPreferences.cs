using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class EditorPrefsDiscordUnityRpcPreferences : IDiscordUnityRpcPreferences
    {
        private readonly IKeyValueStore store;

        internal EditorPrefsDiscordUnityRpcPreferences(string projectRoot)
            : this(projectRoot, new UnityEditorPrefsStore())
        {
        }

        internal EditorPrefsDiscordUnityRpcPreferences(string projectRoot, IKeyValueStore store)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new ArgumentException("A project root is required.", "projectRoot");
            }

            this.store = store ?? throw new ArgumentNullException("store");
            KeyPrefix = "com.lstudios.discord-unity-rpc." + HashNormalizedPath(projectRoot) + ".";
        }

        public event Action Changed;

        internal string KeyPrefix { get; private set; }

        public DiscordUnityRpcOptions Current
        {
            get
            {
                return new DiscordUnityRpcOptions
                {
                    Enabled = store.GetBool(Key("enabled"), false),
                    ShowProjectName = store.GetBool(Key("show-project"), true),
                    ShowSceneName = store.GetBool(Key("show-scene"), true),
                    ShowPrefabName = store.GetBool(Key("show-prefab"), true),
                    ShowElapsedTime = store.GetBool(Key("show-elapsed"), true),
                    ShowActiveTool = store.GetBool(Key("show-active-tool"), true),
                    ShowBuildTarget = store.GetBool(Key("show-build-target"), true),
                    IdleTimeoutMinutes = DiscordUnityRpcOptions.ClampIdleTimeout(
                        store.GetInt(Key("idle-timeout-minutes"), DiscordUnityRpcOptions.DefaultIdleTimeoutMinutes)),
                    LogLevel = ReadLogLevel(),
                    ButtonOneLabel = store.GetString(Key("button-1-label"), string.Empty),
                    ButtonOneUrl = store.GetString(Key("button-1-url"), string.Empty),
                    ButtonTwoLabel = store.GetString(Key("button-2-label"), string.Empty),
                    ButtonTwoUrl = store.GetString(Key("button-2-url"), string.Empty)
                };
            }
        }

        public void Save(DiscordUnityRpcOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            store.SetBool(Key("enabled"), options.Enabled);
            store.SetBool(Key("show-project"), options.ShowProjectName);
            store.SetBool(Key("show-scene"), options.ShowSceneName);
            store.SetBool(Key("show-prefab"), options.ShowPrefabName);
            store.SetBool(Key("show-elapsed"), options.ShowElapsedTime);
            store.SetBool(Key("show-active-tool"), options.ShowActiveTool);
            store.SetBool(Key("show-build-target"), options.ShowBuildTarget);
            store.SetInt(Key("idle-timeout-minutes"), DiscordUnityRpcOptions.ClampIdleTimeout(options.IdleTimeoutMinutes));
            store.SetInt(Key("log-level"), (int)options.LogLevel);
            store.SetString(Key("button-1-label"), options.ButtonOneLabel ?? string.Empty);
            store.SetString(Key("button-1-url"), options.ButtonOneUrl ?? string.Empty);
            store.SetString(Key("button-2-label"), options.ButtonTwoLabel ?? string.Empty);
            store.SetString(Key("button-2-url"), options.ButtonTwoUrl ?? string.Empty);

            var handler = Changed;
            if (handler != null)
            {
                handler();
            }
        }

        private DiscordUnityRpcLogLevel ReadLogLevel()
        {
            var value = store.GetInt(Key("log-level"), (int)DiscordUnityRpcLogLevel.Errors);
            return Enum.IsDefined(typeof(DiscordUnityRpcLogLevel), value)
                ? (DiscordUnityRpcLogLevel)value
                : DiscordUnityRpcLogLevel.Errors;
        }

        private string Key(string suffix)
        {
            return KeyPrefix + suffix;
        }

        private static string HashNormalizedPath(string projectRoot)
        {
            var normalized = Path.GetFullPath(projectRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace('\\', '/')
                .ToLowerInvariant();

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
                var builder = new StringBuilder(hash.Length * 2);
                for (var index = 0; index < hash.Length; index++)
                {
                    builder.Append(hash[index].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private sealed class UnityEditorPrefsStore : IKeyValueStore
        {
            public bool GetBool(string key, bool defaultValue) { return EditorPrefs.GetBool(key, defaultValue); }
            public int GetInt(string key, int defaultValue) { return EditorPrefs.GetInt(key, defaultValue); }
            public string GetString(string key, string defaultValue) { return EditorPrefs.GetString(key, defaultValue); }
            public void SetBool(string key, bool value) { EditorPrefs.SetBool(key, value); }
            public void SetInt(string key, int value) { EditorPrefs.SetInt(key, value); }
            public void SetString(string key, string value) { EditorPrefs.SetString(key, value); }
        }
    }
}
