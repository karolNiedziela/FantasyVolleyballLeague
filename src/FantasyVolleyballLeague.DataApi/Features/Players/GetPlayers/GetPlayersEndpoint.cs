using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayers
{
    internal sealed class GetPlayersEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(PlayersRoutes.Resource, GetPlayers)
                .WithName(nameof(GetPlayers))
                .WithSummary("List players")
                .WithDescription("Returns a page of players, optionally filtered by gender, position, roster, season, league and words of the name, sorted by name.")
                .WithTags(PlayersRoutes.Tag)
                .WithGenderFilter()
                .WithPositionFilter()
                .WithRosterFilter()
                .WithSeasonFilter()
                .WithLeagueFilter()
                .WithNameSearch()
                .WithSorting<GetPlayersQuery, Player>()
                .WithPaging();
        }

        private static async Task<PagedOk<PlayerResponse>> GetPlayers(
            [AsParameters] GetPlayersQuery query,
            IQueryHandler<GetPlayersQuery, PagedResponse<PlayerResponse>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(query, cancellationToken);

            return PagedResults.Ok(result);
        }
    }
}
