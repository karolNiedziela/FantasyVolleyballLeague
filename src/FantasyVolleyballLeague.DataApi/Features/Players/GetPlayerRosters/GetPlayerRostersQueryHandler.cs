using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerRosters
{
    internal sealed class GetPlayerRostersQueryHandler : IQueryHandler<GetPlayerRostersQuery, PagedResponse<PlayerCareerResponse>?>
    {
        private readonly IDbContext _dbContext;

        public GetPlayerRostersQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<PlayerCareerResponse>?> HandleAsync(GetPlayerRostersQuery query, CancellationToken cancellationToken)
        {
            var playerExists = await _dbContext.Set<Player>()
                .AnyAsync(x => x.Id == query.PlayerId, cancellationToken);

            if (!playerExists)
            {
                return null;
            }

            return await _dbContext.Set<RosterPlayer>()
                .Where(x => x.PlayerId == query.PlayerId)
                .ApplySort(query)
                .Select(x => new PlayerCareerResponse
                {
                    Roster = new RosterSummaryResponse
                    {
                        Id = x.RosterId,
                        Name = x.Roster.Name
                    },
                    Season = new SeasonSummaryResponse
                    {
                        Id = x.Roster.Season.Id,
                        Name = x.Roster.Season.Name,
                        StartYear = x.Roster.Season.StartYear,
                        EndYear = x.Roster.Season.EndYear
                    },
                    League = new LeagueSummaryResponse
                    {
                        Id = x.Roster.League.Id,
                        Name = x.Roster.League.Name,
                        DivisionNumber = x.Roster.League.DivisionNumber,
                        Country = x.Roster.League.Country.Code,
                        Gender = x.Roster.League.Gender.Code
                    },
                    ShirtNumber = x.ShirtNumber,
                    Position = x.Position == null ? null : x.Position.Code,
                    MemberFrom = x.MemberFrom,
                    MemberTo = x.MemberTo
                })
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
