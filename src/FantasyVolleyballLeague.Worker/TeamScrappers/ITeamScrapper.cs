using FantasyVolleyballLeague.Worker.TeamScrappers.Models;

namespace FantasyVolleyballLeague.Worker.TeamScrappers
{
    public interface ITeamScrapper
    {
        Task<List<TeamInformation>> GetTeamDataAsync();
    }
}
