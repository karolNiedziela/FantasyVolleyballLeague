namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosterById
{
    internal sealed class RosterPlayerResponse
    {
        public Guid PlayerId { get; init; }

        public string FullName { get; init; } = string.Empty;

        public DateOnly? DateOfBirth { get; init; }

        public int? HeightCm { get; init; }

        public int? WeightKg { get; init; }

        public int? AttackReachCm { get; init; }

        public int? ShirtNumber { get; init; }

        /// <summary>
        /// Code of the position: outside_hitter, opposite_hitter, middle_blocker, setter or libero.
        /// <c>null</c> when the position is unknown.
        /// </summary>
        public string? Position { get; init; }

        /// <summary>
        /// First day the player belonged to the club. <c>null</c> when unknown.
        /// A player who moved during the season belongs to the roster of each club only between these dates.
        /// </summary>
        public DateOnly? MemberFrom { get; init; }

        /// <summary>
        /// Last day the player belonged to the club. <c>null</c> when unknown.
        /// </summary>
        public DateOnly? MemberTo { get; init; }
    }
}
