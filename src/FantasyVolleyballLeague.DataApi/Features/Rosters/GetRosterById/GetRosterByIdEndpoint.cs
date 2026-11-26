using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosterById
{
    internal sealed class GetRosterByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(RostersRoutes.ResourceById, GetRosterById)
                .WithName(nameof(GetRosterById))
                .WithSummary("Get roster by ID")
                .WithDescription("Returns a roster (a club in one season) with its club (the same club across seasons and leagues), league and season and its players, by shirt number and then by name, each with the position and the dates the player belonged to the club, and its staff (coaches and the club president) by role and then by name.")
                .WithTags(RostersRoutes.Tag)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<Ok<GetRosterByIdResponse>, NotFound>> GetRosterById(
            [AsParameters] GetRosterByIdQuery query,
            IQueryHandler<GetRosterByIdQuery, GetRosterByIdResponse?> handler,
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
