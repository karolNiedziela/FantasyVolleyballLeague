using System.Collections.Concurrent;
using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources;
using FantasyVolleyballLeague.Scraper.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Scraper.DataProcessors.Statistics
{
    /// <summary>
    /// <c>--job Matches</c>: scrapes match files of the selected leagues and seasons into the raw data store.
    /// Complete seasons and finished matches are skipped unless <c>--force</c> is set; scheduled and failed
    /// matches are scraped again on every run.
    /// </summary>
    internal sealed class StatisticsDataProcessor : IStatisticsDataProcessor
    {
        private const int ProgressInterval = 25;

        private readonly LeagueSourceResolver _sourceResolver;
        private readonly PlaywrightFactory _playwrightFactory;
        private readonly RawDataStore _store;
        private readonly IReadOnlyList<LeagueOptions> _leagues;
        private readonly JobOptions _jobOptions;
        private readonly PlaywrightOptions _playwrightOptions;

        public StatisticsDataProcessor(
            LeagueSourceResolver sourceResolver,
            PlaywrightFactory playwrightFactory,
            RawDataStore store,
            IReadOnlyList<LeagueOptions> leagues,
            IOptions<JobOptions> jobOptions,
            IOptions<PlaywrightOptions> playwrightOptions)
        {
            _sourceResolver = sourceResolver;
            _playwrightFactory = playwrightFactory;
            _store = store;
            _leagues = leagues;
            _jobOptions = jobOptions.Value;
            _playwrightOptions = playwrightOptions.Value;
        }

        public async Task AcquireAndSaveAsync(CancellationToken cancellationToken)
        {
            await using var session = await _playwrightFactory.SetupBrowserContextAsync();

            var leaguesToProcess = _leagues.Where(l => _jobOptions.IncludesLeague(l.Key)).ToList();
            foreach (var league in leaguesToProcess)
            {
                Console.WriteLine($"[{league.Key}] Loading seasons from {league.BaseUrl} (source: {league.Source})");
                var source = _sourceResolver.Resolve(league);
                var seasons = await source.GetMatchSeasonsAsync(league, session);
                if (seasons.Count == 0)
                {
                    Console.WriteLine($"[{league.Key}] No seasons found. Skipping league.");
                    continue;
                }

                var latestStartYear = seasons.Max(s => s.StartYear);
                var seasonsToProcess = _jobOptions.SelectSeasons(seasons);
                foreach (var season in seasonsToProcess)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await ProcessSeasonAsync(league, source, season, season.StartYear == latestStartYear, session, cancellationToken);
                }
            }
        }

        private async Task ProcessSeasonAsync(
            LeagueOptions league,
            ILeagueSource source,
            Season season,
            bool isLatestSeason,
            PlaywrightSession session,
            CancellationToken cancellationToken)
        {
            var reference = season.Reference;
            var label = $"[{league.Key} {reference.FolderName}]";

            if (!_jobOptions.Force && _store.TryReadManifest(league.Key, reference) is { IsComplete: true })
            {
                Console.WriteLine($"{label} Season is complete. Skipping.");
                return;
            }

            var schedule = await source.GetScheduleAsync(season, session);
            if (schedule.Count == 0)
            {
                Console.WriteLine($"{label} No matches found on the season page. Skipping.");
                return;
            }

            var pending = schedule
                .Where(entry => _jobOptions.Force
                    || _store.TryReadMatch(league.Key, reference, entry.MatchId) is not { Status: MatchStatus.Finished })
                .ToList();
            Console.WriteLine($"{label} {schedule.Count} matches listed, {pending.Count} to scrape.");

            var failedMatchIds = await ScrapeMatchesAsync(league, source, reference, pending, label, session, cancellationToken);

            var records = schedule
                .Select(entry => _store.TryReadMatch(league.Key, reference, entry.MatchId))
                .ToList();
            var finishedCount = records.Count(r => r?.Status == MatchStatus.Finished);
            var scheduledCount = records.Count(r => r?.Status == MatchStatus.Scheduled);

            var manifest = new SeasonManifest
            {
                ScrapedAtUtc = DateTime.UtcNow,
                IsComplete = !isLatestSeason && failedMatchIds.Count == 0 && finishedCount == schedule.Count,
                MatchCount = schedule.Count,
                FinishedCount = finishedCount,
                ScheduledCount = scheduledCount,
                FailedMatchIds = failedMatchIds,
            };
            await _store.WriteManifestAsync(league.Key, reference, manifest, cancellationToken);

            Console.WriteLine(
                $"{label} Done: {finishedCount} finished, {scheduledCount} scheduled, {failedMatchIds.Count} failed, complete: {manifest.IsComplete}.");
        }

        private async Task<IReadOnlyList<int>> ScrapeMatchesAsync(
            LeagueOptions league,
            ILeagueSource source,
            SeasonReference season,
            List<ScheduleEntry> entries,
            string label,
            PlaywrightSession session,
            CancellationToken cancellationToken)
        {
            var failedMatchIds = new ConcurrentBag<int>();
            var processed = 0;

            using var semaphore = new SemaphoreSlim(_playwrightOptions.MaxConcurrentTasks);
            await Task.WhenAll(entries.Select(async entry =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var record = await source.GetMatchAsync(league, season, entry, session);
                    await _store.WriteMatchAsync(record, cancellationToken);
                }
                catch (Exception ex) when (ex is PlaywrightException or TimeoutException or InvalidOperationException or FormatException)
                {
                    failedMatchIds.Add(entry.MatchId);
                    Console.WriteLine($"{label} Match {entry.MatchId} failed: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }

                var done = Interlocked.Increment(ref processed);
                if (done % ProgressInterval == 0)
                {
                    Console.WriteLine($"{label} {done}/{entries.Count} matches processed...");
                }
            }));

            return [.. failedMatchIds.Order()];
        }
    }
}
