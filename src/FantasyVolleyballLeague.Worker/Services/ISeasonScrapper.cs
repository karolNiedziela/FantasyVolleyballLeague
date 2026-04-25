using FantasyVolleyballLeague.Worker.TeamScrappers.Models;

namespace FantasyVolleyballLeague.Worker.Services
{
    public interface ISeasonScrapper
    {
        /// <param name="session">
        /// Optional shared session. If null, a new browser session is created and disposed internally.
        /// </param>
        Task<IEnumerable<SeasonInformation>> GetSeasons(LeagueOptions leagueOptions, PlaywrightSession? session = null);
    }
}
