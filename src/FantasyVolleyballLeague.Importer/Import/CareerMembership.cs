using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// Dates a player belonged to a club, read from the career entries of the player's page.
    /// </summary>
    internal static class CareerMembership
    {
        /// <summary>
        /// From the earliest start to the latest end of the stints in the club; <c>null</c> dates when the career has none.
        /// </summary>
        public static (DateOnly? From, DateOnly? To) ForClub(IReadOnlyList<CareerEntryFile> career, int clubExternalId)
        {
            var stints = career.Where(entry => entry.ClubExternalId == clubExternalId).ToList();
            if (stints.Count == 0)
            {
                return (null, null);
            }

            return (stints.Min(entry => entry.From), stints.Max(entry => entry.To));
        }
    }
}
