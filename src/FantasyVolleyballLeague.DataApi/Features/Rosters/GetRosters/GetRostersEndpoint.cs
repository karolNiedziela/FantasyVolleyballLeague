using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosters
{
    internal sealed class GetRostersEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(RostersRoutes.Resource, GetRosters)
                .WithName(nameof(GetRosters))
                .WithSummary("List rosters")
                .WithDescription("Returns a page of rosters (a club in one season), optionally filtered by league, season and words of the name, and sorted by name or season start year.")
                .WithTags(RostersRoutes.Tag)
                .WithLeagueFilter()
                .WithSeasonFilter()
                .WithNameSearch()
                .WithSorting<GetRostersQuery, Roster>()
                .WithPaging();
        }

        private static async Task<PagedOk<RosterResponse>> GetRosters(
            [AsParameters] GetRostersQuery query,
            IQueryHandler<GetRostersQuery, PagedResponse<RosterResponse>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(query, cancellationToken);

            return PagedResults.Ok(result);
        }
    }
}
