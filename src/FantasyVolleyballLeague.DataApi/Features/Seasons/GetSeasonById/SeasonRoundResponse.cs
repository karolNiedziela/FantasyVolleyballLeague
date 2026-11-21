using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasonById
{
    internal sealed class SeasonRoundResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public int Order { get; init; }

        /// <summary>
        /// The stage of a playoff round (free text from the source); null for rounds without a stage, such as the regular season.
        /// </summary>
        public StageSummaryResponse? Stage { get; init; }
    }
}
