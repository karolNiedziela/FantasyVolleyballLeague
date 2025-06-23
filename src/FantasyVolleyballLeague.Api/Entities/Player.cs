namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class Player
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public PlayerPosition Position { get; set; } = default!;

        public Guid TeamId { get; set; }

        public Team Team { get; set; } = default!;

        public ICollection<PlayerStatistics> Statistics { get; set; } = [];

        private Player() { }

        public Player(Guid teamId, string fullName, PlayerPosition position)
        {
            TeamId = teamId;
            FullName = fullName;
            Position = position;
        }
    }
}
