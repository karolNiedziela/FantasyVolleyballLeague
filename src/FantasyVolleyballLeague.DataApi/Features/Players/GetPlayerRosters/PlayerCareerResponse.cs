using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerRosters
{
    /// <summary>
    /// One roster player of the career: the player on one roster (a club in one season).
    /// </summary>
    internal sealed class PlayerCareerResponse
    {
        public RosterSummaryResponse Roster { get; init; } = new();

        public SeasonSummaryResponse Season { get; init; } = new();

        public LeagueSummaryResponse League { get; init; } = new();

        /// <summary>
        /// <c>null</c> when the shirt number is unknown.
        /// </summary>
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
