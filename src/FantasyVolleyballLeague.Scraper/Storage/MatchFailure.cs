namespace FantasyVolleyballLeague.Scraper.Storage
{
    /// <summary>
    /// A match that could not be scraped completely, with the number of failed attempts so far.
    /// </summary>
    internal sealed record MatchFailure
    {
        public required int MatchId { get; init; }

        /// <summary>
        /// Failed scrape attempts (runs). Website blocks are not counted.
        /// </summary>
        public required int Attempts { get; init; }

        public string? LastError { get; init; }

        public required DateTime LastAttemptUtc { get; init; }
    }
}
