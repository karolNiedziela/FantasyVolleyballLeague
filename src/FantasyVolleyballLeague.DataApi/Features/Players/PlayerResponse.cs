using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Players
{
    internal sealed class PlayerResponse
    {
        public Guid Id { get; init; }

        public string FullName { get; init; } = string.Empty;

        /// <summary>
        /// Code of the gender: men or women.
        /// </summary>
        public string Gender { get; init; } = string.Empty;

        public DateOnly? DateOfBirth { get; init; }

        public int? HeightCm { get; init; }

        public int? WeightKg { get; init; }

        public int? AttackReachCm { get; init; }

        /// <summary>
        /// The roster (the club in one season) of the player's current roster player: the most recent one among the
        /// roster players the request selects (all of them without a roster, season or league filter). A player who
        /// moved during the season shows the roster he moved to. <c>null</c> only for a player without any roster player.
        /// </summary>
        public RosterSummaryResponse? Roster { get; init; }

        /// <summary>
        /// Code of the position: outside_hitter, opposite_hitter, middle_blocker, setter or libero.
        /// Taken from the same current roster player as <see cref="Roster"/>. <c>null</c> when that roster player has
        /// no position, even if an earlier one had.
        /// </summary>
        public string? Position { get; init; }
    }
}
