namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class UserTeam
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<UserTeamPlayer> Players { get; set; } = [];

        private UserTeam() { }

        public UserTeam(Guid userId, string name, DateTimeOffset createdAt)
        {
            UserId = userId;
            Name = name;
            CreatedAt = createdAt;
        }
    }
}
