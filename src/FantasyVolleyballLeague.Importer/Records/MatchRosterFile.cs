namespace FantasyVolleyballLeague.Importer.Records
{
    internal sealed record MatchRosterFile
    {
        public string Name { get; init; } = string.Empty;

        public int? ExternalId { get; init; }

        public int? SetsWon { get; init; }

        public IReadOnlyList<PlayerStatisticsFile> Players { get; init; } = [];
    }
}
