using System;

namespace LStudios.DiscordUnityRpc
{
    internal sealed class PresencePayload : IEquatable<PresencePayload>
    {
        private readonly PresenceButton[] buttons;

        internal PresencePayload(
            string details,
            string state,
            string largeImageKey,
            string largeImageText,
            long? startTimestamp,
            PresenceButton[] buttons)
        {
            Details = details ?? string.Empty;
            State = state ?? string.Empty;
            LargeImageKey = largeImageKey ?? string.Empty;
            LargeImageText = largeImageText ?? string.Empty;
            StartTimestamp = startTimestamp;
            this.buttons = buttons == null ? new PresenceButton[0] : (PresenceButton[])buttons.Clone();
        }

        internal string Details { get; private set; }
        internal string State { get; private set; }
        internal string LargeImageKey { get; private set; }
        internal string LargeImageText { get; private set; }
        internal long? StartTimestamp { get; private set; }
        internal PresenceButton[] Buttons { get { return (PresenceButton[])buttons.Clone(); } }

        public bool Equals(PresencePayload other)
        {
            if (ReferenceEquals(other, null)
                || !string.Equals(Details, other.Details, StringComparison.Ordinal)
                || !string.Equals(State, other.State, StringComparison.Ordinal)
                || !string.Equals(LargeImageKey, other.LargeImageKey, StringComparison.Ordinal)
                || !string.Equals(LargeImageText, other.LargeImageText, StringComparison.Ordinal)
                || StartTimestamp != other.StartTimestamp
                || buttons.Length != other.buttons.Length)
            {
                return false;
            }

            for (var index = 0; index < buttons.Length; index++)
            {
                if (!buttons[index].Equals(other.buttons[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PresencePayload);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Details.GetHashCode();
                hash = (hash * 397) ^ State.GetHashCode();
                hash = (hash * 397) ^ LargeImageKey.GetHashCode();
                hash = (hash * 397) ^ LargeImageText.GetHashCode();
                hash = (hash * 397) ^ StartTimestamp.GetHashCode();
                for (var index = 0; index < buttons.Length; index++)
                {
                    hash = (hash * 397) ^ buttons[index].GetHashCode();
                }

                return hash;
            }
        }
    }
}
