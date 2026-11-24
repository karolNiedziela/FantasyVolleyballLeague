using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Import
{
    internal static class LeagueNaming
    {
        public const int MaxNameLength = 100;

        /// <summary>
        /// The name of the entry of the league with the greatest <see cref="LeagueNameFile.FromStartYear"/> not after
        /// <paramref name="startYear"/>, or <c>null</c> when the file has none (the caller keeps the current league
        /// name).
        /// </summary>
        public static string? Find(IReadOnlyList<LeagueNameFile> entries, string leagueKey, int startYear)
        {
            return entries
                .Where(entry => entry.League == leagueKey && entry.FromStartYear <= startYear)
                .MaxBy(entry => entry.FromStartYear)
                ?.Name;
        }

        public static IReadOnlyList<string> Validate(IReadOnlyList<LeagueNameFile> entries, IReadOnlyCollection<string> leagueKeys)
        {
            var errors = new List<string>();
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (!leagueKeys.Contains(entry.League))
                {
                    errors.Add($"Entry {index}: unknown league '{entry.League}'.");
                }

                if (string.IsNullOrWhiteSpace(entry.Name) || entry.Name.Length > MaxNameLength)
                {
                    errors.Add($"Entry {index}: the name must have 1 to {MaxNameLength} characters.");
                }
            }

            var duplicates = entries
                .GroupBy(entry => (entry.League, entry.FromStartYear))
                .Where(group => group.Count() > 1)
                .Select(group => $"League '{group.Key.League}' has {group.Count()} entries from {group.Key.FromStartYear}.");
            errors.AddRange(duplicates);

            return errors;
        }
    }
}
