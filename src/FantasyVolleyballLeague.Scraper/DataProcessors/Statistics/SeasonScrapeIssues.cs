using FantasyVolleyballLeague.Scraper.Storage;

namespace FantasyVolleyballLeague.Scraper.DataProcessors.Statistics
{
    internal sealed record SeasonScrapeIssues(IReadOnlyList<MatchFailure> Failed, IReadOnlyList<MatchFailure> WithoutStatistics);
}
