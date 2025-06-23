namespace FantasyVolleyballLeague.Worker.StatisticsExtractor.Models
{
    public sealed record MatchTeamStatistics(
        string Name,
        List<PlayerMatchStatistics> PlayersStatistics);
}
