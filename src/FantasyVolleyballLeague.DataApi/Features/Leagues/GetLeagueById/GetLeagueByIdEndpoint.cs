using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagueById
{
    internal sealed class GetLeagueByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(LeaguesRoutes.ResourceById, GetLeagueById)
                .WithName("GetLeagueById")
                .WithSummary("Get league by ID")
                .WithDescription("Returns a league by its ID.")
                .WithTags(LeaguesRoutes.Tag)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<Ok<GetLeagueByIdResponse>, NotFound>> GetLeagueById(
            [AsParameters] GetLeagueByIdQuery query,
            IQueryHandler<GetLeagueByIdQuery, GetLeagueByIdResponse?> handler,
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