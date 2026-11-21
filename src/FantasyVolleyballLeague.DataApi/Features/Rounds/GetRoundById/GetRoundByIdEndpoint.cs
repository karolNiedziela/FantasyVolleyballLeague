using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetRoundById
{
    internal sealed class GetRoundByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(RoundsRoutes.ResourceById, GetRoundById)
                .WithName(nameof(GetRoundById))
                .WithSummary("Get round by ID")
                .WithDescription("Returns a round with its phase and season, and the previous and next round of the season. The rounds of a season are ordered by phase, stage and round order, so a neighbour can belong to another phase or stage.")
                .WithTags(RoundsRoutes.Tag)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<Ok<GetRoundByIdResponse>, NotFound>> GetRoundById(
            [AsParameters] GetRoundByIdQuery query,
            IQueryHandler<GetRoundByIdQuery, GetRoundByIdResponse?> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(query, cancellationToken);

            if (result is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(result);
        }
    }
}
