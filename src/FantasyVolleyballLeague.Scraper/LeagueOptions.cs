namespace FantasyVolleyballLeague.Scraper
{
    internal sealed class LeagueOptions
    {
        public const string SectionName = "Leagues";

        /// <summary>
        /// Stable identifier used in data folder names. Does not change when the league is rebranded.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Key of the <see cref="Sources.ILeagueSource"/> that scrapes the league, e.g. "Pls".
        /// </summary>
        public string Source { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = string.Empty;
    }
}
