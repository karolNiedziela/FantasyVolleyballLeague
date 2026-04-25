using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers
{
    public interface ISeasonMatchScrapper
    {
        Task<IReadOnlyList<SeasonPhaseStatistics>> GetAllPhasesMatchStatisticsAsync(
            IPage page, PlaywrightSession session, string matchDetailsBaseUrl);
    }
}
