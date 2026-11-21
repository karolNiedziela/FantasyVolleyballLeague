using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetSeasonRounds
{
    internal sealed class GetSeasonRoundsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(RoundsRoutes.BySeason, GetSeasonRounds)
                .WithName(nameof(GetSeasonRounds))
                .WithSummary("List rounds of a season")
                .WithDescription("Returns a page of the rounds of one season, optionally of one phase, in a fixed order: by phase, stage and round order. Same as listing rounds with the seasonId filter, but answers 404 when the season does not exist.")
                .WithTags(RoundsRoutes.Tag)
                .WithPhaseFilter()
                .WithPaging()
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<PagedOk<RoundSummaryResponse>, NotFound>> GetSeasonRounds(
            [AsParameters] GetSeasonRoundsQuery query,
            IQueryHandler<GetSeasonRoundsQuery, PagedResponse<RoundSummaryResponse>?> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(query, cancellationToken);

            if (result is null)
            {
                return TypedResults.NotFound();
            }

            return PagedResults.Ok(result);
        }
    }
}
