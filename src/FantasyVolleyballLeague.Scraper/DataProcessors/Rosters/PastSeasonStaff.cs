using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Shared.Constants;

namespace FantasyVolleyballLeague.Scraper.DataProcessors.Rosters
{
    /// <summary>
    /// Staff of a roster scraped again for a season that is no longer the latest (e.g. with <c>--force</c>).
    /// </summary>
    /// <remarks>
    /// The team page of every season shows the club's current president, not the president of that season. Taking
    /// it from the page of a past season would assign today's president to an old season, so the president of a past
    /// season is kept from the file stored while that season was the latest (none if it never was). The coaches
    /// come from the page, because the staff box is per season.
    /// </remarks>
    internal static class PastSeasonStaff
    {
        public static RosterRecord KeepStoredPresident(RosterRecord scraped, RosterRecord? stored)
        {
            var coaches = scraped.Staff.Where(member => member.Role != StaffRoleNames.President);
            var storedPresidents = stored?.Staff.Where(member => member.Role == StaffRoleNames.President) ?? [];

            return scraped with { Staff = [.. coaches, .. storedPresidents] };
        }
    }
}
