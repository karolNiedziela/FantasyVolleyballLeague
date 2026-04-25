using FantasyVolleyballLeague.Worker.TeamScrappers.Models;

namespace FantasyVolleyballLeague.Worker.Services
{
    public interface ISeasonScrapper
    {
        /// <param name="session">
        /// Optional shared session. If null, a new browser session is created and disposed internally.
        /// </param>
        Task<IEnumerable<Season>> GetSeasonsAsync(LeagueOptions leagueOptions, PlaywrightSession? session = null);

        /// <param name="session">
        /// Optional shared session. If null, a new browser session is created and disposed internally.
        /// </param>
        Task<IEnumerable<Season>> GetTeamSeasonsAsync(LeagueOptions leagueOptions, PlaywrightSession? session = null);
    }
}
