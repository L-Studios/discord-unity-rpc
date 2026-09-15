using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class UnityEditorContextTracker : IEditorContextSource
    {
        private const double ToolPollIntervalSeconds = 0.5;

        private readonly IDisposable prefabSubscription;
        private EditorToolKind activeTool;
        private double nextToolPollAt;
        private bool disposed;

        internal UnityEditorContextTracker()
        {
            activeTool = ResolveFocusedTool(EditorToolKind.None);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.projectChanged += OnEditorChanged;
            EditorApplication.hierarchyChanged += OnEditorChanged;
            EditorApplication.update += OnUpdate;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            CompilationPipeline.compilationStarted += OnCompilationChanged;
            CompilationPipeline.compilationFinished += OnCompilationChanged;
            BuildActivityMonitor.Changed += OnEditorChanged;
            prefabSubscription = PrefabStageCompatibility.Subscribe(OnEditorChanged);
        }

        public event Action ContextChanged;

        public bool IsApplicationActive { get { return InternalEditorUtility.isApplicationActive; } }

        public EditorContextSnapshot Capture()
        {
            var prefabName = PrefabStageCompatibility.GetCurrentPrefabName();
            var projectDirectory = Directory.GetParent(Application.dataPath);
            var projectName = projectDirectory == null ? string.Empty : projectDirectory.Name;
            var sceneName = SceneManager.GetActiveScene().name ?? string.Empty;
            var building = BuildActivityMonitor.IsBuilding;
            var platform = building
                ? BuildActivityMonitor.BuildPlatform
                : EditorPlatformResolver.FromBuildTarget(EditorUserBuildSettings.activeBuildTarget);

            return new EditorContextSnapshot(
                projectName,
                sceneName,
                prefabName,
                Application.unityVersion,
                ResolveActivityKind(
                    building,
                    EditorApplication.isCompiling,
                    EditorApplication.isPlayingOrWillChangePlaymode,
                    prefabName.Length > 0),
                activeTool,
                platform);
        }

        internal static EditorActivityKind ResolveActivityKind(
            bool building,
            bool compiling,
            bool playing,
            bool prefabOpen)
        {
            if (building)
            {
                return EditorActivityKind.Building;
            }

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
            EditorApplication.update -= OnUpdate;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            CompilationPipeline.compilationStarted -= OnCompilationChanged;
            CompilationPipeline.compilationFinished -= OnCompilationChanged;
            BuildActivityMonitor.Changed -= OnEditorChanged;
            prefabSubscription.Dispose();
        }

        private void OnUpdate()
        {
            // Failed or cancelled builds skip IPostprocessBuildWithReport; the editor loop only
            // resumes once the build is over, so this is the first safe place to notice it.
            if (BuildActivityMonitor.IsBuilding && !BuildPipeline.isBuildingPlayer)
            {
                BuildActivityMonitor.End();
            }

            var now = EditorApplication.timeSinceStartup;
            if (now < nextToolPollAt)
            {
                return;
            }

            nextToolPollAt = now + ToolPollIntervalSeconds;
            var tool = ResolveFocusedTool(activeTool);
            if (tool != activeTool)
            {
                activeTool = tool;
                RaiseChanged();
            }
        }

        private static EditorToolKind ResolveFocusedTool(EditorToolKind previous)
        {
            var window = EditorWindow.focusedWindow;
            var selected = Selection.activeGameObject;

            // Looked up by name so projects that disable the Terrain module still compile.
            return EditorToolResolver.Resolve(
                window == null ? null : window.GetType().FullName,
                selected != null && selected.GetComponent("Terrain") != null,
                previous);
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
