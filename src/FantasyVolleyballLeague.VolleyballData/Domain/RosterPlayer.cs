namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    /// <summary>
    /// A player in a roster (club in a season), with shirt number and position.
    /// </summary>
    internal sealed class RosterPlayer : Entity
    {
        public Guid RosterId { get; private set; }

        public Roster Roster { get; private set; } = default!;

        public Guid PlayerId { get; private set; }

        public Player Player { get; private set; } = default!;

        public int? ShirtNumber { get; private set; }

        public int? PositionId { get; private set; }

        public PlayerPosition? Position { get; private set; } = default!;

        /// <summary>
        /// First day the player belonged to the club, as listed in the player's career. <c>null</c> when unknown.
        /// A player who moved during the season belongs to the roster of each club only between these dates.
        /// </summary>
        public DateOnly? MemberFrom { get; private set; }

        /// <summary>
        /// Last day the player belonged to the club. <c>null</c> when unknown.
        /// </summary>
        public DateOnly? MemberTo { get; private set; }

        private RosterPlayer() { }

        public RosterPlayer(Guid rosterId, Guid playerId, int? shirtNumber, int? positionId, DateOnly? memberFrom, DateOnly? memberTo)
        {
            RosterId = rosterId;
            PlayerId = playerId;
            ShirtNumber = shirtNumber;
            PositionId = positionId;
            MemberFrom = memberFrom;
            MemberTo = memberTo;
        }

        public void Update(int? shirtNumber, int? positionId, DateOnly? memberFrom, DateOnly? memberTo)
        {
            ShirtNumber = shirtNumber ?? ShirtNumber;
            PositionId = positionId ?? PositionId;
            MemberFrom = memberFrom ?? MemberFrom;
            MemberTo = memberTo ?? MemberTo;
        }
    }
}
