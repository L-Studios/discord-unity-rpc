using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LStudios.DiscordUnityRpc
{
    internal static class DiscordUnityRpcSettingsProvider
    {
        private static readonly EditorPrefsDiscordUnityRpcPreferences ProjectPreferences =
            new EditorPrefsDiscordUnityRpcPreferences(GetProjectRoot());

        internal static event Action ClearPresenceRequested;
        internal static Func<string> ConnectionStatusProvider;
        internal static IDiscordUnityRpcPreferences Preferences { get { return ProjectPreferences; } }

        [SettingsProvider]
        internal static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Preferences/L.Studios/Discord Unity RPC", SettingsScope.User)
            {
                label = "Discord Unity RPC",
                guiHandler = searchContext => DrawSettingsGui(),
                keywords = new[] { "Discord", "Rich Presence", "L.Studios", "Privacy" }
            };
        }

        // Shared by Preferences and Window > Discord Unity RPC so both edit the same settings.
        internal static void DrawSettingsGui()
        {
            var options = ProjectPreferences.Current;

            EditorGUILayout.HelpBox(
                "When enabled, this package shares the selected Unity Editor context with the Discord desktop client running on this computer. Settings are private to this user and project.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            options.Enabled = EditorGUILayout.ToggleLeft("Enable Rich Presence", options.Enabled);

            EditorGUI.BeginDisabledGroup(!options.Enabled);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Privacy", EditorStyles.boldLabel);
            options.ShowProjectName = EditorGUILayout.Toggle("Show project name", options.ShowProjectName);
            options.ShowSceneName = EditorGUILayout.Toggle("Show scene name", options.ShowSceneName);
            options.ShowPrefabName = EditorGUILayout.Toggle("Show prefab name", options.ShowPrefabName);
            options.ShowElapsedTime = EditorGUILayout.Toggle("Show elapsed session time", options.ShowElapsedTime);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optional buttons (HTTPS only)", EditorStyles.boldLabel);
            options.ButtonOneLabel = EditorGUILayout.TextField("Button 1 label", options.ButtonOneLabel);
            options.ButtonOneUrl = EditorGUILayout.TextField("Button 1 URL", options.ButtonOneUrl);
            DrawUrlWarning(options.ButtonOneLabel, options.ButtonOneUrl);
            options.ButtonTwoLabel = EditorGUILayout.TextField("Button 2 label", options.ButtonTwoLabel);
            options.ButtonTwoUrl = EditorGUILayout.TextField("Button 2 URL", options.ButtonTwoUrl);
            DrawUrlWarning(options.ButtonTwoLabel, options.ButtonTwoUrl);

            EditorGUILayout.Space();
            options.LogLevel = (DiscordUnityRpcLogLevel)EditorGUILayout.EnumPopup("Log level", options.LogLevel);
            var statusProvider = ConnectionStatusProvider;
            EditorGUILayout.LabelField("Connection", statusProvider == null ? "Not initialized" : statusProvider());

            if (GUILayout.Button("Clear Presence"))
            {
                var handler = ClearPresenceRequested;
                if (handler != null)
                {
                    handler();
                }
            }

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                ProjectPreferences.Save(options);
            }
        }

        private static void DrawUrlWarning(string label, string url)
        {
            if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            Uri parsed;
            if (string.IsNullOrWhiteSpace(label)
                || !Uri.TryCreate(url, UriKind.Absolute, out parsed)
                || !string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                EditorGUILayout.HelpBox("A button needs a label and an absolute HTTPS URL.", MessageType.Warning);
            }
        }

        private static string GetProjectRoot()
        {
            var assets = new DirectoryInfo(Application.dataPath);
            return assets.Parent == null ? Application.dataPath : assets.Parent.FullName;
        }
    }
}
