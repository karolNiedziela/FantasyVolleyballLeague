using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Search;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetSeasonRosters
{
    internal sealed class GetSeasonRostersQueryHandler : IQueryHandler<GetSeasonRostersQuery, PagedResponse<RosterResponse>?>
    {
        private readonly IDbContext _dbContext;

        public GetSeasonRostersQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<RosterResponse>?> HandleAsync(GetSeasonRostersQuery query, CancellationToken cancellationToken)
        {
            var seasonExists = await _dbContext.Set<Season>()
                .AnyAsync(x => x.Id == query.SeasonId, cancellationToken);

            if (!seasonExists)
            {
                return null;
            }

            return await _dbContext.Set<Roster>()
                .WhereSeason(query.SeasonId)
                .WhereNameMatches(query.Search)
                .ApplySort(query)
                .ProjectToResponse()
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
