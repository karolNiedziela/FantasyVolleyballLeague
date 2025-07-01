namespace FantasyVolleyballLeague.Worker.StatisticsScrapper.Models
{
    public sealed class MatchTeamStatistics
    {
        public string Name { get; set; } = string.Empty;

        public List<PlayerMatchStatistics> PlayersStatistics { get; set; } = [];

        public int SetsWon { get; set; }

        public bool Won { get; set; }
    }
}
