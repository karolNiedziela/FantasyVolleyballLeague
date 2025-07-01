using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Models
{
    public sealed record SeasonMatchWeekGameStatistics(
        MatchTeamStatistics FirstTeamStatistics,
        MatchTeamStatistics SecondTeamStatistics,
        int ExternalMatchId);
}
