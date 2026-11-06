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

        /// <summary>
        /// Time to wait for a statistics widget to render, per attempt.
        /// </summary>
        public int WidgetTimeoutMs { get; set; } = 15000;

        /// <summary>
        /// How many times a failed widget load is retried before the match is marked as failed.
        /// </summary>
        public int WidgetMaxRetries { get; set; } = 2;

        /// <summary>
        /// Delay before the first retry. It doubles with every following retry.
        /// </summary>
        public int WidgetRetryDelayMs { get; set; } = 2000;

        /// <summary>
        /// Minimum time between the starts of two widget loads, shared by all parallel tasks.
        /// The widget host blocks clients that send too many requests.
        /// </summary>
        public int WidgetMinIntervalMs { get; set; } = 500;
    }
}
