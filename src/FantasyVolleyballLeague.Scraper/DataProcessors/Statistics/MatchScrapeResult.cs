using FantasyVolleyballLeague.Scraper.Storage;

namespace FantasyVolleyballLeague.Scraper.DataProcessors.Statistics
{
    internal sealed record MatchScrapeResult(MatchFailure? Failed, MatchFailure? WithoutStatistics)
    {
        public static readonly MatchScrapeResult Succeeded = new(null, null);

        public static MatchScrapeResult NotSaved(int matchId, int attempts, string? lastError)
        {
            return new MatchScrapeResult(NewFailure(matchId, attempts, lastError), null);
        }

        public static MatchScrapeResult SavedWithoutStatistics(int matchId, int attempts, string? lastError)
        {
            return new MatchScrapeResult(null, NewFailure(matchId, attempts, lastError));
        }

        private static MatchFailure NewFailure(int matchId, int attempts, string? lastError)
        {
            return new MatchFailure
            {
                MatchId = matchId,
                Attempts = attempts,
                LastError = lastError,
                LastAttemptUtc = DateTime.UtcNow,
            };
        }
    }
}
