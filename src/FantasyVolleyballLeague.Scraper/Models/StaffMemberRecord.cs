namespace FantasyVolleyballLeague.Scraper.Models
{
    internal sealed record StaffMemberRecord
    {
        /// <summary>
        /// "First Last", as listed on the team page.
        /// </summary>
        public required string FullName { get; init; }

        public required string Role { get; init; }
    }
}
