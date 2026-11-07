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

            var previousManifest = _store.TryReadManifest(league.Key, reference);
            if (!_jobOptions.Force && previousManifest is { IsComplete: true })
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

            // --force starts the attempt count from zero, so matches given up on earlier are tried again.
            var previousFailures = _jobOptions.Force || previousManifest is null
                ? new Dictionary<int, MatchFailure>()
                : previousManifest.FailedMatches.ToDictionary(failure => failure.MatchId);

            var issues = await ScrapePendingMatchesAsync(league, source, reference, pending, previousFailures, label, session, cancellationToken);

            var pendingIds = pending.Select(entry => entry.MatchId).ToHashSet();
            var withoutStatistics = (previousManifest?.MatchesWithoutStatistics ?? [])
                .Where(failure => !pendingIds.Contains(failure.MatchId))
                .Concat(issues.WithoutStatistics)
                .OrderBy(failure => failure.MatchId)
                .ToList();

            var records = schedule
                .Select(entry => _store.TryReadMatch(league.Key, reference, entry.MatchId))
                .ToList();
            var finishedCount = records.Count(r => r?.Status == MatchStatus.Finished);
            var scheduledCount = records.Count(r => r?.Status == MatchStatus.Scheduled);

            var manifest = new SeasonManifest
            {
                ScrapedAtUtc = DateTime.UtcNow,
                IsComplete = !isLatestSeason && issues.Failed.Count == 0 && finishedCount == schedule.Count,
                MatchCount = schedule.Count,
                FinishedCount = finishedCount,
                ScheduledCount = scheduledCount,
                FailedMatches = issues.Failed,
                MatchesWithoutStatistics = withoutStatistics,
            };
            await _store.WriteManifestAsync(league.Key, reference, manifest, cancellationToken);

            Console.WriteLine(
                $"{label} Done: {finishedCount} finished, {scheduledCount} scheduled, {issues.Failed.Count} failed, "
                + $"{withoutStatistics.Count} without statistics, complete: {manifest.IsComplete}.");
        }

        private async Task<SeasonScrapeIssues> ScrapePendingMatchesAsync(
            LeagueOptions league,
            ILeagueSource source,
            SeasonReference season,
            List<ScheduleEntry> entries,
            Dictionary<int, MatchFailure> previousFailures,
            string label,
            PlaywrightSession session,
            CancellationToken cancellationToken)
        {
            var failed = new ConcurrentBag<MatchFailure>();
            var withoutStatistics = new ConcurrentBag<MatchFailure>();
            var processed = 0;

            using var semaphore = new SemaphoreSlim(_playwrightOptions.MaxConcurrentTasks);
            await Task.WhenAll(entries.Select(async entry =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    previousFailures.TryGetValue(entry.MatchId, out var previous);
                    var result = await ScrapeMatchAsync(league, source, season, entry, previous, label, session, cancellationToken);
                    if (result.Failed is not null)
                    {
                        failed.Add(result.Failed);
                    }

                    if (result.WithoutStatistics is not null)
                    {
                        withoutStatistics.Add(result.WithoutStatistics);
                    }
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

            return new SeasonScrapeIssues(
                [.. failed.OrderBy(failure => failure.MatchId)],
                [.. withoutStatistics.OrderBy(failure => failure.MatchId)]);
        }

        /// <summary>
        /// Scrapes one match. A failed attempt is counted; after <see cref="JobOptions.MaxMatchAttempts"/> of them
        /// the match is saved without player statistics. A website block is not the match's fault, so it is not counted.
        /// </summary>
        private async Task<MatchScrapeResult> ScrapeMatchAsync(
            LeagueOptions league,
            ILeagueSource source,
            SeasonReference season,
            ScheduleEntry entry,
            MatchFailure? previous,
            string label,
            PlaywrightSession session,
            CancellationToken cancellationToken)
        {
            var maxAttempts = _jobOptions.MaxMatchAttempts;
            var attempts = previous?.Attempts ?? 0;
            var lastError = previous?.LastError;

            if (attempts < maxAttempts)
            {
                try
                {
                    var record = await source.GetMatchAsync(league, season, entry, session);
                    await _store.WriteMatchAsync(record, cancellationToken);
                    return MatchScrapeResult.Succeeded;
                }
                catch (SourceBlockedException ex)
                {
                    Console.WriteLine($"{label} Match {entry.MatchId} blocked, not counted as an attempt: {FirstLine(ex.Message)}");
                    return MatchScrapeResult.NotSaved(entry.MatchId, attempts, FirstLine(ex.Message));
                }
                catch (Exception ex) when (IsMatchFailure(ex))
                {
                    attempts++;
                    lastError = FirstLine(ex.Message);
                    Console.WriteLine($"{label} Match {entry.MatchId} failed (attempt {attempts}/{maxAttempts}): {lastError}");
                    if (attempts < maxAttempts)
                    {
                        return MatchScrapeResult.NotSaved(entry.MatchId, attempts, lastError);
                    }
                }
            }

            try
            {
                var record = await source.GetMatchWithoutStatisticsAsync(league, season, entry, session);
                await _store.WriteMatchAsync(record, cancellationToken);
                Console.WriteLine($"{label} Match {entry.MatchId} saved without statistics after {attempts} failed attempts.");
                return MatchScrapeResult.SavedWithoutStatistics(entry.MatchId, attempts, lastError);
            }
            catch (SourceBlockedException ex)
            {
                Console.WriteLine($"{label} Match {entry.MatchId} blocked while saving without statistics: {FirstLine(ex.Message)}");
                return MatchScrapeResult.NotSaved(entry.MatchId, attempts, lastError);
            }
            catch (Exception ex) when (IsMatchFailure(ex))
            {
                Console.WriteLine($"{label} Match {entry.MatchId} failed even without statistics: {FirstLine(ex.Message)}");
                return MatchScrapeResult.NotSaved(entry.MatchId, attempts, FirstLine(ex.Message));
            }
        }

        private static bool IsMatchFailure(Exception ex)
        {
            return ex is PlaywrightException or TimeoutException or InvalidOperationException or FormatException;
        }

        private static string FirstLine(string message)
        {
            return message.Split('\n')[0].Trim();
        }
    }
}
