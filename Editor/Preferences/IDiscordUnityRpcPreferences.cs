using System;

namespace LStudios.DiscordUnityRpc
{
    internal interface IDiscordUnityRpcPreferences
    {
        DiscordUnityRpcOptions Current { get; }
        event Action Changed;
        void Save(DiscordUnityRpcOptions options);
    }

    internal interface IKeyValueStore
    {
        bool GetBool(string key, bool defaultValue);
        int GetInt(string key, int defaultValue);
        string GetString(string key, string defaultValue);
        void SetBool(string key, bool value);
        void SetInt(string key, int value);
        void SetString(string key, string value);
    }
}
