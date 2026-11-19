using FantasyVolleyballLeague.DataApi.Common.API;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagueById
{
    internal sealed class GetLeagueByIdResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public short DivisionNumber { get; set; }

        public LookupResponse Country { get; set; } = new();

        public LookupResponse Gender { get; set; } = new();

        public SeasonSummaryResponse? CurrentSeason { get; set; }
    }
}
