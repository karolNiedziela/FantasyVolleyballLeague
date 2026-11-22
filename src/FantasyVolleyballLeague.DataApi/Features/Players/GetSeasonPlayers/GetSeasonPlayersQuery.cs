using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetSeasonPlayers
{
    internal sealed record GetSeasonPlayersQuery(
        Guid SeasonId,
        [FromQuery(Name = PlayerQueryParameters.Position)] string? Position = null,
        [FromQuery(Name = PlayerQueryParameters.RosterId)] Guid? RosterId = null,
        [FromQuery(Name = PlayerQueryParameters.Query)] string? Search = null,
        [FromQuery(Name = SortingQueryParameters.Sort)] string? Sort = null,
        [FromQuery(Name = PagingQueryParameters.Offset)] int Offset = PagingDefaults.DefaultOffset,
        [FromQuery(Name = PagingQueryParameters.Limit)] int Limit = PagingDefaults.DefaultLimit) : ISortableQuery<Player>, IPagedQuery
    {
        public static SortMap<Player> SortMap => PlayerSortMaps.Default;
    }
}
