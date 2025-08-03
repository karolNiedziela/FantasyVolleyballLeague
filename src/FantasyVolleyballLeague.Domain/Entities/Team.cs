namespace FantasyVolleyballLeague.Domain.Entities
{
    public sealed class Team
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid LeagueId { get; set; }

        public League League { get; set; } = default!;

        public ICollection<Player> Players { get; set; } = [];

        private Team() { }

        public Team(string name)
        {
            Name = name;
        }
    }
}
