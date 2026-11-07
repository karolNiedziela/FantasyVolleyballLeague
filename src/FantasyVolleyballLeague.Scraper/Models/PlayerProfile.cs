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
        /// English position name, e.g. "Outside Hitter".
        /// </summary>
        public string? Position { get; init; }

        public int? ShirtNumber { get; init; }

        public DateOnly? DateOfBirth { get; init; }

        public int? Height { get; init; }

        public int? Weight { get; init; }

        public int? AttackReach { get; init; }

        /// <summary>
        /// Stints in clubs during the roster's season (more than one when the player moved mid-season).
        /// Empty when the player page could not be read.
        /// </summary>
        public IReadOnlyList<PlayerCareerEntry> Career { get; init; } = [];
    }
}
