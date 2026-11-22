using FantasyVolleyballLeague.Shared.Text;

namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// Comparison key for player names. Sources disagree on the name order ("Benjamin Toniutti" in rosters,
    /// "Toniutti Benjamin" in widget statistics) and sometimes on case, so the key is the sorted set of
    /// the words of <see cref="NameNormalizer"/>.
    /// </summary>
    internal static class PlayerNameKey
    {
        public static string Create(string fullName)
        {
            return string.Join(' ', NameNormalizer.Tokens(fullName).Order(StringComparer.Ordinal));
        }

        public static bool ShareToken(string firstKey, string secondKey)
        {
            var firstTokens = firstKey.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return secondKey.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(firstTokens.Contains);
        }
    }
}
