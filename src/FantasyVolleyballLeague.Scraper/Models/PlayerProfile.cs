namespace FantasyVolleyballLeague.Scraper.Models
{
    internal sealed record PlayerProfile
    {
        /// <summary>
        /// Player id from the league website (stable across seasons).
        /// </summary>
        public required int ExternalId { get; init; }

        /// <summary>
        /// "First Last", as listed on the team page.
        /// </summary>
        public required string FullName { get; init; }

        /// <summary>
        /// English position name (see <see cref="PlsPositionMapper"/>).
        /// </summary>
        public string? Position { get; init; }

        public int? ShirtNumber { get; init; }

        public DateOnly? DateOfBirth { get; init; }

        public int? Height { get; init; }

        public int? Weight { get; init; }

        public int? AttackReach { get; init; }
    }
}
