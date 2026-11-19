using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues
{
    internal sealed class GetLeaguesQueryHandler : IQueryHandler<GetLeaguesQuery, PagedResponse<GetLeaguesResponse>>
    {
        private readonly IDbContext _dbContext;

        public GetLeaguesQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<GetLeaguesResponse>> HandleAsync(GetLeaguesQuery query, CancellationToken cancellationToken)
        {
            var leagues = _dbContext.Set<League>().AsQueryable(); 

            if (query.Gender is not null)
            {
                var genderId = LeagueGender.FromCode(query.Gender)?.Id;
                leagues = leagues.Where(x => x.GenderId == genderId);
            }

            if (query.Division.HasValue)
            {
                leagues = leagues.Where(x => x.DivisionNumber == query.Division.Value);
            }

            if (query.Country is not null)
            {
                var countryId = LeagueCountry.FromCode(query.Country)?.Id;
                leagues = leagues.Where(x => x.CountryId == countryId);
            }

            return await leagues
                .ApplySort(query)
                .Select(x => new GetLeaguesResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionNumber = x.DivisionNumber,
                    Country = x.Country.Code,
                    Gender = x.Gender.Code
                })
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
