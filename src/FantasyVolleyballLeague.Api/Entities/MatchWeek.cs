namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class MatchWeek
    {
        public Guid Id { get; set; }

        public int WeekNumber { get; set; }

        public Guid SeasonId { get; set; }

        public Season Season { get; set; } = default!;

        public ICollection<Match> Matches { get; set; } = [];

        private MatchWeek() { }

        public MatchWeek(int weekNumber, Guid seasonId)
        {
            WeekNumber = weekNumber;
            SeasonId = seasonId;
        }
    }
}
