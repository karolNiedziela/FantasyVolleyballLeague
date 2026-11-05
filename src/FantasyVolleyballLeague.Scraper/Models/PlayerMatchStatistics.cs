namespace FantasyVolleyballLeague.Scraper.Models
{
    /// <summary>
    /// Player line of a match. Counting statistics are <c>null</c> when the source does not provide them
    /// (archive seasons only list the lineup).
    /// </summary>
    internal sealed record PlayerMatchStatistics
    {
        public int? Number { get; init; }

        public required string Name { get; init; }

        /// <summary>
        /// Player id from the league website. Not available in the volleystation widget era (2019/20+).
        /// </summary>
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
