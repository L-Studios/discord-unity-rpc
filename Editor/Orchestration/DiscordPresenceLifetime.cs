using System;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class DiscordPresenceLifetime
    {
        private Action cleanup;

        internal DiscordPresenceLifetime(Action cleanup)
        {
            this.cleanup = cleanup ?? throw new ArgumentNullException("cleanup");
        }

        internal void Cleanup()
        {
            var action = cleanup;
            if (action == null)
            {
                return;
            }

            cleanup = null;
            action();
        }
    }
}
