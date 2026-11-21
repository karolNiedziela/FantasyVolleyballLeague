using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasons
{
    internal sealed class GetSeasonsQueryHandler : IQueryHandler<GetSeasonsQuery, PagedResponse<SeasonResponse>>
    {
        private readonly IDbContext _dbContext;

        public GetSeasonsQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<SeasonResponse>> HandleAsync(GetSeasonsQuery query, CancellationToken cancellationToken)
        {
            var allSeasons = _dbContext.Set<Season>();
            var seasons = allSeasons.AsQueryable();

            if (query.LeagueId.HasValue)
            {
                seasons = seasons.Where(x => x.LeagueId == query.LeagueId.Value);
            }

            return await seasons
                .WhereStartYear(query.StartYear)
                .WhereCurrent(query.Current, allSeasons)
                .ApplySort(query)
                .ProjectToResponse(allSeasons)
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
