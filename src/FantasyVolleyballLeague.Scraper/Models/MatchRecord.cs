namespace FantasyVolleyballLeague.Scraper.Models
{
    /// <summary>
    /// Raw match file (<c>data/raw/matches/{League}/{YYYY-YYYY}/{MatchId}.json</c>).
    /// Self-describing: carries its full schedule context, so the importer never relies on folder names.
    /// </summary>
    internal sealed record MatchRecord
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; init; } = CurrentSchemaVersion;

        public required DateTime ScrapedAtUtc { get; init; }

        public required string League { get; init; }

        public required SeasonReference Season { get; init; }

        public required NamedOrder Phase { get; init; }

        /// <summary>
        /// Empty name when the phase has a single stage (e.g. the regular season).
        /// </summary>
        public required NamedOrder Stage { get; init; }

        public required NamedOrder Round { get; init; }

        public required int MatchId { get; init; }

        public required MatchStatus Status { get; init; }

        public required StatisticsLevel StatisticsLevel { get; init; }

        public DateTime? Date { get; init; }

        public bool IsTimeConfirmed { get; init; }

        public string? MatchNumber { get; init; }

        public int? Attendance { get; init; }

        public PlayerReference? Mvp { get; init; }

        public string? FirstReferee { get; init; }

        public string? SecondReferee { get; init; }

        public string? Commissioner { get; init; }

        public string? VenueName { get; init; }

        public string? VenueCity { get; init; }

        public IReadOnlyList<SetScore> Sets { get; init; } = [];

        public required MatchRosterRecord HomeRoster { get; init; }

        public required MatchRosterRecord AwayRoster { get; init; }
    }
}
