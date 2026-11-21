using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetRounds
{
    internal sealed class GetRoundsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(RoundsRoutes.Resource, GetRounds)
                .WithName(nameof(GetRounds))
                .WithSummary("List rounds")
                .WithDescription("Returns a page of the rounds of one season, optionally of one phase. The order is fixed: by phase, stage and round order.")
                .WithTags(RoundsRoutes.Tag)
                .WithSeasonFilter()
                .WithPhaseFilter()
                .WithPaging();
        }

        private static async Task<PagedOk<RoundSummaryResponse>> GetRounds(
            [AsParameters] GetRoundsQuery query,
            IQueryHandler<GetRoundsQuery, PagedResponse<RoundSummaryResponse>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(query, cancellationToken);

            return PagedResults.Ok(result);
        }
    }
}
