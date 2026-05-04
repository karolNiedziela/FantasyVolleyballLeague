namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class SetScore
    {
        public Guid Id { get; private set; }

        public int SetNumber { get; private set; }

        public int HomeTeamScore { get; private set; }

        public int AwayTeamScore { get; private set; }

        public Guid MatchId { get; private set; }

        public Match Match { get; private set; } = default!;

        private SetScore() { }

        public SetScore(int setNumber, int homeTeamScore, int awayTeamScore, Guid matchId)
        {
            Id = Guid.CreateVersion7();
            SetNumber = setNumber;
            HomeTeamScore = homeTeamScore;
            AwayTeamScore = awayTeamScore;
            MatchId = matchId;
        }
    }
}
