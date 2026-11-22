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
        /// Code of the position: outside_hitter, opposite_hitter, middle_blocker, setter or libero.
        /// Taken from the most recent roster player that has a position, among the roster players the request selects
        /// (all of them without a roster, season or league filter). <c>null</c> when none of them has one.
        /// </summary>
        public string? Position { get; init; }
    }
}
