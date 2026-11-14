using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds
{
    internal static class LeagueSeeds
    {
        public static string LeagueName(int number)
        {
            return $"League {number:000}";
        }

        // DivisionNumber = number, and (CountryId, GenderId, DivisionNumber) is unique:
        // two calls for the same country and gender collide.
        public static IEnumerable<League> CreateLeagues(
            int count,
            LeagueCountry? country = null,
            LeagueGender? gender = null)
        {
            var countryId = (country ?? LeagueCountry.Poland).Id;
            var genderId = (gender ?? LeagueGender.Men).Id;

            return Enumerable.Range(1, count)
                .Select(number => new League(LeagueName(number), (short)number, countryId, genderId));
        }
    }
}
