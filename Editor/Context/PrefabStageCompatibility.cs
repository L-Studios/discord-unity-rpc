using System;
using System.IO;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace LStudios.DiscordUnityRpc
{
    internal static class PrefabStageCompatibility
    {
        internal static string GetCurrentPrefabName()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null || string.IsNullOrEmpty(stage.assetPath))
            {
                return string.Empty;
            }

            return Path.GetFileNameWithoutExtension(stage.assetPath) ?? string.Empty;
        }

        internal static IDisposable Subscribe(Action changed)
        {
            return new Subscription(changed);
        }

        private sealed class Subscription : IDisposable
        {
            private Action changed;

            internal Subscription(Action changed)
            {
                this.changed = changed;
                PrefabStage.prefabStageOpened += OnOpened;
                PrefabStage.prefabStageClosing += OnClosing;
            }

            public void Dispose()
            {
                if (changed == null)
                {
                    return;
                }

                PrefabStage.prefabStageOpened -= OnOpened;
                PrefabStage.prefabStageClosing -= OnClosing;
                changed = null;
            }

            private void OnOpened(PrefabStage stage)
            {
                RaiseChanged();
            }

            private void OnClosing(PrefabStage stage)
            {
                RaiseChanged();
            }

            private void RaiseChanged()
            {
                var handler = changed;
                if (handler != null)
                {
                    handler();
                }
            }
        }
    }
}
