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

                var stints = new List<ClubStint>();
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

                    await ImportRosterAsync(leagueId, roster, stints, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _dbContext.ChangeTracker.Clear();
                    _report.RostersImported++;
                }

                await ApplyStintsAsync(stints, cancellationToken);
            }
        }

        /// <summary>
        /// Gives a player who moved during the season a roster entry (with the dates) in each club he played for.
        /// A roster lists the players of the end of the season, so the former club does not list him. It runs after
        /// all rosters of the league, because the former club's roster may be imported later than the new club's.
        /// </summary>
        private async Task ApplyStintsAsync(List<ClubStint> stints, CancellationToken cancellationToken)
        {
            var stintsByRoster = stints
                .Select(stint => (RosterId: _catalog.FindRoster(stint.SeasonId, stint.ClubExternalId), Stint: stint))
                .Where(item => item.RosterId is not null)
                .GroupBy(item => item.RosterId!.Value);

            foreach (var group in stintsByRoster)
            {
                var entries = await _dbContext.Set<RosterPlayer>()
                    .Where(rosterEntry => rosterEntry.RosterId == group.Key)
                    .ToDictionaryAsync(rosterEntry => rosterEntry.PlayerId, cancellationToken);

                foreach (var (_, stint) in group)
                {
                    if (entries.TryGetValue(stint.PlayerId, out var entry))
                    {
                        entry.Update(stint.ShirtNumber, stint.PositionId, stint.From, stint.To);
                    }
                    else if (!_catalog.HasRosterEntry(group.Key, stint.PlayerId))
                    {
                        _dbContext.Set<RosterPlayer>().Add(
                            new RosterPlayer(group.Key, stint.PlayerId, stint.ShirtNumber, stint.PositionId, stint.From, stint.To));
                        _catalog.RegisterRosterEntry(group.Key, stint.PlayerId, stint.ShirtNumber, stint.FullName, stint.From, stint.To);
                        _report.RosterEntriesFromCareer++;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
            }
        }

        private async Task ImportRosterAsync(Guid leagueId, RosterFile roster, List<ClubStint> stints, CancellationToken cancellationToken)
        {
            var seasonId = _catalog.ResolveSeason(leagueId, roster.Season);
            var rosterId = _catalog.ResolveRoster(leagueId, seasonId, roster.Club.Name, roster.Club.ExternalId);

            var genderId = _catalog.GetGenderId(leagueId);
            var externalIds = roster.Players.Select(p => (int?)p.ExternalId).ToList();
            var players = await _dbContext.Set<Player>()
                .Where(p => p.GenderId == genderId && externalIds.Contains(p.ExternalId))
                .ToDictionaryAsync(p => p.ExternalId!.Value, cancellationToken);
            var rosterEntries = await _dbContext.Set<RosterPlayer>()
                .Where(rosterEntry => rosterEntry.RosterId == rosterId)
                .ToDictionaryAsync(rosterEntry => rosterEntry.PlayerId, cancellationToken);

            foreach (var rosterPlayer in roster.Players)
            {
                if (!players.TryGetValue(rosterPlayer.ExternalId, out var player))
                {
                    player = new Player(genderId, rosterPlayer.FullName, rosterPlayer.ExternalId);
                    _dbContext.Set<Player>().Add(player);
                    _catalog.RegisterPlayer(leagueId, rosterPlayer.ExternalId, player.Id);
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

                var (memberFrom, memberTo) = CareerMembership.ForClub(rosterPlayer.Career, roster.Club.ExternalId);
                if (rosterEntries.TryGetValue(player.Id, out var entry))
                {
                    entry.Update(rosterPlayer.ShirtNumber, positionId, memberFrom, memberTo);
                }
                else if (!_catalog.HasRosterEntry(rosterId, player.Id))
                {
                    _dbContext.Set<RosterPlayer>().Add(
                        new RosterPlayer(rosterId, player.Id, rosterPlayer.ShirtNumber, positionId, memberFrom, memberTo));
                }

                _catalog.RegisterRosterEntry(rosterId, player.Id, rosterPlayer.ShirtNumber, rosterPlayer.FullName, memberFrom, memberTo);

                AddStintsInOtherClubs(stints, seasonId, roster.Club.ExternalId, player.Id, rosterPlayer);
            }
        }

        private static void AddStintsInOtherClubs(List<ClubStint> stints, Guid seasonId, int ownClubExternalId, Guid playerId, RosterPlayerFile rosterPlayer)
        {
            var otherClubs = rosterPlayer.Career
                .Where(entry => entry.ClubExternalId != ownClubExternalId)
                .GroupBy(entry => entry.ClubExternalId);

            foreach (var club in otherClubs)
            {
                var (from, to) = CareerMembership.ForClub(rosterPlayer.Career, club.Key);
                var latest = club.OrderBy(entry => entry.From).Last();
                var positionId = PlayerPosition.All
                    .FirstOrDefault(p => string.Equals(p.Name, latest.Position, StringComparison.OrdinalIgnoreCase))
                    ?.Id;

                stints.Add(new ClubStint(seasonId, club.Key, playerId, rosterPlayer.FullName, latest.ShirtNumber, positionId, from, to));
            }
        }
    }
}
