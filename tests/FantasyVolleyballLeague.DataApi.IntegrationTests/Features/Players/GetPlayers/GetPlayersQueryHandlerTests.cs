using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Players;
using FantasyVolleyballLeague.DataApi.Features.Players.GetPlayers;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetPlayers
{
    public sealed class GetPlayersQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _otherMen = new League("Men's league 2", 2, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetPlayersQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_ThereAreNoPlayers()
        {
            var page = await Handle(new GetPlayersQuery());

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_MapThePlayerProfileAndGender()
        {
            var player = NameSeeds.Player(
                "Łukasz Kaczmarek",
                LeagueGender.Women,
                dateOfBirth: new DateOnly(1993, 5, 2),
                heightCm: 201,
                weightKg: 96,
                attackReachCm: 350);
            await SeedAsync(player);

            var page = await Handle(new GetPlayersQuery());

            var item = page.Items.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.Id.ShouldBe(player.Id),
                () => item.FullName.ShouldBe("Łukasz Kaczmarek"),
                () => item.Gender.ShouldBe("women"),
                () => item.DateOfBirth.ShouldBe(new DateOnly(1993, 5, 2)),
                () => item.HeightCm.ShouldBe(201),
                () => item.WeightKg.ShouldBe(96),
                () => item.AttackReachCm.ShouldBe(350),
                () => item.Position.ShouldBeNull());
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullProfileFields_When_TheyAreUnknown()
        {
            await SeedAsync(NameSeeds.Player("Jan Kowalski"));

            var page = await Handle(new GetPlayersQuery());

            var item = page.Items.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.DateOfBirth.ShouldBeNull(),
                () => item.HeightCm.ShouldBeNull(),
                () => item.WeightKg.ShouldBeNull(),
                () => item.AttackReachCm.ShouldBeNull());
        }

        [Fact]
        public async Task HandleAsync_Should_SortByFullNameAscendingByDefault_UsingThePolishCollation()
        {
            await SeedAsync(
                NameSeeds.Player("Zenon Zalewski"),
                NameSeeds.Player("Łazarz Nowak"),
                NameSeeds.Player("Leon Nowak"),
                NameSeeds.Player("Śliwka Adam"),
                NameSeeds.Player("Sobolewski Adam"));

            var page = await Handle(new GetPlayersQuery());

            page.Items.Select(player => player.FullName).ShouldBe(
                ["Leon Nowak", "Łazarz Nowak", "Sobolewski Adam", "Śliwka Adam", "Zenon Zalewski"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByFullNameDescending()
        {
            await SeedAsync(
                NameSeeds.Player("Leon Nowak"),
                NameSeeds.Player("Łazarz Nowak"),
                NameSeeds.Player("Zenon Zalewski"));

            var page = await Handle(new GetPlayersQuery(Sort: "-fullName"));

            page.Items.Select(player => player.FullName).ShouldBe(["Zenon Zalewski", "Łazarz Nowak", "Leon Nowak"]);
        }

        [Fact]
        public async Task HandleAsync_Should_PageTheResultsAndReportTheTotal()
        {
            await SeedAsync(
                NameSeeds.Player("Adam A"),
                NameSeeds.Player("Adam B"),
                NameSeeds.Player("Adam C"));

            var page = await Handle(new GetPlayersQuery(Offset: 1, Limit: 1));

            page.Items.Select(player => player.FullName).ShouldBe(["Adam B"]);
            page.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByGender()
        {
            await SeedAsync(
                NameSeeds.Player("Adam Man", LeagueGender.Men),
                NameSeeds.Player("Anna Woman", LeagueGender.Women));

            var page = await Handle(new GetPlayersQuery(Gender: "women"));

            page.Items.ShouldHaveSingleItem().FullName.ShouldBe("Anna Woman");
        }

        [Fact]
        public async Task HandleAsync_Should_SearchByWordsOfTheName_IgnoringOrderCaseAndDiacritics()
        {
            await SeedAsync(
                NameSeeds.Player("Łukasz Kaczmarek"),
                NameSeeds.Player("Łukasz Kowalski"));

            var page = await Handle(new GetPlayersQuery(Search: "KACZMAREK lukasz"));

            page.Items.ShouldHaveSingleItem().FullName.ShouldBe("Łukasz Kaczmarek");
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByRoster()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublinClub = NameSeeds.Club(_men, "Lublin");
            var zawiercieClub = NameSeeds.Club(_men, "Zawiercie");
            var lublin = NameSeeds.Roster(_men, season, lublinClub, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, zawiercieClub, "Zawiercie");
            var own = NameSeeds.Player("Own Player");
            var other = NameSeeds.Player("Other Player");
            var free = NameSeeds.Player("Without Roster");
            await SeedAsync(
                _men,
                season,
                lublinClub,
                zawiercieClub,
                lublin,
                zawiercie,
                own,
                other,
                free,
                NameSeeds.RosterPlayer(lublin, own),
                NameSeeds.RosterPlayer(zawiercie, other));

            var page = await Handle(new GetPlayersQuery(RosterId: lublin.Id));

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(own.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterBySeason()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var olderRoster = NameSeeds.Roster(_men, older, club, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, club, "Lublin");
            var inOlder = NameSeeds.Player("In Older");
            var inNewer = NameSeeds.Player("In Newer");
            await SeedAsync(
                _men,
                older,
                newer,
                club,
                olderRoster,
                newerRoster,
                inOlder,
                inNewer,
                NameSeeds.RosterPlayer(olderRoster, inOlder),
                NameSeeds.RosterPlayer(newerRoster, inNewer));

            var page = await Handle(new GetPlayersQuery(SeasonId: newer.Id));

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(inNewer.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByLeague()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_otherMen, 2023);
            var clubOfLeague = NameSeeds.Club(_men, "Lublin");
            var clubOfOther = NameSeeds.Club(_otherMen, "Lublin");
            var rosterOfLeague = NameSeeds.Roster(_men, season, clubOfLeague, "Lublin");
            var rosterOfOther = NameSeeds.Roster(_otherMen, otherSeason, clubOfOther, "Lublin");
            var inLeague = NameSeeds.Player("In League");
            var inOther = NameSeeds.Player("In Other League");
            await SeedAsync(
                _men,
                _otherMen,
                season,
                otherSeason,
                clubOfLeague,
                clubOfOther,
                rosterOfLeague,
                rosterOfOther,
                inLeague,
                inOther,
                NameSeeds.RosterPlayer(rosterOfLeague, inLeague),
                NameSeeds.RosterPlayer(rosterOfOther, inOther));

            var page = await Handle(new GetPlayersQuery(LeagueId: _otherMen.Id));

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(inOther.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ListAPlayerWithSeveralEntriesInOneSeasonOnce()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublinClub = NameSeeds.Club(_men, "Lublin");
            var zawiercieClub = NameSeeds.Club(_men, "Zawiercie");
            var lublin = NameSeeds.Roster(_men, season, lublinClub, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, zawiercieClub, "Zawiercie");
            var player = NameSeeds.Player("Transferred Player");
            await SeedAsync(
                _men,
                season,
                lublinClub,
                zawiercieClub,
                lublin,
                zawiercie,
                player,
                NameSeeds.RosterPlayer(lublin, player, memberFrom: new DateOnly(2023, 9, 1), memberTo: new DateOnly(2023, 12, 31)),
                NameSeeds.RosterPlayer(zawiercie, player, memberFrom: new DateOnly(2024, 1, 1)));

            var page = await Handle(new GetPlayersQuery(SeasonId: season.Id));

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(player.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnThePositionOfTheEntryInTheSelectedSeason()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var olderRoster = NameSeeds.Roster(_men, older, club, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, club, "Lublin");
            var player = NameSeeds.Player("Changed Position");
            await SeedAsync(
                _men,
                older,
                newer,
                club,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player, position: PlayerPosition.OutsideHitter),
                NameSeeds.RosterPlayer(newerRoster, player, position: PlayerPosition.OppositeHitter));

            var inOlder = await Handle(new GetPlayersQuery(SeasonId: older.Id));
            var inNewer = await Handle(new GetPlayersQuery(SeasonId: newer.Id));
            var anySeason = await Handle(new GetPlayersQuery());

            inOlder.Items.ShouldHaveSingleItem().Position.ShouldBe("outside_hitter");
            inNewer.Items.ShouldHaveSingleItem().Position.ShouldBe("opposite_hitter");
            anySeason.Items.ShouldHaveSingleItem().Position.ShouldBe("opposite_hitter");
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullPosition_When_TheCurrentRosterPlayerHasNone_EvenIfAnEarlierOneDid()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var olderRoster = NameSeeds.Roster(_men, older, lublin, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, zawiercie, "Zawiercie");
            var player = NameSeeds.Player("Unknown Latest Position");
            await SeedAsync(
                _men,
                older,
                newer,
                lublin,
                zawiercie,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(newerRoster, player));

            var page = await Handle(new GetPlayersQuery());

            var item = page.Items.ShouldHaveSingleItem();
            item.Position.ShouldBeNull();
            item.Roster.ShouldNotBeNull().Id.ShouldBe(newerRoster.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRosterAndThePositionOfTheSameCurrentRosterPlayer()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var olderRoster = NameSeeds.Roster(_men, older, lublin, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, zawiercie, "Zawiercie");
            var player = NameSeeds.Player("Moved Club");
            await SeedAsync(
                _men,
                older,
                newer,
                lublin,
                zawiercie,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(newerRoster, player, position: PlayerPosition.OppositeHitter));

            var current = await Handle(new GetPlayersQuery());
            var inOlder = await Handle(new GetPlayersQuery(SeasonId: older.Id));

            var item = current.Items.ShouldHaveSingleItem();
            item.Roster.ShouldNotBeNull().ShouldSatisfyAllConditions(
                roster => roster.Id.ShouldBe(newerRoster.Id),
                roster => roster.Name.ShouldBe("Zawiercie"));
            item.Position.ShouldBe("opposite_hitter");
            var olderItem = inOlder.Items.ShouldHaveSingleItem();
            olderItem.Roster.ShouldNotBeNull().Id.ShouldBe(olderRoster.Id);
            olderItem.Position.ShouldBe("setter");
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullRoster_When_ThePlayerHasNoRosterPlayer()
        {
            await SeedAsync(NameSeeds.Player("Without Roster"));

            var page = await Handle(new GetPlayersQuery());

            var item = page.Items.ShouldHaveSingleItem();
            item.Roster.ShouldBeNull();
            item.Position.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRosterTheFilterSelects_When_ThePlayerMovedWithinTheSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublinClub = NameSeeds.Club(_men, "Lublin");
            var zawiercieClub = NameSeeds.Club(_men, "Zawiercie");
            var lublin = NameSeeds.Roster(_men, season, lublinClub, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, zawiercieClub, "Zawiercie");
            var player = NameSeeds.Player("Transferred Player");
            await SeedAsync(
                _men,
                season,
                lublinClub,
                zawiercieClub,
                lublin,
                zawiercie,
                player,
                NameSeeds.RosterPlayer(lublin, player, position: PlayerPosition.OutsideHitter, memberFrom: new DateOnly(2023, 9, 1)),
                NameSeeds.RosterPlayer(zawiercie, player, position: PlayerPosition.Libero, memberFrom: new DateOnly(2024, 1, 1)));

            var inSeason = await Handle(new GetPlayersQuery(SeasonId: season.Id));
            var inLublin = await Handle(new GetPlayersQuery(RosterId: lublin.Id));

            inSeason.Items.ShouldHaveSingleItem().Roster.ShouldNotBeNull().Name.ShouldBe("Zawiercie");
            inLublin.Items.ShouldHaveSingleItem().Roster.ShouldNotBeNull().Name.ShouldBe("Lublin");
        }

        [Fact]
        public async Task HandleAsync_Should_TakeThePositionOfTheLatestMemberFrom_When_ThePlayerMovedWithinTheSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublinClub = NameSeeds.Club(_men, "Lublin");
            var zawiercieClub = NameSeeds.Club(_men, "Zawiercie");
            var lublin = NameSeeds.Roster(_men, season, lublinClub, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, zawiercieClub, "Zawiercie");
            var player = NameSeeds.Player("Transferred Player");
            await SeedAsync(
                _men,
                season,
                lublinClub,
                zawiercieClub,
                lublin,
                zawiercie,
                player,
                NameSeeds.RosterPlayer(lublin, player, position: PlayerPosition.OutsideHitter, memberFrom: new DateOnly(2023, 9, 1)),
                NameSeeds.RosterPlayer(zawiercie, player, position: PlayerPosition.Libero, memberFrom: new DateOnly(2024, 1, 1)));

            var page = await Handle(new GetPlayersQuery(SeasonId: season.Id));

            page.Items.ShouldHaveSingleItem().Position.ShouldBe("libero");
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByPosition_OnTheSameEntryThePositionIsReturnedFrom()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var olderRoster = NameSeeds.Roster(_men, older, club, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, club, "Lublin");
            var player = NameSeeds.Player("Changed Position");
            await SeedAsync(
                _men,
                older,
                newer,
                club,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(newerRoster, player, position: PlayerPosition.OppositeHitter));

            var setterInOlder = await Handle(new GetPlayersQuery(SeasonId: older.Id, Position: "setter"));
            var setterInNewer = await Handle(new GetPlayersQuery(SeasonId: newer.Id, Position: "setter"));
            var setterNow = await Handle(new GetPlayersQuery(Position: "setter"));
            var oppositeNow = await Handle(new GetPlayersQuery(Position: "opposite_hitter"));

            setterInOlder.Items.ShouldHaveSingleItem().Id.ShouldBe(player.Id);
            setterInNewer.Items.ShouldBeEmpty();
            setterNow.Items.ShouldBeEmpty();
            oppositeNow.Items.ShouldHaveSingleItem().Id.ShouldBe(player.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_NotMatchAPositionFromAnotherSeason_When_TheSeasonIsSelected()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var olderRoster = NameSeeds.Roster(_men, older, club, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, club, "Lublin");
            var player = NameSeeds.Player("Setter Only In Older Season");
            await SeedAsync(
                _men,
                older,
                newer,
                club,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(newerRoster, player));

            var page = await Handle(new GetPlayersQuery(SeasonId: newer.Id, Position: "setter"));

            page.Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task HandleAsync_Should_CombineTheFiltersWithAnd()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var match = NameSeeds.Player("Jan Setter");
            var wrongPosition = NameSeeds.Player("Jan Libero");
            var wrongName = NameSeeds.Player("Piotr Setter");
            await SeedAsync(
                _men,
                season,
                club,
                roster,
                match,
                wrongPosition,
                wrongName,
                NameSeeds.RosterPlayer(roster, match, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(roster, wrongPosition, position: PlayerPosition.Libero),
                NameSeeds.RosterPlayer(roster, wrongName, position: PlayerPosition.Setter));

            var page = await Handle(new GetPlayersQuery(
                Gender: "men",
                Position: "setter",
                RosterId: roster.Id,
                SeasonId: season.Id,
                LeagueId: _men.Id,
                Search: "jan"));

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(match.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_KeepAnUnresolvedPlayerWithoutExternalId()
        {
            await SeedAsync(NameSeeds.Player("Unresolved Player"));

            var page = await Handle(new GetPlayersQuery());

            page.Items.ShouldHaveSingleItem().FullName.ShouldBe("Unresolved Player");
        }

        private Task<PagedResponse<PlayerResponse>> Handle(GetPlayersQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetPlayersQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
