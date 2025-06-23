namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class PlayerStatistics
    {
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }

        public Player Player { get; set; } = default!;

        public Guid MatchId { get; set; }

        public Match Match { get; set; } = default!;

        public int PointsScored { get; set; }

        public int Blocks { get; set; }

        public int Aces { get; set; }

        public int Errors { get; set; }

        private PlayerStatistics() { }

        public PlayerStatistics(Guid playerId, Guid matchId, int pointsScored, int blocks, int aces, int errors)
        {
            PlayerId = playerId;
            MatchId = matchId;
            PointsScored = pointsScored;
            Blocks = blocks;
            Aces = aces;
            Errors = errors;
        }
    }
}
