namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class Match
    {
        private readonly List<SetScore> _sets = [];
        private readonly List<PlayerMatchStatistics> _playerStatistics = [];

        public Guid Id { get; private set; }

        public DateTimeOffset Date { get; private set; }

        public int? ExternalId { get; private set; }

        public Venue Venue { get; private set; } = default!;

        public MatchOfficials Officials { get; private set; } = default!;

        public Guid LeagueId { get; private set; }

        public League League { get; private set; } = default!;

        public Guid? MvpId { get; private set; }

        public Player? Mvp { get; private set; } = default!;

        public Guid HomeTeamId { get; private set; }

        public Team HomeTeam { get; private set; } = default!;

        public Guid AwayTeamId { get; private set; }

        public Team AwayTeam { get; private set; } = default!;

        public IReadOnlyCollection<SetScore> Sets => _sets.AsReadOnly();

        public IReadOnlyCollection<PlayerMatchStatistics> PlayerStatistics => _playerStatistics.AsReadOnly();

        private Match() { }

        public Match(
            DateTimeOffset date,           
            Guid leagueId,
            Guid homeTeamId,
            Guid awayTeamId,
            Venue venue,
            MatchOfficials officials,
            int? externalId = null)
        {
            Id = Guid.CreateVersion7();
            Date = date;
            LeagueId = leagueId;
            HomeTeamId = homeTeamId;
            AwayTeamId = awayTeamId;
            Venue = venue;
            Officials = officials;
            ExternalId = externalId;
        }

        public void RecordSet(int setNumber, int homeScore, int awayScore)
        {
            _sets.Add(new SetScore(setNumber, homeScore, awayScore, Id));
        }

        public void AssignMvp(Guid playerId)
        {
            MvpId = playerId;
        }
    }
}
