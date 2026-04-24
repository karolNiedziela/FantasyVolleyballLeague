using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.StatisticsScrapper
{
    public interface IMatchStatisticsScrapper
    {
        Task<MatchRecord?> GetMatchStatisticsAsync(int matchId, PlaywrightSession session);
    }
}
