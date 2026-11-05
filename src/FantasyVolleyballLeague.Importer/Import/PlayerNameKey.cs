using System.Globalization;
using System.Text;

namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// Comparison key for player names. Sources disagree on the name order ("Benjamin Toniutti" in rosters,
    /// "Toniutti Benjamin" in widget statistics) and sometimes on case, so the key is the sorted set of
    /// upper-case name tokens without diacritics.
    /// </summary>
    internal static class PlayerNameKey
    {
        public static string Create(string fullName)
        {
            return string.Join(' ', Tokens(fullName).Order(StringComparer.Ordinal));
        }

        /// <summary>
        /// True when the names share at least one token, e.g. the same surname with a different first-name spelling.
        /// </summary>
        public static bool ShareToken(string firstKey, string secondKey)
        {
            var firstTokens = firstKey.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return secondKey.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(firstTokens.Contains);
        }

        private static string[] Tokens(string fullName)
        {
            return RemoveDiacritics(fullName.ToUpperInvariant())
                .Split([' ', '\t', ' '], StringSplitOptions.RemoveEmptyEntries);
        }

        private static string RemoveDiacritics(string text)
        {
            var builder = new StringBuilder(text.Length);
            foreach (var character in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                // "Ł" has no decomposed form, so it is mapped explicitly.
                builder.Append(character == 'Ł' ? 'L' : character);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
