namespace FantasyVolleyballLeague.DataApi.Common.API.Summaries
{
    internal sealed class PhaseSummaryResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Code of the phase type: regular_season, playoff or relegation.
        /// </summary>
        public string Type { get; init; } = string.Empty;

        public int Order { get; init; }
    }
}
