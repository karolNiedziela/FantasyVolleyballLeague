namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class Player
    {
        public Guid Id { get; private set; }

        public string FullName { get; private set; } = string.Empty;

        public int PositionId { get; private set; }

        public PlayerPosition Position { get; private set; } = default!;

        public Guid TeamId { get; private set; }

        public Team Team { get; private set; } = default!;

        private Player() { }

        public Player(Guid teamId, string fullName, int positionId)
        {
            Id = Guid.CreateVersion7();
            TeamId = teamId;
            FullName = fullName;
            PositionId = positionId;
        }
    }
}
