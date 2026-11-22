using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerRosters
{
    internal sealed class GetPlayerRostersEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(PlayersRoutes.RostersOfPlayer, GetPlayerRosters)
                .WithName(nameof(GetPlayerRosters))
                .WithSummary("List the rosters of a player")
                .WithDescription("Returns the career of a player: a page of the rosters (a club in one season) the player was on, newest season first, each with the league, the season, the shirt number, the position and the dates the player belonged to the club. Unlike the other nested routes this is not an alias of a top-level list: a roster player is a resource of its own with a DTO of its own. Answers 404 when the player does not exist.")
                .WithTags(PlayersRoutes.Tag)
                .WithSorting<GetPlayerRostersQuery, RosterPlayer>()
                .WithPaging()
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<PagedOk<PlayerCareerResponse>, NotFound>> GetPlayerRosters(
            [AsParameters] GetPlayerRostersQuery query,
            IQueryHandler<GetPlayerRostersQuery, PagedResponse<PlayerCareerResponse>?> handler,
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
