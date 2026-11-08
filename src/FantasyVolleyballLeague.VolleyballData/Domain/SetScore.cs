namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class SetScore : Entity
    {
        public int SetNumber { get; private set; }

        public int HomeScore { get; private set; }

        public int AwayScore { get; private set; }

        public Guid MatchId { get; private set; }

        public Match Match { get; private set; } = default!;

        private SetScore() { }

        public SetScore(int setNumber, int homeScore, int awayScore, Guid matchId)
        {
            SetNumber = setNumber;
            HomeScore = homeScore;
            AwayScore = awayScore;
            MatchId = matchId;
        }
    }
}
