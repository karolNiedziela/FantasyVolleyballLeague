using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerById
{
    internal sealed class GetPlayerByIdQueryHandler : IQueryHandler<GetPlayerByIdQuery, GetPlayerByIdResponse?>
    {
        private readonly IDbContext _dbContext;

        public GetPlayerByIdQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetPlayerByIdResponse?> HandleAsync(GetPlayerByIdQuery query, CancellationToken cancellationToken)
        {
            var player = await _dbContext.Set<Player>()
                .Where(x => x.Id == query.Id)
                .Select(x => new
                {
                    x.Id,
                    x.ExternalId,
                    x.FullName,
                    Gender = x.Gender.Code,
                    x.DateOfBirth,
                    x.HeightCm,
                    x.WeightKg,
                    x.AttackReachCm
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (player is null)
            {
                return null;
            }

            var currentRoster = await LoadCurrentRosterAsync(query.Id, cancellationToken);

            return new GetPlayerByIdResponse
            {
                Id = player.Id,
                ExternalId = player.ExternalId,
                FullName = player.FullName,
                Gender = player.Gender,
                DateOfBirth = player.DateOfBirth,
                HeightCm = player.HeightCm,
                WeightKg = player.WeightKg,
                AttackReachCm = player.AttackReachCm,
                CurrentRoster = currentRoster
            };
        }

        private async Task<CurrentRosterResponse?> LoadCurrentRosterAsync(Guid playerId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<RosterPlayer>()
                .Where(x => x.PlayerId == playerId)
                .OrderByMostRecent()
                .Select(x => new CurrentRosterResponse
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
                    ShirtNumber = x.ShirtNumber,
                    Position = x.Position == null ? null : x.Position.Code
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
