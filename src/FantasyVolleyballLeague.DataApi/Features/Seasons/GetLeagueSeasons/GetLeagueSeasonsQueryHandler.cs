using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetLeagueSeasons
{
    internal sealed class GetLeagueSeasonsQueryHandler : IQueryHandler<GetLeagueSeasonsQuery, PagedResponse<SeasonResponse>?>
    {
        private readonly IDbContext _dbContext;

        public GetLeagueSeasonsQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<SeasonResponse>?> HandleAsync(GetLeagueSeasonsQuery query, CancellationToken cancellationToken)
        {
            var leagueExists = await _dbContext.Set<League>()
                .AnyAsync(x => x.Id == query.LeagueId, cancellationToken);

            if (!leagueExists)
            {
                return null;
            }

            var allSeasons = _dbContext.Set<Season>();

            return await allSeasons
                .Where(x => x.LeagueId == query.LeagueId)
                .WhereStartYear(query.StartYear)
                .WhereCurrent(query.Current, allSeasons)
                .ApplySort(query)
                .ProjectToResponse(allSeasons)
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
