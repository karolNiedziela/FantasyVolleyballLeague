using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetSeasonRosters
{
    internal sealed class GetSeasonRostersEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(RostersRoutes.BySeason, GetSeasonRosters)
                .WithName(nameof(GetSeasonRosters))
                .WithSummary("List rosters of a season")
                .WithDescription("Returns a page of the rosters of one season, optionally filtered by words of the name and sorted by name or season start year. Same as listing rosters with the seasonId filter, but answers 404 when the season does not exist.")
                .WithTags(RostersRoutes.Tag)
                .WithNameSearch()
                .WithSorting<GetSeasonRostersQuery, Roster>()
                .WithPaging()
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<Results<PagedOk<RosterResponse>, NotFound>> GetSeasonRosters(
            [AsParameters] GetSeasonRostersQuery query,
            IQueryHandler<GetSeasonRostersQuery, PagedResponse<RosterResponse>?> handler,
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
