using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasons
{
    internal sealed record GetSeasonsQuery(
        [FromQuery(Name = SeasonQueryParameters.LeagueId)] Guid? LeagueId = null,
        [FromQuery(Name = SeasonQueryParameters.StartYear)] int? StartYear = null,
        [FromQuery(Name = SeasonQueryParameters.Current)] bool? Current = null,
        [FromQuery(Name = SortingQueryParameters.Sort)] string? Sort = null,
        [FromQuery(Name = PagingQueryParameters.Offset)] int Offset = PagingDefaults.DefaultOffset,
        [FromQuery(Name = PagingQueryParameters.Limit)] int Limit = PagingDefaults.DefaultLimit) : ISortableQuery<Season>, IPagedQuery
    {
        public static SortMap<Season> SortMap => SeasonSortMaps.Default;
    }
}
