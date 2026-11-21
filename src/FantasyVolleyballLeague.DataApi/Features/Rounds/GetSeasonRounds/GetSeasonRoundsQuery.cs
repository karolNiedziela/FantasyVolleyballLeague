using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetSeasonRounds
{
    internal sealed record GetSeasonRoundsQuery(
        Guid SeasonId,
        [FromQuery(Name = RoundQueryParameters.PhaseId)] Guid? PhaseId = null,
        [FromQuery(Name = PagingQueryParameters.Offset)] int Offset = PagingDefaults.DefaultOffset,
        [FromQuery(Name = PagingQueryParameters.Limit)] int Limit = PagingDefaults.DefaultLimit) : IPagedQuery;
}
