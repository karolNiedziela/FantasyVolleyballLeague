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

    internal sealed record SeasonFile(int StartYear, int EndYear, int? ExternalId);

    internal sealed record NamedOrderFile(string Name, int Order);

    internal sealed record PlayerReferenceFile(string Name, int? ExternalId);

    internal sealed record SetScoreFile(int HomePoints, int AwayPoints);

    internal sealed record MatchRosterFile
    {
        public string Name { get; init; } = string.Empty;

        public int? ExternalId { get; init; }

        public int? SetsWon { get; init; }

        public IReadOnlyList<PlayerStatisticsFile> Players { get; init; } = [];
    }

    internal sealed record PlayerStatisticsFile
    {
        public int? Number { get; init; }

        public string Name { get; init; } = string.Empty;

        public int? ExternalId { get; init; }

        public int? PointsTotal { get; init; }

        public int? ServeTotal { get; init; }

        public int? ServeErrors { get; init; }

        public int? Aces { get; init; }

        public int? ReceptionTotal { get; init; }

        public int? ReceptionErrors { get; init; }

        public string? ReceptionPositivePercent { get; init; }

        public string? ReceptionPerfectPercent { get; init; }

        public int? AttackTotal { get; init; }

        public int? AttackErrors { get; init; }

        public int? AttackBlocked { get; init; }

        public int? AttackPoints { get; init; }

        public string? AttackEfficiencyPercent { get; init; }

        public int? Blocks { get; init; }
    }
}
