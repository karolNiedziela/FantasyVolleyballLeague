using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers
{
    public interface ISeasonScrapper
    {
        Task<Dictionary<int, List<SeasonMatchWeekGameStatistics>>> GetSeasonStatisticsAsync();
    }
}
