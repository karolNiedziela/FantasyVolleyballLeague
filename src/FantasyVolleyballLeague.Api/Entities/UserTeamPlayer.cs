namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class UserTeamPlayer
    {
        public Guid UserTeamId { get; set; }

        public UserTeam UserTeam { get; set; } = default!;

        public Guid PlayerId { get; set; }

        public Player Player { get; set; } = default!;

        private UserTeamPlayer() { }

        public UserTeamPlayer(Guid userTeamId, Guid playerId)
        {
            UserTeamId = userTeamId;
            PlayerId = playerId;
        }
    }
}
