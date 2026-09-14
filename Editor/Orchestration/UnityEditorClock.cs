using System;
using UnityEditor;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class UnityEditorClock : IEditorClock
    {
        private static readonly DateTime UnixEpochUtc =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public double TimeSinceStartup { get { return EditorApplication.timeSinceStartup; } }
        public long UnixSeconds { get { return (long)(DateTime.UtcNow - UnixEpochUtc).TotalSeconds; } }
    }
}
