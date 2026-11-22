using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerById
{
    internal sealed class CurrentRosterResponse
    {
        public RosterSummaryResponse Roster { get; init; } = new();

        public SeasonSummaryResponse Season { get; init; } = new();

        /// <summary>
        /// <c>null</c> when the shirt number is unknown.
        /// </summary>
        public int? ShirtNumber { get; init; }

        /// <summary>
        /// Code of the position: outside_hitter, opposite_hitter, middle_blocker, setter or libero.
        /// <c>null</c> when the position is unknown.
        /// </summary>
        public string? Position { get; init; }
    }
}
