using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds
{
    internal static class SeasonSeeds
    {
        public static Season Season(League league, int startYear)
        {
            return new Season(startYear, startYear + 1, league.Id, externalId: null);
        }

        // Seasons of one league in consecutive years starting at firstStartYear.
        public static IEnumerable<Season> CreateSeasons(League league, int count, int firstStartYear = 2000)
        {
            return Enumerable.Range(firstStartYear, count).Select(startYear => Season(league, startYear));
        }

        public static Phase Phase(Season season, PhaseType type, int order)
        {
            return new Phase(type.Name, order, season.Id, type.Id);
        }

        public static Round Round(Phase phase, string name, int order, string stage = "", int stageOrder = 1)
        {
            return new Round(phase.Id, stage, stageOrder, name, order);
        }
    }
}
