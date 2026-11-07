namespace FantasyVolleyballLeague.Scraper.Models
{
    /// <summary>
    /// One stint of a player in a club, from the career table of the player page. A player who moved during
    /// the season has one entry per club, so the entries tell which club he played for on a given day.
    /// </summary>
    internal sealed record PlayerCareerEntry
    {
        /// <summary>
        /// Club id from the league website (the same id as <see cref="ClubLink.ExternalId"/>).
        /// </summary>
        public required int ClubExternalId { get; init; }

        public required DateOnly From { get; init; }

        public required DateOnly To { get; init; }

        public int? ShirtNumber { get; init; }

        /// <summary>
        /// English position name, e.g. "Outside Hitter".
        /// </summary>
        public string? Position { get; init; }
    }
}
