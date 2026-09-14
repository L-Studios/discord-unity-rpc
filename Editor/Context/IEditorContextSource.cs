using System;

namespace LStudios.DiscordUnityRpc
{
    internal interface IEditorContextSource : IDisposable
    {
        event Action ContextChanged;
        EditorContextSnapshot Capture();
    }
}
