using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds
{
    internal static class NameSeeds
    {
        public static Player Player(string fullName, LeagueGender? gender = null)
        {
            return new Player((gender ?? LeagueGender.Men).Id, fullName, externalId: null);
        }

        public static Roster Roster(League league, Season season, string name)
        {
            return new Roster(name, league.Id, season.Id, externalId: null);
        }
    }
}
