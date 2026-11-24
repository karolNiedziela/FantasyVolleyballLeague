using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FantasyVolleyballLeague.Importer.Steps
{
    /// <summary>
    /// Sets <see cref="Season.LeagueName"/>, the name the league had in that season, from
    /// <c>ManualData/league-names.json</c> (<see cref="LeagueNaming"/>); a season before the first entry of its league
    /// gets the current league name (<c>Importer:Leagues:Name</c>). Runs after the rosters and the matches, because both
    /// create seasons. A file with errors is reported and the names stay as they were.
    /// </summary>
    internal sealed class SeasonLeagueNamesImportStep : IImportStep
    {
        private readonly FantasyVolleyballLeagueDbContext _dbContext;
        private readonly ImportCatalog _catalog;
        private readonly ManualDataReader _manualDataReader;
        private readonly ImportReport _report;
        private readonly ImporterOptions _options;

        public SeasonLeagueNamesImportStep(
            FantasyVolleyballLeagueDbContext dbContext,
            ImportCatalog catalog,
            ManualDataReader manualDataReader,
            ImportReport report,
            IOptions<ImporterOptions> options)
        {
            _dbContext = dbContext;
            _catalog = catalog;
            _manualDataReader = manualDataReader;
            _report = report;
            _options = options.Value;
        }

        public int Order => 3;

        public string Name => "Set the league name of each season (historical names from league-names.json)";

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var leagues = await _catalog.EnsureLeaguesAsync(_options.Leagues, cancellationToken);
            var leagueKeys = leagues.Values.Select(league => league.Key).ToList();

            var leagueNames = await _manualDataReader.ReadValidatedAsync<LeagueNameFile>(
                ManualDataReader.LeagueNamesFileName,
                entries => LeagueNaming.Validate(entries, leagueKeys),
                cancellationToken);
            var path = _manualDataReader.GetPath(ManualDataReader.LeagueNamesFileName);
            foreach (var error in leagueNames.Errors)
            {
                _report.FileErrors.Add(new FileError(path, error));
            }

            if (leagueNames.Entries is null)
            {
                Console.WriteLine($"  {ManualDataReader.LeagueNamesFileName} has errors; the league names of the seasons were not changed");
                return;
            }

            var leagueIds = leagues.Keys.ToList();
            var seasons = await _dbContext.Set<Season>()
                .Where(season => leagueIds.Contains(season.LeagueId))
                .ToListAsync(cancellationToken);
            foreach (var season in seasons)
            {
                var league = leagues[season.LeagueId];
                var leagueName = LeagueNaming.Find(leagueNames.Entries, league.Key, season.StartYear) ?? league.Name;
                if (season.LeagueName != leagueName)
                {
                    season.SetLeagueName(leagueName);
                    _report.SeasonsRenamed++;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
        }
    }
}
