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
}
