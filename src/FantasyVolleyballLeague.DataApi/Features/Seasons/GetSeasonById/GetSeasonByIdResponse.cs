using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasonById
{
    internal sealed class GetSeasonByIdResponse
    {
        public Guid Id { get; init; }

        public int? ExternalId { get; init; }

        public string Name { get; init; } = string.Empty;

        public int StartYear { get; init; }

        public int EndYear { get; init; }

        public string LeagueName { get; init; } = string.Empty;

        public bool IsCurrent { get; init; }

        public LeagueSummaryResponse League { get; init; } = new();

        public SeasonSummaryResponse? PreviousSeason { get; init; }

        public SeasonSummaryResponse? NextSeason { get; init; }

        public IReadOnlyList<SeasonPhaseResponse> Phases { get; init; } = [];
    }
}
