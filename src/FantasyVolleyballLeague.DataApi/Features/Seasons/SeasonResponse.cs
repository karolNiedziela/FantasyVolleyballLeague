namespace FantasyVolleyballLeague.DataApi.Features.Seasons
{
    internal sealed class SeasonResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public int StartYear { get; init; }

        public int EndYear { get; init; }

        public bool IsCurrent { get; init; }

        public Guid LeagueId { get; init; }
    }
}
