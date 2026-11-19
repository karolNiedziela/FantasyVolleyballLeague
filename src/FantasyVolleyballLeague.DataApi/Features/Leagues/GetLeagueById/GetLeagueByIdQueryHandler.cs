using FantasyVolleyballLeague.DataApi.Common.API;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagueById
{
    internal sealed class GetLeagueByIdQueryHandler : IQueryHandler<GetLeagueByIdQuery, GetLeagueByIdResponse?>
    {
        private readonly IDbContext _dbContext;

        public GetLeagueByIdQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetLeagueByIdResponse?> HandleAsync(GetLeagueByIdQuery query, CancellationToken cancellationToken)
        {
            var seasons = _dbContext.Set<Season>();

            return await _dbContext.Set<League>()
                .Where(x => x.Id == query.Id)
                .Select(x => new GetLeagueByIdResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionNumber = x.DivisionNumber,
                    Country = new LookupResponse { Id = x.Country.Id, Name = x.Country.Name },
                    Gender = new LookupResponse { Id = x.Gender.Id, Name = x.Gender.Name },
                    CurrentSeason = seasons
                        .Where(season => season.LeagueId == x.Id)
                        .OrderByDescending(season => season.StartYear)
                        .Select(season => new SeasonSummaryResponse
                        {
                            Id = season.Id,
                            Name = season.Name,
                            StartYear = season.StartYear,
                            EndYear = season.EndYear
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
