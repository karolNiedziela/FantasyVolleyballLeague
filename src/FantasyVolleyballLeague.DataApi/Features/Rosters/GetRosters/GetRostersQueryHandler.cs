using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Search;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosters
{
    internal sealed class GetRostersQueryHandler : IQueryHandler<GetRostersQuery, PagedResponse<RosterResponse>>
    {
        private readonly IDbContext _dbContext;

        public GetRostersQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<RosterResponse>> HandleAsync(GetRostersQuery query, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<Roster>()
                .WhereLeague(query.LeagueId)
                .WhereSeason(query.SeasonId)
                .WhereNameMatches(query.Search)
                .ApplySort(query)
                .ProjectToResponse()
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
