using FantasyVolleyballLeague.Api.Domain;
using FantasyVolleyballLeague.Api.Infrastructure.Database;
using FantasyVolleyballLeague.Importer.Records;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.Importer.Import
{
    internal sealed record RosterMember(Guid PlayerId, int? ShirtNumber, string NameKey);

    /// <summary>
    /// Identifier cache with resolve-or-create logic shared by the import steps. It only keeps ids,
    /// so the change tracker can be cleared between batches. New entities are added to the context and
    /// persisted by the caller's <c>SaveChangesAsync</c>.
    /// </summary>
    internal sealed class ImportCatalog
    {
        private readonly FantasyVolleyballLeagueDbContext _dbContext;

        private Dictionary<(Guid LeagueId, int StartYear), Guid> _seasons = [];
        private Dictionary<(Guid SeasonId, string Name), Guid> _phases = [];
        private Dictionary<(Guid PhaseId, string Stage, string Name), Guid> _rounds = [];
        private Dictionary<(Guid SeasonId, int ExternalId), Guid> _rostersByExternalId = [];
        private Dictionary<(Guid SeasonId, string Name), Guid> _rostersByName = [];
        private Dictionary<int, Guid> _playersByExternalId = [];
        private Dictionary<string, Guid> _unresolvedPlayersByNameKey = [];
        private Dictionary<Guid, List<RosterMember>> _rosterMembers = [];
        private HashSet<(Guid RosterId, Guid PlayerId)> _rosterEntries = [];

        public ImportCatalog(FantasyVolleyballLeagueDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            _seasons = await _dbContext.Set<Season>().AsNoTracking()
                .ToDictionaryAsync(s => (s.LeagueId, s.StartYear), s => s.Id, cancellationToken);

            _phases = await _dbContext.Set<Phase>().AsNoTracking()
                .ToDictionaryAsync(p => (p.SeasonId, p.Name), p => p.Id, cancellationToken);

            _rounds = await _dbContext.Set<Round>().AsNoTracking()
                .ToDictionaryAsync(r => (r.PhaseId, r.Stage, r.Name), r => r.Id, cancellationToken);

            var rosters = await _dbContext.Set<Roster>().AsNoTracking()
                .Select(t => new { t.Id, t.SeasonId, t.Name, t.ExternalId })
                .ToListAsync(cancellationToken);
            _rostersByExternalId = rosters
                .Where(t => t.ExternalId is not null)
                .ToDictionary(t => (t.SeasonId, t.ExternalId!.Value), t => t.Id);
            _rostersByName = rosters.ToDictionary(t => (t.SeasonId, t.Name), t => t.Id);

            var players = await _dbContext.Set<Player>().AsNoTracking()
                .Select(p => new { p.Id, p.ExternalId, p.FullName })
                .ToListAsync(cancellationToken);
            _playersByExternalId = players
                .Where(p => p.ExternalId is not null)
                .ToDictionary(p => p.ExternalId!.Value, p => p.Id);
            _unresolvedPlayersByNameKey = players
                .Where(p => p.ExternalId is null)
                .GroupBy(p => PlayerNameKey.Create(p.FullName))
                .ToDictionary(g => g.Key, g => g.First().Id);
            var playerNames = players.ToDictionary(p => p.Id, p => p.FullName);

            var rosterEntries = await _dbContext.Set<RosterPlayer>().AsNoTracking()
                .Select(tp => new { tp.RosterId, tp.PlayerId, tp.ShirtNumber })
                .ToListAsync(cancellationToken);
            _rosterEntries = [.. rosterEntries.Select(tp => (tp.RosterId, tp.PlayerId))];
            _rosterMembers = rosterEntries
                .GroupBy(tp => tp.RosterId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(tp => new RosterMember(tp.PlayerId, tp.ShirtNumber, PlayerNameKey.Create(playerNames[tp.PlayerId]))).ToList());
        }

        /// <summary>
        /// Finds the league by country, gender and division (the name changes with sponsors) and keeps its name current.
        /// </summary>
        public async Task<Guid> EnsureLeagueAsync(LeagueImportOptions options, CancellationToken cancellationToken)
        {
            var countryId = options.ResolveCountry().Id;
            var genderId = options.ResolveGender().Id;
            var league = await _dbContext.Set<League>()
                .SingleOrDefaultAsync(
                    l => l.CountryId == countryId && l.GenderId == genderId && l.DivisionNumber == options.DivisionNumber,
                    cancellationToken);

            if (league is null)
            {
                league = new League(options.Name, options.DivisionNumber, countryId, genderId);
                _dbContext.Set<League>().Add(league);
            }
            else if (league.Name != options.Name)
            {
                league.Rename(options.Name);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return league.Id;
        }

        public Guid ResolveSeason(Guid leagueId, SeasonFile season)
        {
            var key = (leagueId, season.StartYear);
            if (_seasons.TryGetValue(key, out var id))
            {
                return id;
            }

            var entity = new Season(season.StartYear, season.EndYear, leagueId, season.ExternalId);
            _dbContext.Set<Season>().Add(entity);
            _seasons[key] = entity.Id;
            return entity.Id;
        }

        public Guid ResolvePhase(Guid seasonId, NamedOrderFile phase)
        {
            var key = (seasonId, phase.Name);
            if (_phases.TryGetValue(key, out var id))
            {
                return id;
            }

            var entity = new Phase(phase.Name, phase.Order, seasonId);
            _dbContext.Set<Phase>().Add(entity);
            _phases[key] = entity.Id;
            return entity.Id;
        }

        public Guid ResolveRound(Guid phaseId, NamedOrderFile stage, NamedOrderFile round)
        {
            var key = (phaseId, stage.Name, round.Name);
            if (_rounds.TryGetValue(key, out var id))
            {
                return id;
            }

            var entity = new Round(phaseId, stage.Name, stage.Order, round.Name, round.Order);
            _dbContext.Set<Round>().Add(entity);
            _rounds[key] = entity.Id;
            return entity.Id;
        }

        /// <summary>
        /// Finds the team by club id, then by name within the season; creates it when missing.
        /// </summary>
        public Guid ResolveRoster(Guid leagueId, Guid seasonId, string name, int? externalId)
        {
            if (externalId is not null && _rostersByExternalId.TryGetValue((seasonId, externalId.Value), out var id))
            {
                return id;
            }

            if (_rostersByName.TryGetValue((seasonId, name), out id))
            {
                return id;
            }

            var entity = new Roster(name, leagueId, seasonId, externalId);
            _dbContext.Set<Roster>().Add(entity);
            _rostersByName[(seasonId, name)] = entity.Id;
            if (externalId is not null)
            {
                _rostersByExternalId[(seasonId, externalId.Value)] = entity.Id;
            }

            return entity.Id;
        }

        public Guid? FindPlayer(int externalId)
        {
            return _playersByExternalId.TryGetValue(externalId, out var id) ? id : null;
        }

        public Guid ResolvePlayer(int externalId, string fullName)
        {
            if (_playersByExternalId.TryGetValue(externalId, out var id))
            {
                return id;
            }

            var entity = new Player(fullName, externalId);
            _dbContext.Set<Player>().Add(entity);
            _playersByExternalId[externalId] = entity.Id;
            return entity.Id;
        }

        public void RegisterPlayer(int externalId, Guid playerId)
        {
            _playersByExternalId[externalId] = playerId;
        }

        /// <summary>
        /// A player without a website id (could not be matched to a roster). Reuses an earlier one with the same name.
        /// </summary>
        public Guid ResolveUnresolvedPlayer(string fullName)
        {
            var nameKey = PlayerNameKey.Create(fullName);
            if (_unresolvedPlayersByNameKey.TryGetValue(nameKey, out var id))
            {
                return id;
            }

            var entity = new Player(fullName, null);
            _dbContext.Set<Player>().Add(entity);
            _unresolvedPlayersByNameKey[nameKey] = entity.Id;
            return entity.Id;
        }

        public IReadOnlyList<RosterMember> GetRosterMembers(Guid rosterId)
        {
            return _rosterMembers.TryGetValue(rosterId, out var members) ? members : [];
        }

        /// <summary>
        /// Adds the player to the team's roster when not listed yet (e.g. a player missing from the scraped roster).
        /// </summary>
        public void EnsureRosterEntry(Guid rosterId, Guid playerId, int? shirtNumber, string fullName)
        {
            if (!_rosterEntries.Add((rosterId, playerId)))
            {
                return;
            }

            _dbContext.Set<RosterPlayer>().Add(new RosterPlayer(rosterId, playerId, shirtNumber, null));
            AddRosterMember(rosterId, new RosterMember(playerId, shirtNumber, PlayerNameKey.Create(fullName)));
        }

        public bool HasRosterEntry(Guid rosterId, Guid playerId)
        {
            return _rosterEntries.Contains((rosterId, playerId));
        }

        public void RegisterRosterEntry(Guid rosterId, Guid playerId, int? shirtNumber, string fullName)
        {
            if (_rosterEntries.Add((rosterId, playerId)))
            {
                AddRosterMember(rosterId, new RosterMember(playerId, shirtNumber, PlayerNameKey.Create(fullName)));
            }
        }

        private void AddRosterMember(Guid rosterId, RosterMember member)
        {
            if (!_rosterMembers.TryGetValue(rosterId, out var members))
            {
                members = [];
                _rosterMembers[rosterId] = members;
            }

            members.Add(member);
        }
    }
}
