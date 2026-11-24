using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;
using FantasyVolleyballLeague.Shared.Text;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class ClubGroupingTests
    {
        private const string PlusLiga = "PlusLiga";
        private const string Pls1Liga = "PLS1Liga";
        private const string TauronLigaKobiet = "TauronLigaKobiet";

        private static readonly Dictionary<string, string> LeagueGenders = new()
        {
            [PlusLiga] = "Men",
            [Pls1Liga] = "Men",
            [TauronLigaKobiet] = "Women",
        };
        
        private static readonly Guid OlderClub = Id(1);
        private static readonly Guid NewerClub = Id(2);
        private static readonly Guid NewestClub = Id(3);
        private static readonly Guid FirstRoster = Id(11);
        private static readonly Guid SecondRoster = Id(12);
        private static readonly Guid ThirdRoster = Id(13);

        [Fact]
        public void Group_Should_MakeOneClubOfTheRostersOfOneLeagueAndClubId()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, PlusLiga, 45, OlderClub, 2021, "LUK Lublin"),
                Roster(SecondRoster, PlusLiga, 45, OlderClub, 2022, "LUK Lublin"),
            ];

            var result = ClubGrouping.Group(rosters, [], [], [OlderClub]);

            var club = result.Clubs.ShouldHaveSingleItem();
            club.ShouldSatisfyAllConditions(
                () => club.ExistingClubId.ShouldBe(OlderClub),
                () => club.RosterIds.ShouldBe([FirstRoster, SecondRoster]));
            result.ClubIdsToDelete.ShouldBeEmpty();
        }

        [Fact]
        public void Group_Should_KeepTheSameClubIdOfTwoLeaguesApart()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, PlusLiga, 11, OlderClub, 2021, "BBTS Bielsko-Biała"),
                Roster(SecondRoster, Pls1Liga, 11, NewerClub, 2021, "Exact Systems Hemarpol Częstochowa"),
            ];

            var result = ClubGrouping.Group(rosters, [], [], [OlderClub, NewerClub]);

            result.Clubs.Select(club => club.ExistingClubId).ShouldBe([OlderClub, NewerClub], ignoreOrder: true);
        }

        [Fact]
        public void Group_Should_GroupTheRostersWithoutAClubIdByLeagueAndName()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, PlusLiga, null, OlderClub, 2001, "AZS Częstochowa"),
                Roster(SecondRoster, PlusLiga, null, OlderClub, 2002, "AZS Częstochowa"),
                Roster(ThirdRoster, Pls1Liga, null, NewerClub, 2019, "AZS Częstochowa"),
            ];

            var result = ClubGrouping.Group(rosters, [], [], [OlderClub, NewerClub]);

            result.Clubs.Select(club => club.RosterIds).ShouldBe([[FirstRoster, SecondRoster], [ThirdRoster]]);
        }

        [Fact]
        public void Group_Should_MergeTheIdsOfAGroup_KeepingTheOldestClub()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 46, NewerClub, 2019, "LUK Politechnika Lublin"),
                Roster(SecondRoster, PlusLiga, 45, OlderClub, 2021, "LUK Lublin"),
            ];
            ClubGroupFile[] groups = [new("lublin", ["PLS1Liga:46", "PlusLiga:45"])];

            var result = ClubGrouping.Group(rosters, groups, [], [OlderClub, NewerClub]);

            var club = result.Clubs.ShouldHaveSingleItem();
            club.ShouldSatisfyAllConditions(
                () => club.ExistingClubId.ShouldBe(OlderClub),
                () => club.RosterIds.ShouldBe([FirstRoster, SecondRoster]));
            result.ClubIdsToDelete.ShouldBe([NewerClub]);
        }

        [Fact]
        public void Group_Should_MergeTwoIdsOfOneLeagueWithAnIdOfAnotherLeague()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 69, OlderClub, 2018, "Stal Nysa"),
                Roster(SecondRoster, Pls1Liga, 90, NewerClub, 2019, "Stal Nysa"),
                Roster(ThirdRoster, PlusLiga, 68, NewestClub, 2020, "Stal Nysa"),
            ];
            ClubGroupFile[] groups = [new("stal-nysa", ["PLS1Liga:69", "PLS1Liga:90", "PlusLiga:68"])];

            var result = ClubGrouping.Group(rosters, groups, [], [OlderClub, NewerClub, NewestClub]);

            var club = result.Clubs.ShouldHaveSingleItem();
            club.RosterIds.ShouldBe([FirstRoster, SecondRoster, ThirdRoster]);
            result.ClubIdsToDelete.ShouldBe([NewerClub, NewestClub]);
        }

        [Fact]
        public void Group_Should_LeaveTheClubToThePartWithTheEarliestSeason_When_AMergeIsRemoved()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, PlusLiga, 45, OlderClub, 2021, "LUK Lublin"),
                Roster(SecondRoster, Pls1Liga, 46, OlderClub, 2019, "LUK Politechnika Lublin"),
            ];

            var result = ClubGrouping.Group(rosters, [], [], [OlderClub]);

            result.Clubs.ShouldSatisfyAllConditions(
                () => result.Clubs.Single(club => club.RosterIds.Contains(SecondRoster)).ExistingClubId.ShouldBe(OlderClub),
                () => result.Clubs.Single(club => club.RosterIds.Contains(FirstRoster)).ExistingClubId.ShouldBeNull());
            result.ClubIdsToDelete.ShouldBeEmpty();
        }

        [Fact]
        public void Group_Should_NameTheClubAfterTheNewestRoster()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 46, OlderClub, 2020, "LUK Lublin"),
                Roster(SecondRoster, Pls1Liga, 46, OlderClub, 2019, "LUK Politechnika Lublin"),
            ];

            var result = ClubGrouping.Group(rosters, [], [], [OlderClub]);

            result.Clubs.ShouldHaveSingleItem().Name.ShouldBe("LUK Lublin");
        }

        [Fact]
        public void Group_Should_NameTheClubAfterTheRosterImportedLater_When_TwoRostersShareTheNewestSeason()
        {
            ClubRoster[] rosters =
            [
                Roster(SecondRoster, PlusLiga, 52, NewerClub, 2021, "Name imported later"),
                Roster(FirstRoster, Pls1Liga, 53, OlderClub, 2021, "Name imported first"),
            ];
            ClubGroupFile[] groups = [new("mks-bedzin", ["PLS1Liga:53", "PlusLiga:52"])];

            var result = ClubGrouping.Group(rosters, groups, [], [OlderClub, NewerClub]);

            result.Clubs.ShouldHaveSingleItem().Name.ShouldBe("Name imported later");
        }

        [Fact]
        public void Group_Should_DeleteAClubWithoutRosters()
        {
            ClubRoster[] rosters = [Roster(FirstRoster, PlusLiga, 45, OlderClub, 2021, "LUK Lublin")];

            var result = ClubGrouping.Group(rosters, [], [], [OlderClub, NewerClub]);

            result.ClubIdsToDelete.ShouldBe([NewerClub]);
        }

        [Fact]
        public void Group_Should_ReportTheIdsOfAGroupWithoutRosters_AndMergeTheRest()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 46, OlderClub, 2019, "LUK Politechnika Lublin"),
                Roster(SecondRoster, PlusLiga, 45, NewerClub, 2021, "LUK Lublin"),
            ];
            ClubGroupFile[] groups = [new("lublin", ["PLS1Liga:46", "PlusLiga:45", "PlusLiga:999"])];

            var result = ClubGrouping.Group(rosters, groups, [], [OlderClub, NewerClub]);

            result.UnmatchedIds.ShouldBe(["PlusLiga:999"]);
            result.Clubs.ShouldHaveSingleItem().RosterIds.ShouldBe([FirstRoster, SecondRoster]);
        }

        [Fact]
        public void Group_Should_ChangeNothing_When_TheRostersAreAlreadyGrouped()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 46, OlderClub, 2019, "LUK Politechnika Lublin"),
                Roster(SecondRoster, PlusLiga, 45, OlderClub, 2021, "LUK Lublin"),
                Roster(ThirdRoster, PlusLiga, 11, NewerClub, 2021, "BBTS Bielsko-Biała"),
            ];
            ClubGroupFile[] groups = [new("lublin", ["PLS1Liga:46", "PlusLiga:45"])];

            var result = ClubGrouping.Group(rosters, groups, [], [OlderClub, NewerClub]);

            result.Clubs.Select(club => club.ExistingClubId).ShouldBe([OlderClub, NewerClub], ignoreOrder: true);
            result.ClubIdsToDelete.ShouldBeEmpty();
        }

        [Fact]
        public void Group_Should_GiveThePartAfterASplitItsOwnClub_AndLeaveTheExistingClubToTheEarlierPart()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 57, OlderClub, 2022, "MKST Astra Nowa Sól"),
                Roster(SecondRoster, Pls1Liga, 57, OlderClub, 2025, "Karton-Pak Astra Nowa Sól"),
                Roster(ThirdRoster, Pls1Liga, 57, OlderClub, 2026, "FORT Poznań"),
            ];
            ClubSplitFile[] splits = [new("PLS1Liga:57", 2026)];

            var result = ClubGrouping.Group(rosters, [], splits, [OlderClub]);

            var astra = result.Clubs.Single(club => club.RosterIds.Contains(FirstRoster));
            var fort = result.Clubs.Single(club => club.RosterIds.Contains(ThirdRoster));
            result.ShouldSatisfyAllConditions(
                () => astra.ExistingClubId.ShouldBe(OlderClub),
                () => astra.Name.ShouldBe("Karton-Pak Astra Nowa Sól"),
                () => astra.RosterIds.ShouldBe([FirstRoster, SecondRoster]),
                () => fort.ExistingClubId.ShouldBeNull(),
                () => fort.Name.ShouldBe("FORT Poznań"),
                () => fort.RosterIds.ShouldBe([ThirdRoster]),
                () => result.ClubIdsToDelete.ShouldBeEmpty());
        }

        [Fact]
        public void Group_Should_MergeThePartAfterASplit_When_AGroupNamesItWithTheYear()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 57, OlderClub, 2025, "Karton-Pak Astra Nowa Sól"),
                Roster(SecondRoster, Pls1Liga, 57, OlderClub, 2026, "FORT Poznań"),
                Roster(ThirdRoster, PlusLiga, 999, NewerClub, 2027, "FORT Poznań"),
            ];
            ClubSplitFile[] splits = [new("PLS1Liga:57", 2026)];
            ClubGroupFile[] groups = [new("fort-poznan", ["PLS1Liga:57@2026", "PlusLiga:999"])];

            var result = ClubGrouping.Group(rosters, groups, splits, [OlderClub, NewerClub]);

            result.Clubs.Select(club => club.RosterIds).ShouldBe([[FirstRoster], [SecondRoster, ThirdRoster]], ignoreOrder: true);
        }

        [Fact]
        public void Group_Should_MergeThePartBeforeTheSplit_When_AGroupNamesTheIdWithoutAYear()
        {
            ClubRoster[] rosters =
            [
                Roster(FirstRoster, Pls1Liga, 57, OlderClub, 2025, "Karton-Pak Astra Nowa Sól"),
                Roster(SecondRoster, Pls1Liga, 57, OlderClub, 2026, "FORT Poznań"),
                Roster(ThirdRoster, PlusLiga, 999, NewerClub, 2021, "Astra Nowa Sól"),
            ];
            ClubSplitFile[] splits = [new("PLS1Liga:57", 2026)];
            ClubGroupFile[] groups = [new("astra-nowa-sol", ["PLS1Liga:57", "PlusLiga:999"])];

            var result = ClubGrouping.Group(rosters, groups, splits, [OlderClub, NewerClub]);

            result.Clubs.Select(club => club.RosterIds).ShouldBe([[FirstRoster, ThirdRoster], [SecondRoster]], ignoreOrder: true);
        }

        [Fact]
        public void ValidateSplits_Should_ReturnNoErrors_When_TheSplitsAreValid()
        {
            ClubSplitFile[] splits = [new("PLS1Liga:57", 2026), new("PLS1Liga:57", 2030)];

            ClubGrouping.ValidateSplits(splits, LeagueGenders).ShouldBeEmpty();
        }

        [Theory]
        [InlineData("PLS1Liga57")]
        [InlineData("PLS1Liga:57@2026")]
        [InlineData("")]
        public void ValidateSplits_Should_ReportAMalformedId(string id)
        {
            ClubSplitFile[] splits = [new(id, 2026)];

            var errors = ClubGrouping.ValidateSplits(splits, LeagueGenders);

            errors.ShouldHaveSingleItem().ShouldContain("is not 'League:clubId' with a positive club id");
        }

        [Fact]
        public void ValidateSplits_Should_ReportAnUnknownLeague()
        {
            ClubSplitFile[] splits = [new("Pls1Liga:57", 2026)];

            ClubGrouping.ValidateSplits(splits, LeagueGenders).ShouldBe(["Entry 0: unknown league 'Pls1Liga'."]);
        }

        [Fact]
        public void ValidateSplits_Should_ReportAYearThatIsNotPositive()
        {
            ClubSplitFile[] splits = [new("PLS1Liga:57", 0)];

            ClubGrouping.ValidateSplits(splits, LeagueGenders).ShouldBe(["Entry 0: fromStartYear must be a positive year."]);
        }

        [Fact]
        public void ValidateSplits_Should_ReportARepeatedSplit()
        {
            ClubSplitFile[] splits = [new("PLS1Liga:57", 2026), new("PLS1Liga:057", 2026)];

            ClubGrouping.ValidateSplits(splits, LeagueGenders).ShouldBe(["Entry 1: 'PLS1Liga:057' is split from 2026 by an earlier entry."]);
        }

        [Fact]
        public void Validate_Should_AcceptThePartOfASplit()
        {
            ClubGroupFile[] entries = [new("fort-poznan", ["PLS1Liga:57@2026", "PlusLiga:999"])];
            ClubSplitFile[] splits = [new("PLS1Liga:57", 2026)];

            ClubGrouping.Validate(entries, LeagueGenders, splits).ShouldBeEmpty();
        }

        [Fact]
        public void Validate_Should_ReportThePartOfASplitThatDoesNotExist()
        {
            ClubGroupFile[] entries = [new("fort-poznan", ["PLS1Liga:57@2026", "PlusLiga:999"])];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 0: 'PLS1Liga:57@2026' names a split that club-splits.json does not have."]);
        }

        [Fact]
        public void Validate_Should_ReturnNoErrors_When_TheGroupsAreValid()
        {
            ClubGroupFile[] entries =
            [
                new("lublin", ["PLS1Liga:46", "PlusLiga:45"]),
                new("stal-mielec", ["TauronLigaKobiet:40", "TauronLigaKobiet:36"]),
            ];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBeEmpty();
        }

        [Theory]
        [InlineData("PlusLiga45")]
        [InlineData("PlusLiga:abc")]
        [InlineData("PlusLiga:0")]
        [InlineData("PlusLiga:-45")]
        [InlineData(":45")]
        [InlineData("")]
        [InlineData("PlusLiga:45@")]
        [InlineData("PlusLiga:45@abc")]
        [InlineData("PlusLiga:45@0")]
        public void Validate_Should_ReportAMalformedId(string id)
        {
            ClubGroupFile[] entries = [new("lublin", [id, "PLS1Liga:46"])];

            var errors = ClubGrouping.Validate(entries, LeagueGenders, []);

            errors.ShouldHaveSingleItem().ShouldContain("is not 'League:clubId' with a positive club id");
        }

        [Fact]
        public void Validate_Should_ReportAnUnknownLeague()
        {
            ClubGroupFile[] entries = [new("lublin", ["Plusliga:45", "PLS1Liga:46"])];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 0: unknown league 'Plusliga'."]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_Should_ReportABlankKey(string? key)
        {
            ClubGroupFile[] entries = [new(key!, ["PLS1Liga:46", "PlusLiga:45"])];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 0: the key is blank."]);
        }

        [Fact]
        public void Validate_Should_ReportARepeatedKey()
        {
            ClubGroupFile[] entries =
            [
                new("lublin", ["PLS1Liga:46", "PlusLiga:45"]),
                new("lublin", ["PLS1Liga:12", "PlusLiga:11"]),
            ];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 1: the key 'lublin' is used by an earlier entry."]);
        }

        [Fact]
        public void Validate_Should_ReportAGroupWithFewerThanTwoIds()
        {
            ClubGroupFile[] entries = [new("lublin", ["PlusLiga:45"])];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 0: a group needs at least 2 ids."]);
        }

        [Fact]
        public void Validate_Should_ReportAnIdListedInTwoGroups()
        {
            ClubGroupFile[] entries =
            [
                new("lublin", ["PLS1Liga:46", "PlusLiga:45"]),
                new("luk-lublin", ["PLS1Liga:46", "PlusLiga:11"]),
            ];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 1: 'PLS1Liga:46' is listed more than once in the file."]);
        }

        [Fact]
        public void Validate_Should_ReportAnIdListedTwiceInOneGroup()
        {
            ClubGroupFile[] entries = [new("lublin", ["PlusLiga:45", "PlusLiga:045"])];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 0: 'PlusLiga:045' is listed more than once in the file."]);
        }

        [Fact]
        public void Validate_Should_ReportAGroupMixingGenders()
        {
            ClubGroupFile[] entries = [new("stal-mielec", ["PlusLiga:45", "TauronLigaKobiet:40"])];

            ClubGrouping.Validate(entries, LeagueGenders, []).ShouldBe(["Entry 0: the group mixes leagues of different genders."]);
        }

        private static ClubRoster Roster(Guid rosterId, string leagueKey, int? externalId, Guid clubId, int startYear, string name)
        {
            return new ClubRoster(rosterId, leagueKey, externalId, NameNormalizer.Normalize(name), clubId, startYear, name);
        }

        private static Guid Id(int order)
        {
            return new Guid(order, 0, 0x7000, 0x80, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}
