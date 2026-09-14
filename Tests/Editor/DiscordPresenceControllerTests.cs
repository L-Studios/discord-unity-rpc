using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace LStudios.DiscordUnityRpc.Tests
{
    internal sealed class DiscordPresenceControllerTests
    {
        [Test]
        public void DisabledStartDoesNotInitializeTransport()
        {
            var fixture = new Fixture(false);

            fixture.Controller.Start();

            Assert.That(fixture.Transport.InitializeCalls, Is.EqualTo(0));
        }

        [Test]
        public void EnabledChangePublishesAfterOneSecond()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();

            fixture.Clock.Advance(0.99);
            fixture.Controller.Tick();
            Assert.That(fixture.Transport.Payloads, Is.Empty);

            fixture.Clock.Advance(0.01);
            fixture.Controller.Tick();
            Assert.That(fixture.Transport.Payloads, Has.Count.EqualTo(1));
        }

        [Test]
        public void RapidChangesPublishOnlyLatestPayload()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();
            fixture.Clock.Advance(0.5);
            fixture.Context.Snapshot = Snapshot("Second");
            fixture.Context.RaiseChanged();
            fixture.Clock.Advance(0.5);
            fixture.Context.Snapshot = Snapshot("Latest");
            fixture.Context.RaiseChanged();

            fixture.Clock.Advance(1.0);
            fixture.Controller.Tick();

            Assert.That(fixture.Transport.Payloads, Has.Count.EqualTo(1));
            Assert.That(fixture.Transport.Payloads[0].State, Is.EqualTo("Editing scene Latest"));
        }

        [Test]
        public void EqualPayloadIsNotPublishedTwice()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();
            fixture.Clock.Advance(1.0);
            fixture.Controller.Tick();
            fixture.Context.RaiseChanged();
            fixture.Clock.Advance(1.0);
            fixture.Controller.Tick();

            Assert.That(fixture.Transport.Payloads, Has.Count.EqualTo(1));
        }

        [Test]
        public void ConnectedEventRepublishesLatestPayload()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();
            fixture.Clock.Advance(1.0);
            fixture.Controller.Tick();

            fixture.Transport.RaiseConnected();

            Assert.That(fixture.Transport.Payloads, Has.Count.EqualTo(2));
        }

        [Test]
        public void DisconnectRetriesAtFiveFifteenThirtyThenSixtySeconds()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();
            fixture.Transport.RaiseDisconnected();

            fixture.Clock.Advance(5.0);
            fixture.Controller.Tick();
            fixture.Clock.Advance(15.0);
            fixture.Controller.Tick();
            fixture.Clock.Advance(30.0);
            fixture.Controller.Tick();
            fixture.Clock.Advance(60.0);
            fixture.Controller.Tick();
            fixture.Clock.Advance(60.0);
            fixture.Controller.Tick();

            Assert.That(fixture.Transport.InitializeTimes, Is.EqualTo(new[] { 0.0, 5.0, 20.0, 50.0, 110.0, 170.0 }));
        }

        [Test]
        public void RepeatedDisconnectNotificationDoesNotResetBackoff()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();
            fixture.Transport.RaiseDisconnected();
            fixture.Clock.Advance(5.0);
            fixture.Controller.Tick();

            fixture.Transport.RaiseDisconnected();
            fixture.Clock.Advance(5.0);
            fixture.Controller.Tick();

            Assert.That(fixture.Transport.InitializeTimes, Is.EqualTo(new[] { 0.0, 5.0 }));
            fixture.Clock.Advance(10.0);
            fixture.Controller.Tick();
            Assert.That(fixture.Transport.InitializeTimes, Is.EqualTo(new[] { 0.0, 5.0, 20.0 }));
        }

        [Test]
        public void ClearNowCancelsPendingPublishUntilContextChanges()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();

            fixture.Controller.ClearNow();
            fixture.Clock.Advance(1.0);
            fixture.Controller.Tick();
            Assert.That(fixture.Transport.Payloads, Is.Empty);

            fixture.Context.RaiseChanged();
            fixture.Clock.Advance(1.0);
            fixture.Controller.Tick();
            Assert.That(fixture.Transport.Payloads, Has.Count.EqualTo(1));
        }

        [Test]
        public void DisableClearsAndDisposesTransport()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();

            fixture.Preferences.Options.Enabled = false;
            fixture.Preferences.RaiseChanged();

            Assert.That(fixture.Transport.ClearCalls, Is.EqualTo(1));
            Assert.That(fixture.Transport.DisposeCalls, Is.EqualTo(1));
        }

        [Test]
        public void DisposeIsIdempotent()
        {
            var fixture = new Fixture(true);
            fixture.Controller.Start();

            fixture.Controller.Dispose();
            fixture.Controller.Dispose();

            Assert.That(fixture.Transport.DisposeCalls, Is.EqualTo(1));
            Assert.That(fixture.Context.DisposeCalls, Is.EqualTo(1));
        }

        private static EditorContextSnapshot Snapshot(string scene)
        {
            return new EditorContextSnapshot("Project", scene, string.Empty, "6000.3.24f1", EditorActivityKind.EditingScene);
        }

        private sealed class Fixture
        {
            internal Fixture(bool enabled)
            {
                Clock = new FakeClock();
                Context = new FakeContext { Snapshot = Snapshot("First") };
                Preferences = new FakePreferences { Options = new DiscordUnityRpcOptions { Enabled = enabled } };
                Transport = new FakeTransport(Clock);
                Controller = new DiscordPresenceController(
                    Context,
                    Preferences,
                    new DiscordPresenceFormatter(),
                    Transport,
                    Clock,
                    null);
            }

            internal FakeClock Clock { get; private set; }
            internal FakeContext Context { get; private set; }
            internal FakePreferences Preferences { get; private set; }
            internal FakeTransport Transport { get; private set; }
            internal DiscordPresenceController Controller { get; private set; }
        }

        private sealed class FakeClock : IEditorClock
        {
            internal FakeClock() { UnixSeconds = 1000L; }
            public double TimeSinceStartup { get; private set; }
            public long UnixSeconds { get; private set; }
            internal void Advance(double seconds)
            {
                TimeSinceStartup += seconds;
                UnixSeconds += (long)seconds;
            }
        }

        private sealed class FakeContext : IEditorContextSource
        {
            public event Action ContextChanged;
            internal EditorContextSnapshot Snapshot { get; set; }
            internal int DisposeCalls { get; private set; }
            public EditorContextSnapshot Capture() { return Snapshot; }
            public void Dispose() { DisposeCalls++; }
            internal void RaiseChanged() { var handler = ContextChanged; if (handler != null) handler(); }
        }

        private sealed class FakePreferences : IDiscordUnityRpcPreferences
        {
            public event Action Changed;
            internal DiscordUnityRpcOptions Options { get; set; }
            public DiscordUnityRpcOptions Current { get { return Options.Copy(); } }
            public void Save(DiscordUnityRpcOptions options) { Options = options.Copy(); RaiseChanged(); }
            internal void RaiseChanged() { var handler = Changed; if (handler != null) handler(); }
        }

        private sealed class FakeTransport : IDiscordRpcTransport
        {
            private readonly FakeClock clock;
            internal FakeTransport(FakeClock clock) { this.clock = clock; }
            public bool IsConnected { get; private set; }
            public event Action Connected;
            public event Action Disconnected;
            internal int InitializeCalls { get; private set; }
            internal int ClearCalls { get; private set; }
            internal int DisposeCalls { get; private set; }
            internal List<double> InitializeTimes { get; } = new List<double>();
            internal List<PresencePayload> Payloads { get; } = new List<PresencePayload>();
            public void Initialize(string applicationId) { InitializeCalls++; InitializeTimes.Add(clock.TimeSinceStartup); }
            public void Invoke() { }
            public void SetPresence(PresencePayload payload) { Payloads.Add(payload); }
            public void ClearPresence() { ClearCalls++; }
            public void Dispose() { DisposeCalls++; }
            internal void RaiseConnected() { IsConnected = true; var handler = Connected; if (handler != null) handler(); }
            internal void RaiseDisconnected() { IsConnected = false; var handler = Disconnected; if (handler != null) handler(); }
        }
    }
}
