using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters
{
    internal sealed class RosterResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public Guid LeagueId { get; init; }

        public SeasonSummaryResponse Season { get; init; } = new();
    }
}
