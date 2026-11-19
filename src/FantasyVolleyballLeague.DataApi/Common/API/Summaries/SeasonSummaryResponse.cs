namespace FantasyVolleyballLeague.DataApi.Common.API.Summaries
{
    internal sealed class SeasonSummaryResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public int StartYear { get; init; }

        public int EndYear { get; init; }
    }
}
