namespace FantasyVolleyballLeague.Importer.Records
{
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
