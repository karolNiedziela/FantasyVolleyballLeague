using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues
{
    internal sealed class GetLeaguesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(LeaguesRoutes.Resource, GetLeagues)
                .WithName("GetLeagues")
                .WithSummary("List leagues")
                .WithDescription("Returns a page of leagues, optionally filtered by gender and division and sorted by name or division number.")
                .WithTags(LeaguesRoutes.Tag)
                .WithGenderFilter()
                .WithDivisionFilter()
                .WithSorting<GetLeaguesQuery, League>()
                .WithPaging();
        }

        private static async Task<PagedOk<GetLeaguesResponse>> GetLeagues(
            [AsParameters] GetLeaguesQuery query,
            GetLeaguesQueryHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(query, cancellationToken);

            return PagedResults.Ok(result);
        }
    }
}
