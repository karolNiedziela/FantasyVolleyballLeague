namespace FantasyVolleyballLeague.Worker
{
    public class LeagueOptions
    {
        public const string SectionName = "Leagues";

        public string Name { get; set; } = string.Empty;

        public string GamesUrl { get; set; } = string.Empty;

        public string MatchDetailsBaseUrl { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
