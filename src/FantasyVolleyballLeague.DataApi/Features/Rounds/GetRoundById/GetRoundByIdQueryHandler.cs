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

            var rounds = await _dbContext.Set<Round>()
                .WhereSeason(season.Id)
                .OrderByPhaseStageRound()
                .ProjectToSummary()
                .ToListAsync(cancellationToken);

            var index = rounds.FindIndex(round => round.Id == query.Id);
            var current = rounds[index];

            return new GetRoundByIdResponse
            {
                Id = current.Id,
                Name = current.Name,
                Order = current.Order,
                Stage = current.Stage,
                Phase = current.Phase,
                Season = season,
                PreviousRound = index > 0 ? rounds[index - 1] : null,
                NextRound = index < rounds.Count - 1 ? rounds[index + 1] : null
            };
        }
    }
}
