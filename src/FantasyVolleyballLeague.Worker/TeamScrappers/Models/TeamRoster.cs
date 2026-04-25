namespace FantasyVolleyballLeague.Worker.TeamScrappers.Models
{
    public sealed class TeamRoster
    {
        public string Name { get; set; } = string.Empty;

        public List<PlayerProfile> Players { get; set; } = new List<PlayerProfile>();
    }
}
