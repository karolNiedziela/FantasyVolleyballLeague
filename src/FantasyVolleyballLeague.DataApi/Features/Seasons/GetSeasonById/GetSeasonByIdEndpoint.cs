using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasonById
{
    internal sealed class GetSeasonByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(SeasonsRoutes.ResourceById, GetSeasonById)
                .WithName(nameof(GetSeasonById))
                .WithSummary("Get season by ID")
                .WithDescription("Returns a season with its league, the previous and next season of the league, and its phases with their rounds.")
                .WithTags(SeasonsRoutes.Tag)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<Ok<GetSeasonByIdResponse>, NotFound>> GetSeasonById(
            [AsParameters] GetSeasonByIdQuery query,
            IQueryHandler<GetSeasonByIdQuery, GetSeasonByIdResponse?> handler,
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
