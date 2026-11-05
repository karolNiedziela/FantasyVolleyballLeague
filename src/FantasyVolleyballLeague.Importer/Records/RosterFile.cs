namespace FantasyVolleyballLeague.Importer.Records
{
    /// <summary>
    /// Raw roster file written by the scraper (<c>data/raw/rosters/{League}/{YYYY-YYYY}/{ClubId}.json</c>, schema 1).
    /// </summary>
    internal sealed record RosterFile
    {
        public int SchemaVersion { get; init; }

        public DateTime ScrapedAtUtc { get; init; }

        public string League { get; init; } = string.Empty;

        public SeasonFile Season { get; init; } = new(0, 0, null);

        public ClubFile Club { get; init; } = new(0, string.Empty);

        public IReadOnlyList<RosterPlayerFile> Players { get; init; } = [];
    }

    internal sealed record ClubFile(int ExternalId, string Name);

    internal sealed record RosterPlayerFile
    {
        public int ExternalId { get; init; }

        /// <summary>
        /// "First Last".
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        /// English position name, e.g. "Outside Hitter".
        /// </summary>
        public string? Position { get; init; }

        public int? ShirtNumber { get; init; }

        public DateOnly? DateOfBirth { get; init; }

        public int? Height { get; init; }

        public int? Weight { get; init; }

        public int? AttackReach { get; init; }
    }
}
