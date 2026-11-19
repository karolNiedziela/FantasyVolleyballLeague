namespace FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues
{
    internal sealed class GetLeaguesResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public short DivisionNumber { get; set; }

        public string Country { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;
    }
}
