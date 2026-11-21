using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons
{
    internal static class SeasonSortMaps
    {
        public static SortMap<Season> Default => new SortMap<Season>()
            .Add(SeasonSortFields.StartYear, x => x.StartYear)
            .WithDefault($"-{SeasonSortFields.StartYear}");
    }
}
