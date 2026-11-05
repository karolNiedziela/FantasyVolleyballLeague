namespace FantasyVolleyballLeague.Scraper.Models
{
    internal sealed record MatchRosterRecord
    {
        public required string Name { get; init; }

        /// <summary>
        /// Club id from the league website (stable across seasons).
        /// </summary>
        public int? ExternalId { get; init; }

        public int? SetsWon { get; init; }

        public IReadOnlyList<PlayerMatchStatistics> Players { get; init; } = [];
    }
}
