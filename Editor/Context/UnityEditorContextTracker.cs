using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class UnityEditorContextTracker : IEditorContextSource
    {
        private readonly IDisposable prefabSubscription;
        private bool disposed;

        internal UnityEditorContextTracker()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.projectChanged += OnEditorChanged;
            EditorApplication.hierarchyChanged += OnEditorChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            CompilationPipeline.compilationStarted += OnCompilationChanged;
            CompilationPipeline.compilationFinished += OnCompilationChanged;
            prefabSubscription = PrefabStageCompatibility.Subscribe(OnEditorChanged);
        }

        public event Action ContextChanged;

        public EditorContextSnapshot Capture()
        {
            var prefabName = PrefabStageCompatibility.GetCurrentPrefabName();
            var projectDirectory = Directory.GetParent(Application.dataPath);
            var projectName = projectDirectory == null ? string.Empty : projectDirectory.Name;
            var sceneName = SceneManager.GetActiveScene().name ?? string.Empty;

            return new EditorContextSnapshot(
                projectName,
                sceneName,
                prefabName,
                Application.unityVersion,
                ResolveActivityKind(
                    EditorApplication.isCompiling,
                    EditorApplication.isPlayingOrWillChangePlaymode,
                    prefabName.Length > 0));
        }

        internal static EditorActivityKind ResolveActivityKind(
            bool compiling,
            bool playing,
            bool prefabOpen)
        {
            if (compiling)
            {
                return EditorActivityKind.Compiling;
            }

            if (playing)
            {
                return EditorActivityKind.Playing;
            }

            return prefabOpen
                ? EditorActivityKind.EditingPrefab
                : EditorActivityKind.EditingScene;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.projectChanged -= OnEditorChanged;
            EditorApplication.hierarchyChanged -= OnEditorChanged;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            CompilationPipeline.compilationStarted -= OnCompilationChanged;
            CompilationPipeline.compilationFinished -= OnCompilationChanged;
            prefabSubscription.Dispose();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            RaiseChanged();
        }

        private void OnActiveSceneChanged(Scene previous, Scene current)
        {
            RaiseChanged();
        }

        private void OnCompilationChanged(object context)
        {
            RaiseChanged();
        }

        private void OnEditorChanged()
        {
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            var handler = ContextChanged;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
