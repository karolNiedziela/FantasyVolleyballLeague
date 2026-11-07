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
    /// Upserts matches by (league, website match id). A match is skipped when its file is not newer than
    /// <see cref="Match.SourceScrapedAt"/>; otherwise schedule, details, sets, player statistics and MVP are
    /// replaced with the file content (e.g. a scheduled match that has been played).
    /// </summary>
    internal sealed class MatchesImportStep : IImportStep
    {
        private const int BatchSize = 100;

        private readonly FantasyVolleyballLeagueDbContext _dbContext;
        private readonly ImportCatalog _catalog;
        private readonly RawDataReader _reader;
        private readonly ImportReport _report;
        private readonly ImporterOptions _options;

        private Dictionary<(Guid LeagueId, int ExternalId), ImportedMatch> _importedMatches = [];

        public MatchesImportStep(
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

        public int Order => 2;

        public string Name => "Import matches";

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _catalog.LoadAsync(cancellationToken);

            _importedMatches = await _dbContext.Set<Match>().AsNoTracking()
                .Where(m => m.ExternalId != null)
                .Select(m => new { m.LeagueId, ExternalId = m.ExternalId!.Value, m.Id, m.SourceScrapedAt })
                .ToDictionaryAsync(m => (m.LeagueId, m.ExternalId), m => new ImportedMatch(m.Id, m.SourceScrapedAt), cancellationToken);

            foreach (var league in _options.Leagues)
            {
                var leagueId = await _catalog.EnsureLeagueAsync(league, cancellationToken);
                var files = _reader.GetFiles("matches", league.Key);
                Console.WriteLine($"  [{league.Key}] {files.Count} match files");

                var processed = 0;
                foreach (var path in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    MatchFile match;
                    try
                    {
                        match = await RawDataReader.ReadAsync<MatchFile>(path, m => m.SchemaVersion, cancellationToken);
                    }
                    catch (Exception ex) when (ex is JsonException or InvalidDataException)
                    {
                        _report.FileErrors.Add(new FileError(path, ex.Message));
                        continue;
                    }

                    if (await ImportMatchAsync(leagueId, match, path, cancellationToken) && ++processed % BatchSize == 0)
                    {
                        await SaveBatchAsync(cancellationToken);
                        Console.WriteLine($"  [{league.Key}] {processed} matches imported...");
                    }
                }

                await SaveBatchAsync(cancellationToken);
                Console.WriteLine($"  [{league.Key}] {processed} matches imported or updated");
            }
        }

        private async Task SaveBatchAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
        }

        /// <returns><c>false</c> when the match was skipped.</returns>
        private async Task<bool> ImportMatchAsync(Guid leagueId, MatchFile file, string path, CancellationToken cancellationToken)
        {
            var scrapedAt = StatisticsParsing.ToUtcOffset(file.ScrapedAtUtc);
            var key = (leagueId, file.MatchId);
            if (_importedMatches.TryGetValue(key, out var imported) && scrapedAt <= imported.SourceScrapedAt)
            {
                _report.MatchesSkipped++;
                return false;
            }

            var status = MatchStatus.FromName(file.Status);
            var statisticsLevel = StatisticsLevel.FromName(file.StatisticsLevel);
            if (status is null || statisticsLevel is null)
            {
                _report.FileErrors.Add(new FileError(path, $"Unknown status '{file.Status}' or statistics level '{file.StatisticsLevel}'."));
                return false;
            }

            var seasonId = _catalog.ResolveSeason(leagueId, file.Season);
            var phaseId = _catalog.ResolvePhase(seasonId, file.Phase);
            var roundId = _catalog.ResolveRound(phaseId, file.Stage, file.Round);
            var homeRosterId = _catalog.ResolveRoster(leagueId, seasonId, file.HomeRoster.Name, file.HomeRoster.ExternalId);
            var awayRosterId = _catalog.ResolveRoster(leagueId, seasonId, file.AwayRoster.Name, file.AwayRoster.ExternalId);
            DateTimeOffset? date = file.Date is null ? null : StatisticsParsing.ToUtcOffset(file.Date.Value);

            Match match;
            if (imported is not null)
            {
                match = await _dbContext.Set<Match>()
                    .Include(m => m.Sets)
                    .Include(m => m.PlayerStatistics)
                    .AsSplitQuery()
                    .SingleAsync(m => m.Id == imported.Id, cancellationToken);
                match.Reschedule(roundId, homeRosterId, awayRosterId, date, file.IsTimeConfirmed);
                _report.MatchesUpdated++;
            }
            else
            {
                match = new Match(file.MatchId, leagueId, roundId, homeRosterId, awayRosterId, date, file.IsTimeConfirmed);
                _dbContext.Set<Match>().Add(match);
                _report.MatchesInserted++;
            }

            match.UpdateDetails(
                status.Id,
                file.MatchNumber,
                file.Attendance,
                new Venue(file.VenueName, file.VenueCity),
                new MatchOfficials(file.FirstReferee, file.SecondReferee, file.Commissioner));
            match.ReplaceSets(file.Sets.Select(s => (s.HomePoints, s.AwayPoints)));

            var context = new MatchImportContext(file, path, leagueId, match.Id);
            var statistics = BuildStatistics(context, homeRosterId, file.HomeRoster)
                .Concat(BuildStatistics(context, awayRosterId, file.AwayRoster))
                .ToList();
            match.ReplacePlayerStatistics(statisticsLevel.Id, statistics);
            match.AssignMvp(ResolveMvp(context));
            match.MarkSynced(scrapedAt);

            _importedMatches[key] = new ImportedMatch(match.Id, scrapedAt);
            return true;
        }

        private List<PlayerMatchStatistics> BuildStatistics(MatchImportContext context, Guid rosterId, MatchRosterFile roster)
        {
            var statistics = new List<PlayerMatchStatistics>();
            foreach (var row in roster.Players)
            {
                var playerId = ResolvePlayer(context, rosterId, roster.Name, row);
                var isInOtherSquad = context.ParticipantRosters.TryGetValue(playerId, out var firstRosterId) && firstRosterId != rosterId;
                if (isInOtherSquad && row.ExternalId is not null)
                {
                    // The website links two different players (one in each squad) to the same player id.
                    _report.Warnings.Add(new FileError(
                        context.Path,
                        $"Player id {row.ExternalId} ('{row.Name}') is used for both squads of the match; this one was imported without a website id."));
                    playerId = ResolveWithoutWebsiteId(context, roster.Name, row);
                }

                if (!context.Participants.Add(playerId))
                {
                    _report.FileErrors.Add(new FileError(context.Path, $"Player '{row.Name}' resolved twice in one match; second line skipped."));
                    continue;
                }

                context.ParticipantRosters[playerId] = rosterId;

                context.ParticipantsByNameKey.TryAdd(PlayerNameKey.Create(row.Name), playerId);
                _catalog.EnsureRosterEntry(rosterId, playerId, row.Number, row.Name);

                statistics.Add(new PlayerMatchStatistics(
                    number: row.Number,
                    pointsTotal: row.PointsTotal,
                    serveTotal: row.ServeTotal,
                    serveErrors: row.ServeErrors,
                    aces: row.Aces,
                    receptionTotal: row.ReceptionTotal,
                    receptionErrors: row.ReceptionErrors,
                    receptionPositivePercent: StatisticsParsing.ParsePercent(row.ReceptionPositivePercent),
                    receptionPerfectPercent: StatisticsParsing.ParsePercent(row.ReceptionPerfectPercent),
                    attackTotal: row.AttackTotal,
                    attackErrors: row.AttackErrors,
                    attackBlocked: row.AttackBlocked,
                    attackPoints: row.AttackPoints,
                    attackEfficiencyPercent: StatisticsParsing.ParsePercent(row.AttackEfficiencyPercent),
                    blocks: row.Blocks,
                    matchId: context.MatchId,
                    playerId: playerId,
                    rosterId: rosterId));
            }

            return statistics;
        }

        /// <summary>
        /// Player id from the file → roster shirt number confirmed by name → roster name (members who belonged to
        /// the club on the match date first) → the only player of that name on any roster of the season → unresolved player.
        /// </summary>
        private Guid ResolvePlayer(MatchImportContext context, Guid rosterId, string rosterName, PlayerStatisticsFile row)
        {
            if (row.ExternalId is not null)
            {
                return _catalog.ResolvePlayer(context.LeagueId, row.ExternalId.Value, row.Name);
            }

            var nameKey = PlayerNameKey.Create(row.Name);
            var fromRoster = RosterMatching.Find(_catalog.GetRosterMembers(rosterId), row.Number, nameKey, context.Date);
            if (fromRoster is not null)
            {
                _report.PlayersResolvedByRoster++;
                return fromRoster.Value;
            }

            var fromSeason = _catalog.FindPlayerInSeason(context.LeagueId, context.File.Season.StartYear, row.Name);
            if (fromSeason is not null)
            {
                _report.PlayersResolvedBySeason++;
                return fromSeason.Value;
            }

            return ResolveWithoutWebsiteId(context, rosterName, row);
        }

        /// <summary>
        /// A separate player without a website id. Used when the id cannot be trusted (the website uses it for both
        /// squads of the match), so neither the id nor the name may lead back to the player already in the match.
        /// </summary>
        private Guid ResolveWithoutWebsiteId(MatchImportContext context, string rosterName, PlayerStatisticsFile row)
        {
            var file = context.File;
            _report.UnresolvedPlayers.Add(new UnresolvedPlayer(
                file.League, $"{file.Season.StartYear}-{file.Season.EndYear}", file.MatchId, rosterName, row.Name, row.Number));
            return _catalog.ResolveUnresolvedPlayer(context.LeagueId, row.Name);
        }

        private Guid? ResolveMvp(MatchImportContext context)
        {
            var mvp = context.File.Mvp;
            if (mvp is null)
            {
                return null;
            }

            if (mvp.ExternalId is not null)
            {
                return _catalog.FindPlayer(context.LeagueId, mvp.ExternalId.Value)
                    ?? _catalog.ResolvePlayer(context.LeagueId, mvp.ExternalId.Value, mvp.Name);
            }

            if (context.ParticipantsByNameKey.TryGetValue(PlayerNameKey.Create(mvp.Name), out var playerId))
            {
                return playerId;
            }

            _report.MatchesWithoutMvp++;
            _report.UnresolvedMvps.Add($"{context.File.League} {context.File.MatchId}: {mvp.Name}");
            return null;
        }

    }
}
