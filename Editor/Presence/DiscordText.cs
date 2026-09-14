using System.Globalization;
using System.Text;

namespace LStudios.DiscordUnityRpc
{
    internal static class DiscordText
    {
        internal static string Normalize(string value, int maxTextElements)
        {
            if (string.IsNullOrWhiteSpace(value) || maxTextElements <= 0)
            {
                return string.Empty;
            }

            var trimmed = value.Trim();
            var enumerator = StringInfo.GetTextElementEnumerator(trimmed);
            var result = new StringBuilder();
            var count = 0;

            while (count < maxTextElements && enumerator.MoveNext())
            {
                result.Append(enumerator.GetTextElement());
                count++;
            }

            return result.ToString();
        }
    }
}
