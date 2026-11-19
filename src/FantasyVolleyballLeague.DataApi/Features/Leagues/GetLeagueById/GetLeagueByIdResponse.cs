using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagueById
{
    internal sealed class GetLeagueByIdResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public short DivisionNumber { get; set; }

        public string Country { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public SeasonSummaryResponse? CurrentSeason { get; set; }
    }
}
