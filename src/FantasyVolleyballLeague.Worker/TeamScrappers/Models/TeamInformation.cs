namespace FantasyVolleyballLeague.Worker.TeamScrappers.Models
{
    public sealed class TeamInformation
    {
        public string Name { get; set; } = string.Empty;

        public List<PlayerInformation> Players { get; set; } = new List<PlayerInformation>();
    }
}
