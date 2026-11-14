using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Import
{
    internal static class CareerMembership
    {
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
