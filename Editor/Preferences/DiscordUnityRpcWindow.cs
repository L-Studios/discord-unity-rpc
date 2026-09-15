using UnityEditor;
using UnityEngine;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class DiscordUnityRpcWindow : EditorWindow
    {
        private static GUIStyle contentStyle;
        private Vector2 scrollPosition;

        [MenuItem("Window/Discord Unity RPC")]
        internal static void Open()
        {
            var window = GetWindow<DiscordUnityRpcWindow>();
            window.titleContent = new GUIContent("Discord Unity RPC");
            window.minSize = new Vector2(320f, 380f);
            window.Show();
        }

        private void OnInspectorUpdate()
        {
            // Keeps the connection status current while the window is open.
            Repaint();
        }

        private void OnGUI()
        {
            if (contentStyle == null)
            {
                contentStyle = new GUIStyle { padding = new RectOffset(10, 10, 10, 10) };
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical(contentStyle);
            DiscordUnityRpcSettingsProvider.DrawSettingsGui();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}
