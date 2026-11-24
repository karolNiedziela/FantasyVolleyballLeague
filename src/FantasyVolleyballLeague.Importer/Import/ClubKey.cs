using System.Globalization;

namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// The rosters that are one club unless <c>ManualData/clubs.json</c> merges them with others: the rosters of one
    /// league with one club id, or, for rosters without a club id, with one normalized name. When
    /// <c>ManualData/club-splits.json</c> splits a club id (the id passed to another club, e.g. a licence transfer),
    /// <see cref="FromStartYear"/> is the season the part starts in; <c>null</c> is the part before the first split.
    /// </summary>
    internal readonly record struct ClubKey(string LeagueKey, int? ExternalId, string? NormalizedName, int? FromStartYear)
    {
        public const char Separator = ':';

        public const char SplitSeparator = '@';

        /// <summary>
        /// The key of a roster. <paramref name="splitYears"/> holds the years a club id is split from, by league key
        /// and club id; the roster belongs to the part of the latest split year not after its season.
        /// </summary>
        public static ClubKey ForRoster(ClubRoster roster, IReadOnlyDictionary<(string LeagueKey, int ExternalId), int[]> splitYears)
        {
            if (roster.ExternalId is not { } externalId)
            {
                return new ClubKey(roster.LeagueKey, null, roster.NormalizedName, null);
            }

            int? fromStartYear = null;
            if (splitYears.TryGetValue((roster.LeagueKey, externalId), out var years))
            {
                var startedYears = years.Where(year => year <= roster.StartYear).ToList();
                fromStartYear = startedYears.Count > 0 ? startedYears.Max() : null;
            }

            return new ClubKey(roster.LeagueKey, externalId, null, fromStartYear);
        }

        /// <summary>
        /// Parses an id of the manual files: <c>League:clubId</c> with a positive club id, optionally followed by
        /// <c>@fromStartYear</c> (the part of a split club id).
        /// </summary>
        public static bool TryParse(string? value, out ClubKey key)
        {
            key = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var id = value;
            int? fromStartYear = null;
            var splitSeparator = value.IndexOf(SplitSeparator);
            if (splitSeparator >= 0)
            {
                if (!int.TryParse(value.AsSpan(splitSeparator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
                    || year <= 0)
                {
                    return false;
                }

                id = value[..splitSeparator];
                fromStartYear = year;
            }

            var separator = id.LastIndexOf(Separator);
            if (separator <= 0
                || !int.TryParse(id.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var externalId)
                || externalId <= 0)
            {
                return false;
            }

            key = new ClubKey(id[..separator], externalId, null, fromStartYear);
            return true;
        }

        public static ClubKey Parse(string value)
        {
            return TryParse(value, out var key)
                ? key
                : throw new FormatException($"'{value}' is not 'League{Separator}clubId' with a positive club id.");
        }
    }
}
