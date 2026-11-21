namespace FantasyVolleyballLeague.DataApi.Common.API.Summaries
{
    internal sealed class RoundSummaryResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public int Order { get; init; }

        /// <summary>
        /// The stage of a playoff round; null for rounds without a stage, such as the regular season.
        /// </summary>
        public StageSummaryResponse? Stage { get; init; }

        public PhaseSummaryResponse Phase { get; init; } = new();
    }
}
