using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;
using FantasyVolleyballLeague.Shared.Text;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FantasyVolleyballLeague.Importer.Steps
{
    /// <summary>
    /// Groups the rosters into clubs (<see cref="ClubGrouping"/>): one club per league and club id, except the ids that
    /// <c>ManualData/clubs.json</c> merges and the ids that <c>ManualData/club-splits.json</c> splits. Runs after the rosters and the matches, because both create rosters. A file
    /// with errors is reported and the clubs stay as they were, so a typo does not split the merged clubs and change
    /// their ids.
    /// </summary>
    internal sealed class ClubGroupingImportStep : IImportStep
    {
        private readonly FantasyVolleyballLeagueDbContext _dbContext;
        private readonly ImportCatalog _catalog;
        private readonly ManualDataReader _manualDataReader;
        private readonly ImportReport _report;
        private readonly ImporterOptions _options;

        public ClubGroupingImportStep(
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

        public int Order => 4;

        public string Name => "Group the rosters into clubs";

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var leagues = await _catalog.EnsureLeaguesAsync(_options.Leagues, cancellationToken);
            var leagueGenders = leagues.Values.ToDictionary(league => league.Key, league => league.Gender);

            var clubSplits = await _manualDataReader.ReadValidatedAsync<ClubSplitFile>(
                ManualDataReader.ClubSplitsFileName,
                entries => ClubGrouping.ValidateSplits(entries, leagueGenders),
                cancellationToken);
            ReportFileErrors(ManualDataReader.ClubSplitsFileName, clubSplits.Errors);
            if (clubSplits.Entries is null)
            {
                Console.WriteLine($"  {ManualDataReader.ClubSplitsFileName} has errors; the clubs were not regrouped");
                return;
            }

            var clubGroups = await _manualDataReader.ReadValidatedAsync<ClubGroupFile>(
                ManualDataReader.ClubsFileName,
                entries => ClubGrouping.Validate(entries, leagueGenders, clubSplits.Entries),
                cancellationToken);
            ReportFileErrors(ManualDataReader.ClubsFileName, clubGroups.Errors);
            if (clubGroups.Entries is null)
            {
                Console.WriteLine($"  {ManualDataReader.ClubsFileName} has errors; the clubs were not regrouped");
                return;
            }

            var leagueIds = leagues.Keys.ToList();
            var rosters = await _dbContext.Set<Roster>()
                .Include(roster => roster.Season)
                .Where(roster => leagueIds.Contains(roster.LeagueId))
                .ToListAsync(cancellationToken);
            var clubs = await _dbContext.Set<Club>().ToDictionaryAsync(club => club.Id, cancellationToken);

            // The rosters of leagues missing from Importer:Leagues are not grouped, so their clubs must stay.
            var clubIdsOfOtherLeagues = await _dbContext.Set<Roster>()
                .Where(roster => !leagueIds.Contains(roster.LeagueId))
                .Select(roster => roster.ClubId)
                .Distinct()
                .ToListAsync(cancellationToken);
            var existingClubIds = clubs.Keys.Except(clubIdsOfOtherLeagues).ToList();

            var clubRosters = rosters
                .Select(roster => new ClubRoster(
                    roster.Id,
                    leagues[roster.LeagueId].Key,
                    roster.ExternalId,
                    roster.NormalizedName,
                    roster.ClubId,
                    roster.Season.StartYear,
                    roster.Name))
                .ToList();
            var result = ClubGrouping.Group(clubRosters, clubGroups.Entries, clubSplits.Entries, existingClubIds);

            var rostersById = rosters.ToDictionary(roster => roster.Id);
            foreach (var groupedClub in result.Clubs)
            {
                var club = ResolveClub(groupedClub, clubs, rostersById, leagues);
                foreach (var rosterId in groupedClub.RosterIds)
                {
                    var roster = rostersById[rosterId];
                    if (roster.ClubId != club.Id)
                    {
                        roster.AssignClub(club.Id);
                        _report.RostersMovedToAnotherClub++;
                    }
                }
            }

            var clubsToDelete = result.ClubIdsToDelete.Select(clubId => clubs[clubId]).ToList();
            _dbContext.Set<Club>().RemoveRange(clubsToDelete);
            _report.ClubsDeleted += clubsToDelete.Count;

            var staleClubs = clubs.Values.Where(club => club.NormalizedName != NameNormalizer.Normalize(club.Name)).ToList();
            foreach (var club in staleClubs)
            {
                club.RefreshNormalizedName();
            }

            var clubsPath = _manualDataReader.GetPath(ManualDataReader.ClubsFileName);
            foreach (var id in result.UnmatchedIds)
            {
                _report.Warnings.Add(new FileError(clubsPath, $"Id '{id}' matches no roster."));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
        }

        private void ReportFileErrors(string fileName, IReadOnlyList<string> errors)
        {
            var path = _manualDataReader.GetPath(fileName);
            foreach (var error in errors)
            {
                _report.FileErrors.Add(new FileError(path, error));
            }
        }

        private Club ResolveClub(
            GroupedClub groupedClub,
            Dictionary<Guid, Club> clubs,
            Dictionary<Guid, Roster> rostersById,
            IReadOnlyDictionary<Guid, LeagueImportOptions> leagues)
        {
            if (groupedClub.ExistingClubId is { } clubId)
            {
                var club = clubs[clubId];
                if (club.Name != groupedClub.Name)
                {
                    club.Rename(groupedClub.Name);
                    _report.ClubsRenamed++;
                }

                return club;
            }

            // ClubGrouping.Validate guarantees that all rosters of a club are of one gender.
            var leagueId = rostersById[groupedClub.RosterIds[0]].LeagueId;
            var newClub = new Club(groupedClub.Name, leagues[leagueId].ResolveGender().Id);
            _dbContext.Set<Club>().Add(newClub);
            _report.ClubsCreated++;
            return newClub;
        }
    }
}
