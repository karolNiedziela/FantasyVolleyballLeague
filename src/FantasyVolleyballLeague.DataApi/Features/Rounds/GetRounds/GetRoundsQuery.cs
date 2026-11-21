using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetRounds
{
    internal sealed record GetRoundsQuery(
        [FromQuery(Name = RoundQueryParameters.SeasonId)] Guid SeasonId,
        [FromQuery(Name = RoundQueryParameters.PhaseId)] Guid? PhaseId = null,
        [FromQuery(Name = PagingQueryParameters.Offset)] int Offset = PagingDefaults.DefaultOffset,
        [FromQuery(Name = PagingQueryParameters.Limit)] int Limit = PagingDefaults.DefaultLimit) : IPagedQuery;
}
