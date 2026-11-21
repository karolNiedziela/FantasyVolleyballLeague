using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetRoundById
{
    internal sealed class GetRoundByIdQueryHandler : IQueryHandler<GetRoundByIdQuery, GetRoundByIdResponse?>
    {
        private readonly IDbContext _dbContext;

        public GetRoundByIdQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetRoundByIdResponse?> HandleAsync(GetRoundByIdQuery query, CancellationToken cancellationToken)
        {
            var season = await _dbContext.Set<Round>()
                .Where(x => x.Id == query.Id)
                .Select(x => new SeasonSummaryResponse
                {
                    Id = x.Phase.Season.Id,
                    Name = x.Phase.Season.Name,
                    StartYear = x.Phase.Season.StartYear,
                    EndYear = x.Phase.Season.EndYear
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (season is null)
            {
                return null;
            }

            var rounds = await LoadSeasonRoundsAsync(season.Id, cancellationToken);

            var index = rounds.FindIndex(round => round.Id == query.Id);
            if (index < 0)
            {
                return null;
            }

            var current = ToSummary(rounds[index]);

            return new GetRoundByIdResponse
            {
                Id = current.Id,
                Name = current.Name,
                Order = current.Order,
                Stage = current.Stage,
                Phase = current.Phase,
                Season = season,
                PreviousRound = index > 0 ? ToSummary(rounds[index - 1]) : null,
                NextRound = index < rounds.Count - 1 ? ToSummary(rounds[index + 1]) : null
            };
        }

        private async Task<List<RoundRow>> LoadSeasonRoundsAsync(Guid seasonId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<Round>()
                .Where(x => x.Phase.SeasonId == seasonId)
                .OrderBy(x => x.Phase.Order)
                .ThenBy(x => x.StageOrder)
                .ThenBy(x => x.Order)
                .ThenBy(x => x.Id)
                .Select(x => new RoundRow(
                    x.Id,
                    x.Name,
                    x.Order,
                    x.Stage,
                    x.StageOrder,
                    x.PhaseId,
                    x.Phase.Name,
                    x.Phase.PhaseType.Code,
                    x.Phase.Order))
                .ToListAsync(cancellationToken);
        }

        private static RoundSummaryResponse ToSummary(RoundRow row)
        {
            return new RoundSummaryResponse
            {
                Id = row.Id,
                Name = row.Name,
                Order = row.Order,
                Stage = string.IsNullOrEmpty(row.StageName)
                    ? null
                    : new StageSummaryResponse { Name = row.StageName, Order = row.StageOrder },
                Phase = new PhaseSummaryResponse
                {
                    Id = row.PhaseId,
                    Name = row.PhaseName,
                    Type = row.PhaseType,
                    Order = row.PhaseOrder
                }
            };
        }

        private sealed record RoundRow(
            Guid Id,
            string Name,
            int Order,
            string StageName,
            int StageOrder,
            Guid PhaseId,
            string PhaseName,
            string PhaseType,
            int PhaseOrder);
    }
}
