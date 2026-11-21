using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds.GetRoundById
{
    internal sealed class GetRoundByIdResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public int Order { get; init; }

        /// <summary>
        /// The stage of a playoff round; null for rounds without a stage, such as the regular season.
        /// </summary>
        public StageSummaryResponse? Stage { get; init; }

        public PhaseSummaryResponse Phase { get; init; } = new();

        public SeasonSummaryResponse Season { get; init; } = new();

        /// <summary>
        /// The round before this one in the season, ordered by phase, stage and round order. It can belong to an earlier
        /// phase or stage. Null for the first round of the season.
        /// </summary>
        public RoundSummaryResponse? PreviousRound { get; init; }

        /// <summary>
        /// The round after this one in the season, ordered by phase, stage and round order. It can belong to a later
        /// phase or stage. Null for the last round of the season.
        /// </summary>
        public RoundSummaryResponse? NextRound { get; init; }
    }
}
