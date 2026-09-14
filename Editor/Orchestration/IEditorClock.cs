namespace LStudios.DiscordUnityRpc
{
    internal interface IEditorClock
    {
        double TimeSinceStartup { get; }
        long UnixSeconds { get; }
    }
}
