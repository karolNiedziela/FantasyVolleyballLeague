using System.Text.Json;
using FantasyVolleyballLeague.Api.Domain;
using FantasyVolleyballLeague.Api.Infrastructure.Database;
using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FantasyVolleyballLeague.Importer.Steps
{
    /// <summary>
    /// Imports season rosters: team (by club id), player (by player id, profile refreshed) and roster entry
    /// (shirt number, position). Runs before the matches, so widget-era statistics can be matched to rosters.
    /// </summary>
    internal sealed class RostersImportStep : IImportStep
    {
        private readonly FantasyVolleyballLeagueDbContext _dbContext;
        private readonly ImportCatalog _catalog;
        private readonly RawDataReader _reader;
        private readonly ImportReport _report;
        private readonly ImporterOptions _options;

        public RostersImportStep(
            FantasyVolleyballLeagueDbContext dbContext,
            ImportCatalog catalog,
            RawDataReader reader,
            ImportReport report,
            IOptions<ImporterOptions> options)
        {
            _dbContext = dbContext;
            _catalog = catalog;
            _reader = reader;
            _report = report;
            _options = options.Value;
        }

        public int Order => 1;

        public string Name => "Import rosters";

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _catalog.LoadAsync(cancellationToken);

            foreach (var league in _options.Leagues)
            {
                var leagueId = await _catalog.EnsureLeagueAsync(league, cancellationToken);
                var files = _reader.GetFiles("rosters", league.Key);
                Console.WriteLine($"  [{league.Key}] {files.Count} roster files");

                foreach (var path in files)
                {
                    RosterFile roster;
                    try
                    {
                        roster = await RawDataReader.ReadAsync<RosterFile>(path, r => r.SchemaVersion, cancellationToken);
                    }
                    catch (Exception ex) when (ex is JsonException or InvalidDataException)
                    {
                        _report.FileErrors.Add(new FileError(path, ex.Message));
                        continue;
                    }

                    await ImportRosterAsync(leagueId, roster, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _dbContext.ChangeTracker.Clear();
                    _report.RostersImported++;
                }
            }
        }

        private async Task ImportRosterAsync(Guid leagueId, RosterFile roster, CancellationToken cancellationToken)
        {
            var seasonId = _catalog.ResolveSeason(leagueId, roster.Season);
            var rosterId = _catalog.ResolveRoster(leagueId, seasonId, roster.Club.Name, roster.Club.ExternalId);

            var externalIds = roster.Players.Select(p => (int?)p.ExternalId).ToList();
            var players = await _dbContext.Set<Player>()
                .Where(p => externalIds.Contains(p.ExternalId))
                .ToDictionaryAsync(p => p.ExternalId!.Value, cancellationToken);
            var rosterEntries = await _dbContext.Set<RosterPlayer>()
                .Where(tp => tp.RosterId == rosterId)
                .ToDictionaryAsync(tp => tp.PlayerId, cancellationToken);

            foreach (var rosterPlayer in roster.Players)
            {
                if (!players.TryGetValue(rosterPlayer.ExternalId, out var player))
                {
                    player = new Player(rosterPlayer.FullName, rosterPlayer.ExternalId);
                    _dbContext.Set<Player>().Add(player);
                    _catalog.RegisterPlayer(rosterPlayer.ExternalId, player.Id);
                }

                player.UpdateProfile(
                    rosterPlayer.FullName,
                    rosterPlayer.DateOfBirth,
                    rosterPlayer.Height,
                    rosterPlayer.Weight,
                    rosterPlayer.AttackReach);

                var positionId = PlayerPosition.All
                    .FirstOrDefault(p => string.Equals(p.Name, rosterPlayer.Position, StringComparison.OrdinalIgnoreCase))
                    ?.Id;

                if (rosterEntries.TryGetValue(player.Id, out var entry))
                {
                    entry.Update(rosterPlayer.ShirtNumber, positionId);
                }
                else if (!_catalog.HasRosterEntry(rosterId, player.Id))
                {
                    _dbContext.Set<RosterPlayer>().Add(new RosterPlayer(rosterId, player.Id, rosterPlayer.ShirtNumber, positionId));
                }

                _catalog.RegisterRosterEntry(rosterId, player.Id, rosterPlayer.ShirtNumber, rosterPlayer.FullName);
            }
        }
    }
}
