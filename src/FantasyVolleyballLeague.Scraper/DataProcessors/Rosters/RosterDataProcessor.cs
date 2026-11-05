using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources;
using FantasyVolleyballLeague.Scraper.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Scraper.DataProcessors.Rosters
{
    /// <summary>
    /// <c>--job Rosters</c>: scrapes season rosters into the raw data store. Rosters of past seasons are
    /// skipped when they already exist (unless <c>--force</c>); the latest season is always refreshed,
    /// because squads change during the season.
    /// </summary>
    internal sealed class RosterDataProcessor : IRosterDataProcessor
    {
        private readonly LeagueSourceResolver _sourceResolver;
        private readonly PlaywrightFactory _playwrightFactory;
        private readonly RawDataStore _store;
        private readonly IReadOnlyList<LeagueOptions> _leagues;
        private readonly JobOptions _jobOptions;

        public RosterDataProcessor(
            LeagueSourceResolver sourceResolver,
            PlaywrightFactory playwrightFactory,
            RawDataStore store,
            IReadOnlyList<LeagueOptions> leagues,
            IOptions<JobOptions> jobOptions)
        {
            _sourceResolver = sourceResolver;
            _playwrightFactory = playwrightFactory;
            _store = store;
            _leagues = leagues;
            _jobOptions = jobOptions.Value;
        }

        public async Task AcquireAndSaveAsync(CancellationToken cancellationToken)
        {
            await using var session = await _playwrightFactory.SetupBrowserContextAsync();

            var leaguesToProcess = _leagues.Where(l => _jobOptions.IncludesLeague(l.Key)).ToList();
            foreach (var league in leaguesToProcess)
            {
                Console.WriteLine($"[{league.Key}] Loading seasons from {league.BaseUrl} (source: {league.Source})");
                var source = _sourceResolver.Resolve(league);
                var seasons = await source.GetRosterSeasonsAsync(league, session);
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
            var label = $"[{league.Key} {season.Reference.FolderName}]";
            var clubs = await source.GetClubsAsync(season, session);
            Console.WriteLine($"{label} {clubs.Count} clubs.");

            foreach (var club in clubs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_jobOptions.Force && !isLatestSeason && _store.RosterExists(league.Key, season.Reference, club.ExternalId))
                {
                    continue;
                }

                try
                {
                    var roster = await source.GetRosterAsync(league, season, club, session);
                    await _store.WriteRosterAsync(roster, cancellationToken);
                    Console.WriteLine($"{label} {roster.Club.Name}: {roster.Players.Count} players.");
                }
                catch (Exception ex) when (ex is PlaywrightException or TimeoutException or InvalidOperationException)
                {
                    Console.WriteLine($"{label} {club.Name} ({club.ExternalId}) failed: {ex.Message}");
                }
            }
        }
    }
}
