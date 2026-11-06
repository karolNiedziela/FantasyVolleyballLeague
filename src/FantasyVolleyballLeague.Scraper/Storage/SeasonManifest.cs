namespace FantasyVolleyballLeague.Scraper.Storage
{
    /// <summary>
    /// Scraping state of a season (<c>data/raw/matches/{League}/{YYYY-YYYY}/_manifest.json</c>).
    /// </summary>
    internal sealed record SeasonManifest
    {
        public int SchemaVersion { get; init; } = 1;

        public required DateTime ScrapedAtUtc { get; init; }

        /// <summary>
        /// The season is over and every match was scraped as finished. Complete seasons are skipped unless forced.
        /// </summary>
        public required bool IsComplete { get; init; }

        public required int MatchCount { get; init; }

        public required int FinishedCount { get; init; }

        public required int ScheduledCount { get; init; }

        /// <summary>
        /// Matches that failed in the last run. They have no file, so the next run retries them.
        /// </summary>
        public IReadOnlyList<MatchFailure> FailedMatches { get; init; } = [];

        /// <summary>
        /// Matches saved without player statistics after <see cref="JobOptions.MaxMatchAttempts"/> failed attempts
        /// (<c>StatisticsLevel = None</c>). They are not scraped again unless forced.
        /// </summary>
        public IReadOnlyList<MatchFailure> MatchesWithoutStatistics { get; init; } = [];
    }
}
