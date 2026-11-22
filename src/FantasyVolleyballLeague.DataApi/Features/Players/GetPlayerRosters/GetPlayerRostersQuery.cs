using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerRosters
{
    internal sealed record GetPlayerRostersQuery(
        Guid PlayerId,
        [FromQuery(Name = SortingQueryParameters.Sort)] string? Sort = null,
        [FromQuery(Name = PagingQueryParameters.Offset)] int Offset = PagingDefaults.DefaultOffset,
        [FromQuery(Name = PagingQueryParameters.Limit)] int Limit = PagingDefaults.DefaultLimit) : ISortableQuery<RosterPlayer>, IPagedQuery
    {
        // MemberFrom is nullable and Postgres puts NULL first when descending, so an unknown date counts as the
        // earliest one: the dated roster player comes first in a descending sort.
        public static SortMap<RosterPlayer> SortMap { get; } = new SortMap<RosterPlayer>()
            .Add(PlayerCareerSortFields.Season, x => x.Roster.Season.StartYear)
            .Add(PlayerCareerSortFields.MemberFrom, x => x.MemberFrom ?? DateOnly.MinValue)
            .WithDefault($"-{PlayerCareerSortFields.Season},-{PlayerCareerSortFields.MemberFrom}");
    }
}
