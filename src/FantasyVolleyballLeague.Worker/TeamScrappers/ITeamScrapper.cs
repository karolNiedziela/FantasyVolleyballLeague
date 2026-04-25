using FantasyVolleyballLeague.Worker.TeamScrappers.Models;

namespace FantasyVolleyballLeague.Worker.TeamScrappers
{
    public interface ITeamScrapper
    {
        /// <param name="session">
        /// Optional shared session. If null, a new browser session is created and disposed internally.
        /// </param>
        Task<IEnumerable<TeamRoster>> GetTeamRosterAsync(Uri pageUrl, string baseUrl, PlaywrightSession? session = null);
    }
}
