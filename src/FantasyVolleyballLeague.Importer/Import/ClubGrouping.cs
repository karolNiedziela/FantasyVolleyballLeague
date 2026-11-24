using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// Groups the rosters into clubs: one club per <see cref="ClubKey"/>, except the keys that a group of
    /// <c>ManualData/clubs.json</c> merges into one club.
    /// </summary>
    internal static class ClubGrouping
    {
        public const int MinIdsPerGroup = 2;

        /// <summary>
        /// Checks the entries of <c>club-splits.json</c>. <paramref name="leagueGenders"/> maps each league key to its
        /// gender.
        /// </summary>
        public static IReadOnlyList<string> ValidateSplits(IReadOnlyList<ClubSplitFile> splits, IReadOnlyDictionary<string, string> leagueGenders)
        {
            var errors = new List<string>();
            var splitKeys = new HashSet<ClubKey>();
            for (var index = 0; index < splits.Count; index++)
            {
                var split = splits[index];
                if (!ClubKey.TryParse(split.Id, out var clubKey) || clubKey.FromStartYear is not null)
                {
                    errors.Add($"Entry {index}: '{split.Id}' is not 'League{ClubKey.Separator}clubId' with a positive club id.");
                }
                else if (!leagueGenders.ContainsKey(clubKey.LeagueKey))
                {
                    errors.Add($"Entry {index}: unknown league '{clubKey.LeagueKey}'.");
                }
                else if (split.FromStartYear <= 0)
                {
                    errors.Add($"Entry {index}: fromStartYear must be a positive year.");
                }
                else if (!splitKeys.Add(clubKey with { FromStartYear = split.FromStartYear }))
                {
                    errors.Add($"Entry {index}: '{split.Id}' is split from {split.FromStartYear} by an earlier entry.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Checks the entries of <c>clubs.json</c>. <paramref name="leagueGenders"/> maps each league key to its gender;
        /// an id with <c>@fromStartYear</c> must name a split of <paramref name="splits"/>.
        /// </summary>
        public static IReadOnlyList<string> Validate(
            IReadOnlyList<ClubGroupFile> entries,
            IReadOnlyDictionary<string, string> leagueGenders,
            IReadOnlyList<ClubSplitFile> splits)
        {
            var errors = new List<string>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            var clubKeys = new HashSet<ClubKey>();
            var splitKeys = SplitKeys(splits);
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (string.IsNullOrWhiteSpace(entry.Key))
                {
                    errors.Add($"Entry {index}: the key is blank.");
                }
                else if (!keys.Add(entry.Key))
                {
                    errors.Add($"Entry {index}: the key '{entry.Key}' is used by an earlier entry.");
                }

                IReadOnlyList<string> ids = entry.Ids ?? [];
                if (ids.Count < MinIdsPerGroup)
                {
                    errors.Add($"Entry {index}: a group needs at least {MinIdsPerGroup} ids.");
                }

                var genders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var id in ids)
                {
                    if (!ClubKey.TryParse(id, out var clubKey))
                    {
                        errors.Add(
                            $"Entry {index}: '{id}' is not 'League{ClubKey.Separator}clubId' with a positive club id " +
                            $"(optionally followed by '{ClubKey.SplitSeparator}fromStartYear').");
                    }
                    else if (!leagueGenders.TryGetValue(clubKey.LeagueKey, out var gender))
                    {
                        errors.Add($"Entry {index}: unknown league '{clubKey.LeagueKey}'.");
                    }
                    else if (clubKey.FromStartYear is not null && !splitKeys.Contains(clubKey))
                    {
                        errors.Add($"Entry {index}: '{id}' names a split that club-splits.json does not have.");
                    }
                    else
                    {
                        genders.Add(gender);
                        if (!clubKeys.Add(clubKey))
                        {
                            errors.Add($"Entry {index}: '{id}' is listed more than once in the file.");
                        }
                    }
                }

                if (genders.Count > 1)
                {
                    errors.Add($"Entry {index}: the group mixes leagues of different genders.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Groups <paramref name="rosters"/> into clubs; <paramref name="groups"/> and <paramref name="splits"/> must
        /// have passed <see cref="Validate"/> and <see cref="ValidateSplits"/>. A split club id gives one key per part
        /// (<see cref="ClubKey.FromStartYear"/>). The groups are taken from the earliest season (then the smallest roster id), and each
        /// keeps the oldest club its rosters use that no earlier group kept (ids are UUID v7, so the smallest is the
        /// oldest). So when a merge is removed, the part with the earliest season keeps the club and the other part
        /// gets a new one. A club is named after its newest roster, and of two rosters in the newest season after
        /// the one imported later (the greater roster id).
        /// </summary>
        public static ClubGroupingResult Group(
            IReadOnlyList<ClubRoster> rosters,
            IReadOnlyList<ClubGroupFile> groups,
            IReadOnlyList<ClubSplitFile> splits,
            IReadOnlyCollection<Guid> existingClubIds)
        {
            var splitYears = SplitKeys(splits)
                .GroupBy(splitKey => (splitKey.LeagueKey, ExternalId: splitKey.ExternalId!.Value))
                .ToDictionary(idSplits => idSplits.Key, idSplits => idSplits.Select(splitKey => splitKey.FromStartYear!.Value).ToArray());
            var rostersByKey = rosters
                .GroupBy(roster => ClubKey.ForRoster(roster, splitYears))
                .ToDictionary(keyRosters => keyRosters.Key, keyRosters => keyRosters.ToList());

            var clubRosters = new List<List<ClubRoster>>();
            var mergedKeys = new HashSet<ClubKey>();
            var unmatchedIds = new List<string>();
            foreach (var group in groups)
            {
                var groupRosters = new List<ClubRoster>();
                foreach (var id in group.Ids)
                {
                    var clubKey = ClubKey.Parse(id);
                    mergedKeys.Add(clubKey);
                    if (rostersByKey.TryGetValue(clubKey, out var keyRosters))
                    {
                        groupRosters.AddRange(keyRosters);
                    }
                    else
                    {
                        unmatchedIds.Add(id);
                    }
                }

                if (groupRosters.Count > 0)
                {
                    clubRosters.Add(groupRosters);
                }
            }

            var unmergedRosters = rostersByKey
                .Where(keyRosters => !mergedKeys.Contains(keyRosters.Key))
                .Select(keyRosters => keyRosters.Value);
            clubRosters.AddRange(unmergedRosters);

            var keptClubIds = new HashSet<Guid>();
            var clubs = new List<GroupedClub>();
            var clubRostersFromEarliestSeason = clubRosters
                .OrderBy(club => club.Min(roster => roster.StartYear))
                .ThenBy(club => club.Min(roster => roster.RosterId));
            foreach (var club in clubRostersFromEarliestSeason)
            {
                var freeClubIds = club
                    .Select(roster => roster.ClubId)
                    .Where(clubId => !keptClubIds.Contains(clubId))
                    .Order()
                    .ToList();
                Guid? existingClubId = freeClubIds.Count > 0 ? freeClubIds[0] : null;
                if (existingClubId is not null)
                {
                    keptClubIds.Add(existingClubId.Value);
                }

                var newestRoster = club.MaxBy(roster => (roster.StartYear, roster.RosterId))!;
                var rosterIds = club.Select(roster => roster.RosterId).Order().ToList();
                clubs.Add(new GroupedClub(existingClubId, newestRoster.Name, rosterIds));
            }

            var clubIdsToDelete = existingClubIds
                .Where(clubId => !keptClubIds.Contains(clubId))
                .Order()
                .ToList();

            return new ClubGroupingResult(clubs, clubIdsToDelete, unmatchedIds);
        }

        /// <summary>
        /// The parts of the split club ids as keys (<c>League:clubId@fromStartYear</c>); entries that do not parse are
        /// left out (<see cref="ValidateSplits"/> reports them).
        /// </summary>
        private static HashSet<ClubKey> SplitKeys(IReadOnlyList<ClubSplitFile> splits)
        {
            var splitKeys = new HashSet<ClubKey>();
            foreach (var split in splits)
            {
                if (ClubKey.TryParse(split.Id, out var clubKey) && clubKey.FromStartYear is null)
                {
                    splitKeys.Add(clubKey with { FromStartYear = split.FromStartYear });
                }
            }

            return splitKeys;
        }
    }
}
