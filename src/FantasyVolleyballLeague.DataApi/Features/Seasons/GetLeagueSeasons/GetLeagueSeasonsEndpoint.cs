using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetLeagueSeasons
{
    internal sealed class GetLeagueSeasonsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(SeasonsRoutes.ByLeague, GetLeagueSeasons)
                .WithName(nameof(GetLeagueSeasons))
                .WithSummary("List seasons of a league")
                .WithDescription("Returns a page of the seasons of one league, optionally filtered by start year and current season and sorted by start year. Same as listing seasons with the leagueId filter, but answers 404 when the league does not exist.")
                .WithTags(SeasonsRoutes.Tag)
                .WithStartYearFilter()
                .WithCurrentSeasonFilter()
                .WithSorting<GetLeagueSeasonsQuery, Season>()
                .WithPaging()
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<PagedOk<SeasonResponse>, NotFound>> GetLeagueSeasons(
            [AsParameters] GetLeagueSeasonsQuery query,
            IQueryHandler<GetLeagueSeasonsQuery, PagedResponse<SeasonResponse>?> handler,
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
