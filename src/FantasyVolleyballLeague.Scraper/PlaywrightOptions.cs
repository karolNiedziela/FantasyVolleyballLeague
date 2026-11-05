namespace FantasyVolleyballLeague.Scraper
{
    internal sealed class PlaywrightOptions
    {
        public const string SectionName = "Playwright";

        public bool Headless { get; set; } = true;

        /// <summary>
        /// Maximum number of matches (or roster players) scraped in parallel.
        /// Keep it low to avoid overloading the league websites.
        /// </summary>
        public int MaxConcurrentTasks { get; set; } = 3;
    }
}
