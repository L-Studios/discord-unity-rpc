using System;

namespace LStudios.DiscordUnityRpc
{
    internal interface IDiscordRpcTransport : IDisposable
    {
        bool IsConnected { get; }
        event Action Connected;
        event Action Disconnected;
        void Initialize(string applicationId);
        void Invoke();
        void SetPresence(PresencePayload payload);
        void ClearPresence();
    }
}
