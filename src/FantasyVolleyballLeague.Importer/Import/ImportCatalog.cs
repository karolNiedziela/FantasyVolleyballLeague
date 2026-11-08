using FantasyVolleyballLeague.VolleyballData.Domain;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.Importer.Records;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.Importer.Import
{
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
        private Dictionary<Guid, int> _leagueGenders = [];
        private Dictionary<(int GenderId, int ExternalId), Guid> _playersByExternalId = [];
        private Dictionary<(int GenderId, string NameKey), Guid> _unresolvedPlayersByNameKey = [];
        private Dictionary<Guid, List<RosterMember>> _rosterMembers = [];
        private HashSet<(Guid RosterId, Guid PlayerId)> _rosterEntries = [];
        private Dictionary<Guid, (Guid LeagueId, int StartYear)> _seasonInfo = [];
        private Dictionary<Guid, Guid> _rosterSeasons = [];
        private SeasonNameIndex _seasonNames = new();

        public ImportCatalog(FantasyVolleyballLeagueDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            _seasons = await _dbContext.Set<Season>().AsNoTracking()
                .ToDictionaryAsync(s => (s.LeagueId, s.StartYear), s => s.Id, cancellationToken);
            _seasonInfo = _seasons.ToDictionary(s => s.Value, s => (s.Key.LeagueId, s.Key.StartYear));

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
            _rosterSeasons = rosters.ToDictionary(t => t.Id, t => t.SeasonId);

            _leagueGenders = await _dbContext.Set<League>().AsNoTracking()
                .ToDictionaryAsync(l => l.Id, l => l.GenderId, cancellationToken);

            var players = await _dbContext.Set<Player>().AsNoTracking()
                .Select(p => new { p.Id, p.GenderId, p.ExternalId, p.FullName })
                .ToListAsync(cancellationToken);
            _playersByExternalId = players
                .Where(p => p.ExternalId is not null)
                .ToDictionary(p => (p.GenderId, p.ExternalId!.Value), p => p.Id);
            _unresolvedPlayersByNameKey = players
                .Where(p => p.ExternalId is null)
                .GroupBy(p => (p.GenderId, NameKey: PlayerNameKey.Create(p.FullName)))
                .ToDictionary(g => g.Key, g => g.First().Id);
            var playerNames = players.ToDictionary(p => p.Id, p => p.FullName);

            var rosterEntries = await _dbContext.Set<RosterPlayer>().AsNoTracking()
                .Select(rosterEntry => new { rosterEntry.RosterId, rosterEntry.PlayerId, rosterEntry.ShirtNumber, rosterEntry.MemberFrom, rosterEntry.MemberTo })
                .ToListAsync(cancellationToken);
            _rosterEntries = [.. rosterEntries.Select(rosterEntry => (rosterEntry.RosterId, rosterEntry.PlayerId))];
            _seasonNames = new SeasonNameIndex();
            _rosterMembers = [];
            foreach (var rosterEntry in rosterEntries)
            {
                AddRosterMember(
                    rosterEntry.RosterId,
                    new RosterMember(
                        rosterEntry.PlayerId,
                        rosterEntry.ShirtNumber,
                        PlayerNameKey.Create(playerNames[rosterEntry.PlayerId]),
                        rosterEntry.MemberFrom,
                        rosterEntry.MemberTo));
            }
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
            _leagueGenders[league.Id] = genderId;
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
            _seasonInfo[entity.Id] = (leagueId, season.StartYear);
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
            _rosterSeasons[entity.Id] = seasonId;
            if (externalId is not null)
            {
                _rostersByExternalId[(seasonId, externalId.Value)] = entity.Id;
            }

            return entity.Id;
        }

        /// <summary>
        /// The roster of the club (by club id) in a season, or <c>null</c> when the club has none there.
        /// </summary>
        public Guid? FindRoster(Guid seasonId, int clubExternalId)
        {
            return _rostersByExternalId.TryGetValue((seasonId, clubExternalId), out var id) ? id : null;
        }

        /// <summary>
        /// The only player of that name on any roster of the same gender and season, in any division.
        /// </summary>
        public Guid? FindPlayerInSeason(Guid leagueId, int startYear, string fullName)
        {
            return _seasonNames.Find(GetGenderId(leagueId), startYear, PlayerNameKey.Create(fullName));
        }

        public Guid? FindPlayer(Guid leagueId, int externalId)
        {
            return _playersByExternalId.TryGetValue((GetGenderId(leagueId), externalId), out var id) ? id : null;
        }

        public Guid ResolvePlayer(Guid leagueId, int externalId, string fullName)
        {
            var genderId = GetGenderId(leagueId);
            if (_playersByExternalId.TryGetValue((genderId, externalId), out var id))
            {
                return id;
            }

            var entity = new Player(genderId, fullName, externalId);
            _dbContext.Set<Player>().Add(entity);
            _playersByExternalId[(genderId, externalId)] = entity.Id;
            return entity.Id;
        }

        public void RegisterPlayer(Guid leagueId, int externalId, Guid playerId)
        {
            _playersByExternalId[(GetGenderId(leagueId), externalId)] = playerId;
        }

        /// <summary>
        /// A player without a website id (could not be matched to a roster). Reuses an earlier one of the same gender with the same name.
        /// </summary>
        public Guid ResolveUnresolvedPlayer(Guid leagueId, string fullName)
        {
            var genderId = GetGenderId(leagueId);
            var key = (genderId, PlayerNameKey.Create(fullName));
            if (_unresolvedPlayersByNameKey.TryGetValue(key, out var id))
            {
                return id;
            }

            var entity = new Player(genderId, fullName, null);
            _dbContext.Set<Player>().Add(entity);
            _unresolvedPlayersByNameKey[key] = entity.Id;
            return entity.Id;
        }

        /// <summary>
        /// Gender of a league that went through <see cref="EnsureLeagueAsync"/> (player ids are scoped by it).
        /// </summary>
        public int GetGenderId(Guid leagueId)
        {
            return _leagueGenders.TryGetValue(leagueId, out var genderId)
                ? genderId
                : throw new InvalidOperationException($"League {leagueId} was not ensured before resolving players.");
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

            _dbContext.Set<RosterPlayer>().Add(new RosterPlayer(rosterId, playerId, shirtNumber, null, null, null));
            AddRosterMember(rosterId, new RosterMember(playerId, shirtNumber, PlayerNameKey.Create(fullName), null, null));
        }

        public bool HasRosterEntry(Guid rosterId, Guid playerId)
        {
            return _rosterEntries.Contains((rosterId, playerId));
        }

        public void RegisterRosterEntry(Guid rosterId, Guid playerId, int? shirtNumber, string fullName, DateOnly? from, DateOnly? to)
        {
            if (_rosterEntries.Add((rosterId, playerId)))
            {
                AddRosterMember(rosterId, new RosterMember(playerId, shirtNumber, PlayerNameKey.Create(fullName), from, to));
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

            if (_rosterSeasons.TryGetValue(rosterId, out var seasonId) && _seasonInfo.TryGetValue(seasonId, out var season))
            {
                _seasonNames.Add(GetGenderId(season.LeagueId), season.StartYear, member.NameKey, member.PlayerId);
            }
        }
    }
}
