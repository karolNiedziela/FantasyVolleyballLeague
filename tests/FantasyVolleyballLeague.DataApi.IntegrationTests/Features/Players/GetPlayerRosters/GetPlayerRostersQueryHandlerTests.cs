using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerRosters;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetPlayerRosters
{
    public sealed class GetPlayerRostersQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _secondDivision = new League("Men's second league", 2, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetPlayerRostersQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_ThePlayerDoesNotExist()
        {
            await SeedAsync(NameSeeds.Player("Jan Kowalski"));

            var page = await Handle(new GetPlayerRostersQuery(EntityId.New()));

            page.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_ThePlayerHasNoRosterPlayer()
        {
            var player = NameSeeds.Player("Without Roster");
            await SeedAsync(player);

            var page = await Handle(new GetPlayerRostersQuery(player.Id));

            page.ShouldNotBeNull().Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_MapTheRosterSeasonLeagueShirtNumberPositionAndMembership()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(
                _men,
                season,
                club,
                roster,
                player,
                NameSeeds.RosterPlayer(
                    roster,
                    player,
                    shirtNumber: 9,
                    position: PlayerPosition.OppositeHitter,
                    memberFrom: new DateOnly(2023, 9, 1),
                    memberTo: new DateOnly(2024, 4, 30)));

            var page = await Handle(new GetPlayerRostersQuery(player.Id));

            var item = page.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.Roster.Id.ShouldBe(roster.Id),
                () => item.Roster.Name.ShouldBe("Lublin"),
                () => item.Season.Id.ShouldBe(season.Id),
                () => item.Season.Name.ShouldBe("2023-2024"),
                () => item.Season.StartYear.ShouldBe(2023),
                () => item.Season.EndYear.ShouldBe(2024),
                () => item.League.Id.ShouldBe(_men.Id),
                () => item.League.Name.ShouldBe("Men's league"),
                () => item.League.DivisionNumber.ShouldBe((short)1),
                () => item.League.Country.ShouldBe("pl"),
                () => item.League.Gender.ShouldBe("men"),
                () => item.ShirtNumber.ShouldBe(9),
                () => item.Position.ShouldBe("opposite_hitter"),
                () => item.MemberFrom.ShouldBe(new DateOnly(2023, 9, 1)),
                () => item.MemberTo.ShouldBe(new DateOnly(2024, 4, 30)));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullShirtNumberPositionAndDates_When_TheyAreUnknown()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(_men, season, club, roster, player, NameSeeds.RosterPlayer(roster, player));

            var page = await Handle(new GetPlayerRostersQuery(player.Id));

            var item = page.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.ShirtNumber.ShouldBeNull(),
                () => item.Position.ShouldBeNull(),
                () => item.MemberFrom.ShouldBeNull(),
                () => item.MemberTo.ShouldBeNull());
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheLeagueOfEachRoster_When_ThePlayerPlayedInTwoDivisions()
        {
            var older = SeasonSeeds.Season(_secondDivision, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var lublinReserves = NameSeeds.Club(_secondDivision, "Lublin II");
            var lublin = NameSeeds.Club(_men, "Lublin");
            var olderRoster = NameSeeds.Roster(_secondDivision, older, lublinReserves, "Lublin II");
            var newerRoster = NameSeeds.Roster(_men, newer, lublin, "Lublin");
            var player = NameSeeds.Player("Promoted Player");
            await SeedAsync(
                _men,
                _secondDivision,
                older,
                newer,
                lublinReserves,
                lublin,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player),
                NameSeeds.RosterPlayer(newerRoster, player));

            var page = await Handle(new GetPlayerRostersQuery(player.Id));

            page.ShouldNotBeNull().Items.Select(item => (item.League.Id, item.League.DivisionNumber)).ShouldBe(
                [(_men.Id, (short)1), (_secondDivision.Id, (short)2)]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByNewestSeasonFirstByDefault()
        {
            var first = SeasonSeeds.Season(_men, 2021);
            var second = SeasonSeeds.Season(_men, 2022);
            var third = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var resovia = NameSeeds.Club(_men, "Resovia");
            var firstRoster = NameSeeds.Roster(_men, first, lublin, "Lublin");
            var secondRoster = NameSeeds.Roster(_men, second, zawiercie, "Zawiercie");
            var thirdRoster = NameSeeds.Roster(_men, third, resovia, "Resovia");
            var player = NameSeeds.Player("Veteran");
            await SeedAsync(
                _men,
                first,
                second,
                third,
                lublin,
                zawiercie,
                resovia,
                firstRoster,
                secondRoster,
                thirdRoster,
                player,
                NameSeeds.RosterPlayer(secondRoster, player),
                NameSeeds.RosterPlayer(thirdRoster, player),
                NameSeeds.RosterPlayer(firstRoster, player));

            var page = await Handle(new GetPlayerRostersQuery(player.Id));

            page.ShouldNotBeNull().Items.Select(item => item.Season.StartYear).ShouldBe([2023, 2022, 2021]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortOldestSeasonFirst_When_SortIsSeason()
        {
            var first = SeasonSeeds.Season(_men, 2021);
            var second = SeasonSeeds.Season(_men, 2022);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var firstRoster = NameSeeds.Roster(_men, first, lublin, "Lublin");
            var secondRoster = NameSeeds.Roster(_men, second, zawiercie, "Zawiercie");
            var player = NameSeeds.Player("Veteran");
            await SeedAsync(
                _men,
                first,
                second,
                lublin,
                zawiercie,
                firstRoster,
                secondRoster,
                player,
                NameSeeds.RosterPlayer(secondRoster, player),
                NameSeeds.RosterPlayer(firstRoster, player));

            var page = await Handle(new GetPlayerRostersQuery(player.Id, Sort: "season"));

            page.ShouldNotBeNull().Items.Select(item => item.Season.StartYear).ShouldBe([2021, 2022]);
        }

        [Fact]
        public async Task HandleAsync_Should_PutTheLaterMemberFromFirstInASeason_AndUndatedLast()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublinClub = NameSeeds.Club(_men, "Lublin");
            var zawiercieClub = NameSeeds.Club(_men, "Zawiercie");
            var resoviaClub = NameSeeds.Club(_men, "Resovia");
            var lublin = NameSeeds.Roster(_men, season, lublinClub, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, zawiercieClub, "Zawiercie");
            var resovia = NameSeeds.Roster(_men, season, resoviaClub, "Resovia");
            var player = NameSeeds.Player("Transferred Twice");
            await SeedAsync(
                _men,
                season,
                lublinClub,
                zawiercieClub,
                resoviaClub,
                lublin,
                zawiercie,
                resovia,
                player,
                NameSeeds.RosterPlayer(lublin, player, memberFrom: new DateOnly(2023, 9, 1)),
                NameSeeds.RosterPlayer(resovia, player),
                NameSeeds.RosterPlayer(zawiercie, player, memberFrom: new DateOnly(2024, 1, 1)));

            var page = await Handle(new GetPlayerRostersQuery(player.Id));

            page.ShouldNotBeNull().Items.Select(item => item.Roster.Name).ShouldBe(["Zawiercie", "Lublin", "Resovia"]);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnOnlyTheRosterPlayersOfThePlayer()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var otherClub = NameSeeds.Club(_men, "Zawiercie");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var otherRoster = NameSeeds.Roster(_men, season, otherClub, "Zawiercie");
            var player = NameSeeds.Player("Own Player");
            var other = NameSeeds.Player("Other Player");
            await SeedAsync(
                _men,
                season,
                club,
                otherClub,
                roster,
                otherRoster,
                player,
                other,
                NameSeeds.RosterPlayer(roster, player),
                NameSeeds.RosterPlayer(otherRoster, other));

            var page = await Handle(new GetPlayerRostersQuery(player.Id));

            page.ShouldNotBeNull().Items.ShouldHaveSingleItem().Roster.Id.ShouldBe(roster.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_PageTheCareerAndReportTheTotal()
        {
            var first = SeasonSeeds.Season(_men, 2021);
            var second = SeasonSeeds.Season(_men, 2022);
            var third = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var firstRoster = NameSeeds.Roster(_men, first, club, "Lublin");
            var secondRoster = NameSeeds.Roster(_men, second, club, "Lublin");
            var thirdRoster = NameSeeds.Roster(_men, third, club, "Lublin");
            var player = NameSeeds.Player("Veteran");
            await SeedAsync(
                _men,
                first,
                second,
                third,
                club,
                firstRoster,
                secondRoster,
                thirdRoster,
                player,
                NameSeeds.RosterPlayer(firstRoster, player),
                NameSeeds.RosterPlayer(secondRoster, player),
                NameSeeds.RosterPlayer(thirdRoster, player));

            var page = await Handle(new GetPlayerRostersQuery(player.Id, Offset: 1, Limit: 1));

            page.ShouldNotBeNull().Items.ShouldHaveSingleItem().Season.StartYear.ShouldBe(2022);
            page.TotalCount.ShouldBe(3);
        }

        private Task<PagedResponse<PlayerCareerResponse>?> Handle(GetPlayerRostersQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetPlayerRostersQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
