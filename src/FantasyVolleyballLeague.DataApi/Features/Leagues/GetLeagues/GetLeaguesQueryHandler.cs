using FantasyVolleyballLeague.DataApi.Common.API;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues
{
    internal sealed class GetLeaguesQueryHandler : IQueryHandler<GetLeaguesQuery, PagedResponse<GetLeaguesResponse>>
    {
        private readonly FantasyVolleyballLeagueDbContext _dbContext;

        public GetLeaguesQueryHandler(FantasyVolleyballLeagueDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<GetLeaguesResponse>> HandleAsync(GetLeaguesQuery query, CancellationToken cancellationToken)
        {
            var leagues = _dbContext.Set<League>().AsQueryable(); 

            if (query.Gender.HasValue)
            {
                leagues = leagues.Where(x => x.GenderId == query.Gender.Value);
            }

            if (query.Division.HasValue)
            {
                leagues = leagues.Where(x => x.DivisionNumber == query.Division.Value);
            }

            return await leagues
                .ApplySort(query)
                .Select(x => new GetLeaguesResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionNumber = x.DivisionNumber,
                    Country = new LookupResponse { Id = x.Country.Id, Name = x.Country.Name },
                    Gender = new LookupResponse { Id = x.Gender.Id, Name = x.Gender.Name }
                })
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
