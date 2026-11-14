namespace FantasyVolleyballLeague.Importer.Records
{
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
