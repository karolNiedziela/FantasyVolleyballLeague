using FantasyVolleyballLeague.DataApi.Common.API;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues
{
    internal sealed class GetLeaguesResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public short DivisionNumber { get; set; }

        public LookupResponse Country { get; set; } = new();

        public LookupResponse Gender { get; set; } = new();
    }
}
