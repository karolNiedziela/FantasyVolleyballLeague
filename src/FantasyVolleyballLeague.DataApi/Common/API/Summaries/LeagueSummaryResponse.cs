namespace FantasyVolleyballLeague.DataApi.Common.API.Summaries
{
    internal sealed class LeagueSummaryResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public short DivisionNumber { get; init; }

        public string Country { get; init; } = string.Empty;

        public string Gender { get; init; } = string.Empty;
    }
}
