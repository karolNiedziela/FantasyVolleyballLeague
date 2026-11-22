using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetSeasonRosters
{
    internal sealed record GetSeasonRostersQuery(
        Guid SeasonId,
        [FromQuery(Name = RosterQueryParameters.Query)] string? Search = null,
        [FromQuery(Name = SortingQueryParameters.Sort)] string? Sort = null,
        [FromQuery(Name = PagingQueryParameters.Offset)] int Offset = PagingDefaults.DefaultOffset,
        [FromQuery(Name = PagingQueryParameters.Limit)] int Limit = PagingDefaults.DefaultLimit) : ISortableQuery<Roster>, IPagedQuery
    {
        public static SortMap<Roster> SortMap => RosterSortMaps.Default;
    }
}
