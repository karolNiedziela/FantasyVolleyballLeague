using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerById
{
    internal sealed class GetPlayerByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(PlayersRoutes.ResourceById, GetPlayerById)
                .WithName(nameof(GetPlayerById))
                .WithSummary("Get player by ID")
                .WithDescription("Returns a player with the profile and the current roster: the roster, the season, the shirt number and the position of the player's most recent roster entry.")
                .WithTags(PlayersRoutes.Tag)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<Ok<GetPlayerByIdResponse>, NotFound>> GetPlayerById(
            [AsParameters] GetPlayerByIdQuery query,
            IQueryHandler<GetPlayerByIdQuery, GetPlayerByIdResponse?> handler,
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
