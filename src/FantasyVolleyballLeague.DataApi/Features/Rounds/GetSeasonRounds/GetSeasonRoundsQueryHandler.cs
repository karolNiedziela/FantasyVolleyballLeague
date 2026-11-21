using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetSeasonRounds
{
    internal sealed class GetSeasonRoundsQueryHandler : IQueryHandler<GetSeasonRoundsQuery, PagedResponse<RoundSummaryResponse>?>
    {
        private readonly IDbContext _dbContext;

        public GetSeasonRoundsQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<RoundSummaryResponse>?> HandleAsync(GetSeasonRoundsQuery query, CancellationToken cancellationToken)
        {
            var seasonExists = await _dbContext.Set<Season>()
                .AnyAsync(x => x.Id == query.SeasonId, cancellationToken);

            if (!seasonExists)
            {
                return null;
            }

            return await _dbContext.Set<Round>()
                .WhereSeason(query.SeasonId)
                .WherePhase(query.PhaseId)
                .OrderByPhaseStageRound()
                .ProjectToSummary()
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
