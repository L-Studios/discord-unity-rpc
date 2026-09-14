using NUnit.Framework;

namespace LStudios.DiscordUnityRpc.Tests
{
    internal sealed class DiscordPresenceLifetimeTests
    {
        [Test]
        public void RepeatedReloadOrQuitNotificationsCleanUpExactlyOnce()
        {
            var calls = 0;
            var lifetime = new DiscordPresenceLifetime(delegate { calls++; });

            lifetime.Cleanup();
            lifetime.Cleanup();

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void ReinitializationUsesFreshLifetime()
        {
            var calls = 0;
            var first = new DiscordPresenceLifetime(delegate { calls++; });
            first.Cleanup();
            var second = new DiscordPresenceLifetime(delegate { calls++; });

            second.Cleanup();

            Assert.That(calls, Is.EqualTo(2));
        }
    }
}
