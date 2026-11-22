using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosterById
{
    internal sealed class GetRosterByIdQueryHandler : IQueryHandler<GetRosterByIdQuery, GetRosterByIdResponse?>
    {
        private readonly IDbContext _dbContext;

        public GetRosterByIdQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetRosterByIdResponse?> HandleAsync(GetRosterByIdQuery query, CancellationToken cancellationToken)
        {
            var roster = await _dbContext.Set<Roster>()
                .Where(x => x.Id == query.Id)
                .Select(x => new
                {
                    x.Id,
                    x.ExternalId,
                    x.Name,
                    League = new LeagueSummaryResponse
                    {
                        Id = x.League.Id,
                        Name = x.League.Name,
                        DivisionNumber = x.League.DivisionNumber,
                        Country = x.League.Country.Code,
                        Gender = x.League.Gender.Code
                    },
                    Season = new SeasonSummaryResponse
                    {
                        Id = x.Season.Id,
                        Name = x.Season.Name,
                        StartYear = x.Season.StartYear,
                        EndYear = x.Season.EndYear
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (roster is null)
            {
                return null;
            }

            var players = await LoadPlayersAsync(query.Id, cancellationToken);

            return new GetRosterByIdResponse
            {
                Id = roster.Id,
                ExternalId = roster.ExternalId,
                Name = roster.Name,
                League = roster.League,
                Season = roster.Season,
                Players = players
            };
        }

        private async Task<List<RosterPlayerResponse>> LoadPlayersAsync(Guid rosterId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<RosterPlayer>()
                .Where(x => x.RosterId == rosterId)
                .OrderBy(x => x.ShirtNumber == null)
                .ThenBy(x => x.ShirtNumber)
                .ThenBy(x => x.Player.FullName)
                .ThenBy(x => x.Id)
                .Select(x => new RosterPlayerResponse
                {
                    PlayerId = x.PlayerId,
                    FullName = x.Player.FullName,
                    DateOfBirth = x.Player.DateOfBirth,
                    HeightCm = x.Player.HeightCm,
                    WeightKg = x.Player.WeightKg,
                    AttackReachCm = x.Player.AttackReachCm,
                    ShirtNumber = x.ShirtNumber,
                    Position = x.Position == null ? null : x.Position.Code,
                    MemberFrom = x.MemberFrom,
                    MemberTo = x.MemberTo
                })
                .ToListAsync(cancellationToken);
        }
    }
}
