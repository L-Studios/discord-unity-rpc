using System;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class PresenceButton : IEquatable<PresenceButton>
    {
        internal PresenceButton(string label, string url)
        {
            Label = label ?? string.Empty;
            Url = url ?? string.Empty;
        }

        internal string Label { get; private set; }
        internal string Url { get; private set; }

        public bool Equals(PresenceButton other)
        {
            return !ReferenceEquals(other, null)
                && string.Equals(Label, other.Label, StringComparison.Ordinal)
                && string.Equals(Url, other.Url, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PresenceButton);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Label.GetHashCode() * 397) ^ Url.GetHashCode());
            }
        }
    }
}
