namespace FantasyVolleyballLeague.Importer.Records
{
    /// <summary>
    /// Raw match file written by the scraper (<c>data/raw/matches/{League}/{YYYY-YYYY}/{MatchId}.json</c>, schema 1).
    /// </summary>
    internal sealed record MatchFile
    {
        public int SchemaVersion { get; init; }

        public DateTime ScrapedAtUtc { get; init; }

        public string League { get; init; } = string.Empty;

        public SeasonFile Season { get; init; } = new(0, 0, null);

        public NamedOrderFile Phase { get; init; } = new(string.Empty, 0);

        public NamedOrderFile Stage { get; init; } = new(string.Empty, 0);

        public NamedOrderFile Round { get; init; } = new(string.Empty, 0);

        public int MatchId { get; init; }

        /// <summary>
        /// "Scheduled" or "Finished".
        /// </summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>
        /// "None", "LineupOnly" or "Full".
        /// </summary>
        public string StatisticsLevel { get; init; } = string.Empty;

        public DateTime? Date { get; init; }

        public bool IsTimeConfirmed { get; init; }

        public string? MatchNumber { get; init; }

        public int? Attendance { get; init; }

        public PlayerReferenceFile? Mvp { get; init; }

        public string? FirstReferee { get; init; }

        public string? SecondReferee { get; init; }

        public string? Commissioner { get; init; }

        public string? VenueName { get; init; }

        public string? VenueCity { get; init; }

        public IReadOnlyList<SetScoreFile> Sets { get; init; } = [];

        public MatchRosterFile HomeRoster { get; init; } = new();

        public MatchRosterFile AwayRoster { get; init; } = new();
    }
}
