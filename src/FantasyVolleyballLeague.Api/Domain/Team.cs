namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class Team
    {
        private readonly List<Player> _players = [];

        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public Guid LeagueId { get; private set; }

        public League League { get; private set; } = default!;

        public IReadOnlyCollection<Player> Players => _players.AsReadOnly();

        private Team() { }

        public Team(string name, Guid leagueId)
        {
            Id = Guid.CreateVersion7();
            Name = name;
            LeagueId = leagueId;
        }

        public void AddPlayer(string fullName, int positionId)
        {
            _players.Add(new Player(Id, fullName, positionId));
        }
    }
}
