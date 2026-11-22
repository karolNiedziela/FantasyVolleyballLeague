using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters
{
    internal static class RosterSortMaps
    {
        public static SortMap<Roster> Default => new SortMap<Roster>()
            .Add(RosterSortFields.Name, x => x.Name)
            .Add(RosterSortFields.StartYear, x => x.Season.StartYear)
            .WithDefault(RosterSortFields.Name);
    }
}
