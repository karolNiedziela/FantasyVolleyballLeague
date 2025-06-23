using FantasyVolleyballLeague.Worker.StatisticsExtractor.Models;

namespace FantasyVolleyballLeague.Worker.StatisticsExtractor
{
    public interface IStatisticsScrapper
    {
        Task<(MatchTeamStatistics FirstTeamStatistics, MatchTeamStatistics SecondTeamStatitics)> GetMatchStatisticsAsync();
    }
}
