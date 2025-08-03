namespace FantasyVolleyballLeague.Domain.Entities
{
    public sealed class Match
    {
        public Guid Id { get; set; }

        public DateTimeOffset Date { get; set; }

        public Guid HomeTeamId { get; set; }

        public Team HomeTeam { get; set; } = default!;

        public Guid AwayTeamId { get; set; }

        public Team AwayTeam { get; set; } = default!;

        public Guid ExternalMatchId { get; set; }

        public ICollection<PlayerStatistics> PlayerStatistics { get; set; } = [];

        private Match() { }

        public Match(Guid homeTeamId, Guid awayTeamId, Guid externalMatchId, DateTimeOffset date)
        {
            HomeTeamId = homeTeamId;
            AwayTeamId = awayTeamId;
            ExternalMatchId = externalMatchId;
            Date = date;
        }
    }
}
