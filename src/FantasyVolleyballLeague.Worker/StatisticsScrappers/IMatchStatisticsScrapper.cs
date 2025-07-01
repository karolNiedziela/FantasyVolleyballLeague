using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;

namespace FantasyVolleyballLeague.Worker.StatisticsScrapper
{
    public interface IMatchStatisticsScrapper
    {
        Task<(MatchTeamStatistics FirstTeamStatistics, MatchTeamStatistics SecondTeamStatitics)> GetMatchStatisticsAsync(int matchId);
    }
}
