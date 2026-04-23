using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.StatisticsScrapper
{
    public interface IMatchStatisticsScrapper
    {
        Task<(MatchTeamStatistics FirstTeamStatistics, MatchTeamStatistics SecondTeamStatitics)?> GetMatchStatisticsAsync(int matchId, PlaywrightSession session);
    }
}
