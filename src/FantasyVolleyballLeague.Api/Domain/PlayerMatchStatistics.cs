namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class PlayerMatchStatistics
    {
        public Guid Id { get; private set; }

        public int Number { get; private set; }

        public int PointsTotal { get; private set; }

        public int ServeTotal { get; private set; }

        public int ServeErrors { get; private set; }

        public int Aces { get; private set; }

        public int ReceptionTotal { get; private set; }

        public int ReceptionErrors { get; private set; }

        public decimal? ReceptionPositivePercent { get; private set; }

        public decimal? ReceptionPerfectPercent { get; private set; }

        public int AttackTotal { get; private set; }

        public int AttackErrors { get; private set; }

        public int AttackBlocked { get; private set; }

        public int AttackPoints { get; private set; }

        public decimal? AttackEfficiencyPercent { get; private set; }

        public int Blocks { get; private set; }

        public Guid MatchId { get; private set; }

        public Match Match { get; private set; } = default!;

        public Guid PlayerId { get; private set; }

        public Player Player { get; private set; } = default!;

        private PlayerMatchStatistics() { }

        public PlayerMatchStatistics(
            int number,
            int pointsTotal,
            int serveTotal,
            int serveErrors,
            int aces,
            int receptionTotal,
            int receptionErrors,
            decimal? receptionPositivePercent,
            decimal? receptionPerfectPercent,
            int attackTotal,
            int attackErrors,
            int attackBlocked,
            int attackPoints,
            decimal? attackEfficiencyPercent,
            int blocks,
            Guid matchId,
            Guid playerId)
        {
            Id = Guid.CreateVersion7();
            Number = number;
            PointsTotal = pointsTotal;
            ServeTotal = serveTotal;
            ServeErrors = serveErrors;
            Aces = aces;
            ReceptionTotal = receptionTotal;
            ReceptionErrors = receptionErrors;
            ReceptionPositivePercent = receptionPositivePercent;
            ReceptionPerfectPercent = receptionPerfectPercent;
            AttackTotal = attackTotal;
            AttackErrors = attackErrors;
            AttackBlocked = attackBlocked;
            AttackPoints = attackPoints;
            AttackEfficiencyPercent = attackEfficiencyPercent;
            Blocks = blocks;
            MatchId = matchId;
            PlayerId = playerId;
        }
    }
}
