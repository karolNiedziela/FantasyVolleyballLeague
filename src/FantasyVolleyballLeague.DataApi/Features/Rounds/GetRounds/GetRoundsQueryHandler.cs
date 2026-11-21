using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetRounds
{
    internal sealed class GetRoundsQueryHandler : IQueryHandler<GetRoundsQuery, PagedResponse<RoundSummaryResponse>>
    {
        private readonly IDbContext _dbContext;

        public GetRoundsQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<RoundSummaryResponse>> HandleAsync(GetRoundsQuery query, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<Round>()
                .WhereSeason(query.SeasonId)
                .WherePhase(query.PhaseId)
                .OrderByPhaseStageRound()
                .ProjectToSummary()
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
