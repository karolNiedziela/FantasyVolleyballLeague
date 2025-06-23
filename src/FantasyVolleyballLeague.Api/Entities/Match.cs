namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class Match
    {
        public Guid Id { get; set; }

        public DateTimeOffset Date { get; set; }

        public Guid HomeTeamId { get; set; }

        public Team HomeTeam { get; set; } = default!;

        public Guid AwayTeamId { get; set; }

        public Team AwayTeam { get; set; } = default!;

        public ICollection<PlayerStatistics> PlayerStatistics { get; set; } = [];

        private Match() { }

        public Match(Guid homeTeamId, Guid awayTeamId, DateTimeOffset date)
        {
            HomeTeamId = homeTeamId;
            AwayTeamId = awayTeamId;
            Date = date;
        }
    }
}
