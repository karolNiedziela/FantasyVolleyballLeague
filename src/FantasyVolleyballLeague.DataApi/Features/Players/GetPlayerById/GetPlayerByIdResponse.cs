namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerById
{
    internal sealed class GetPlayerByIdResponse
    {
        public Guid Id { get; init; }

        /// <summary>
        /// Id of the player on the source website.
        /// </summary>
        public int? ExternalId { get; init; }

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
        /// The player's most recent roster entry: the latest season, and within it (a transfer) the one with the
        /// latest <c>MemberFrom</c>. <c>null</c> for a player without any roster entry.
        /// </summary>
        public CurrentRosterResponse? CurrentRoster { get; init; }
    }
}
