using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds
{
    internal static class RoundQueryExtensions
    {
        public static IQueryable<Round> WhereSeason(this IQueryable<Round> rounds, Guid seasonId)
        {
            return rounds.Where(x => x.Phase.SeasonId == seasonId);
        }

        public static IQueryable<Round> WherePhase(this IQueryable<Round> rounds, Guid? phaseId)
        {
            if (!phaseId.HasValue)
            {
                return rounds;
            }

            return rounds.Where(x => x.PhaseId == phaseId.Value);
        }

        public static IOrderedQueryable<Round> OrderByPhaseStageRound(this IQueryable<Round> rounds)
        {
            return rounds
                .OrderBy(x => x.Phase.Order)
                .ThenBy(x => x.StageOrder)
                .ThenBy(x => x.Order)
                .ThenBy(x => x.Id);
        }

        public static IQueryable<RoundSummaryResponse> ProjectToSummary(this IQueryable<Round> rounds)
        {
            return rounds.Select(x => new RoundSummaryResponse
            {
                Id = x.Id,
                Name = x.Name,
                Order = x.Order,
                Stage = x.Stage == string.Empty
                    ? null
                    : new StageSummaryResponse { Name = x.Stage, Order = x.StageOrder },
                Phase = new PhaseSummaryResponse
                {
                    Id = x.Phase.Id,
                    Name = x.Phase.Name,
                    Type = x.Phase.PhaseType.Code,
                    Order = x.Phase.Order
                }
            });
        }
    }
}
