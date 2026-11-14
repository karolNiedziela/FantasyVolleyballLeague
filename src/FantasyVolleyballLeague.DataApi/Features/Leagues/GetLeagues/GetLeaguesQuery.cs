using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues
{
    internal sealed record GetLeaguesQuery(
        [FromQuery(Name = LeaguesQueryParameters.Gender)] int? Gender,
        [FromQuery(Name = LeaguesQueryParameters.Division)] int? Division,
        [FromQuery(Name = SortingQueryParameters.Sort)] string? Sort,
        [FromQuery(Name = PagingQueryParameters.Offset)] int Offset = PagingDefaults.DefaultOffset,
        [FromQuery(Name = PagingQueryParameters.Limit)] int Limit = PagingDefaults.DefaultLimit) : ISortableQuery<League>, IPagedQuery
    {
        public static SortMap<League> SortMap { get; } = new SortMap<League>()
            .Add(LeaguesSortFields.Name, x => x.Name)
            .Add(LeaguesSortFields.DivisionNumber, x => x.DivisionNumber)
            .WithDefault(LeaguesSortFields.Name);
    }
}
