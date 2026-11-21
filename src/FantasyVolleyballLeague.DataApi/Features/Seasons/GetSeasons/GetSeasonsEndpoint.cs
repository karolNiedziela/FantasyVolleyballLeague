using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasons
{
    internal sealed class GetSeasonsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(SeasonsRoutes.Resource, GetSeasons)
                .WithName(nameof(GetSeasons))
                .WithSummary("List seasons")
                .WithDescription("Returns a page of seasons, optionally filtered by league, start year and current season and sorted by start year.")
                .WithTags(SeasonsRoutes.Tag)
                .WithLeagueFilter()
                .WithStartYearFilter()
                .WithCurrentSeasonFilter()
                .WithSorting<GetSeasonsQuery, Season>()
                .WithPaging();
        }

        private static async Task<PagedOk<SeasonResponse>> GetSeasons(
            [AsParameters] GetSeasonsQuery query,
            IQueryHandler<GetSeasonsQuery, PagedResponse<SeasonResponse>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(query, cancellationToken);

            return PagedResults.Ok(result);
        }
    }
}
