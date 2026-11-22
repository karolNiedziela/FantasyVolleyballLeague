using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetSeasonPlayers
{
    internal sealed class GetSeasonPlayersEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(PlayersRoutes.BySeason, GetSeasonPlayers)
                .WithName(nameof(GetSeasonPlayers))
                .WithSummary("List players of a season")
                .WithDescription("Returns a page of the players who were on a roster of one season, optionally filtered by position, roster and words of the name, sorted by name. The position is the one from that season. Same as listing players with the seasonId filter, but answers 404 when the season does not exist.")
                .WithTags(PlayersRoutes.Tag)
                .WithPositionFilter()
                .WithRosterFilter()
                .WithNameSearch()
                .WithSorting<GetSeasonPlayersQuery, Player>()
                .WithPaging()
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<PagedOk<PlayerResponse>, NotFound>> GetSeasonPlayers(
            [AsParameters] GetSeasonPlayersQuery query,
            IQueryHandler<GetSeasonPlayersQuery, PagedResponse<PlayerResponse>?> handler,
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
