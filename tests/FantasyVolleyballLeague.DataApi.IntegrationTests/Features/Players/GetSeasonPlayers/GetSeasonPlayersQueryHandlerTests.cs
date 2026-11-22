using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Players;
using FantasyVolleyballLeague.DataApi.Features.Players.GetSeasonPlayers;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetSeasonPlayers
{
    public sealed class GetSeasonPlayersQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetSeasonPlayersQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheSeasonDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var page = await Handle(new GetSeasonPlayersQuery(EntityId.New()));

            page.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheSeasonHasNoPlayers()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season, NameSeeds.Player("Without Roster"));

            var page = await Handle(new GetSeasonPlayersQuery(season.Id));

            page.ShouldNotBeNull().Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnOnlyThePlayersOfTheSeason_SortedByNameWithThePolishCollation()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var otherRoster = NameSeeds.Roster(_men, otherSeason, "Lublin");
            var withAccent = NameSeeds.Player("Łazarz Nowak");
            var withoutAccent = NameSeeds.Player("Leon Nowak");
            var zalewski = NameSeeds.Player("Zenon Zalewski");
            var otherSeasonPlayer = NameSeeds.Player("Other Season Player");
            await SeedAsync(
                _men,
                season,
                otherSeason,
                roster,
                otherRoster,
                withAccent,
                withoutAccent,
                zalewski,
                otherSeasonPlayer,
                NameSeeds.RosterPlayer(roster, zalewski),
                NameSeeds.RosterPlayer(roster, withAccent),
                NameSeeds.RosterPlayer(roster, withoutAccent),
                NameSeeds.RosterPlayer(otherRoster, otherSeasonPlayer));

            var page = await Handle(new GetSeasonPlayersQuery(season.Id));

            page.ShouldNotBeNull().Items.Select(player => player.FullName).ShouldBe(
                ["Leon Nowak", "Łazarz Nowak", "Zenon Zalewski"]);
            page.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheSameShapeAsTheList_WithThePositionOfTheSeason()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var olderRoster = NameSeeds.Roster(_men, older, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, "Lublin");
            var player = NameSeeds.Player(
                "Changed Position",
                dateOfBirth: new DateOnly(1995, 3, 4),
                heightCm: 199,
                weightKg: 90,
                attackReachCm: 340);
            await SeedAsync(
                _men,
                older,
                newer,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(newerRoster, player, position: PlayerPosition.OppositeHitter));

            var inOlder = await Handle(new GetSeasonPlayersQuery(older.Id));
            var inNewer = await Handle(new GetSeasonPlayersQuery(newer.Id));

            var item = inOlder.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.Id.ShouldBe(player.Id),
                () => item.FullName.ShouldBe("Changed Position"),
                () => item.Gender.ShouldBe("men"),
                () => item.DateOfBirth.ShouldBe(new DateOnly(1995, 3, 4)),
                () => item.HeightCm.ShouldBe(199),
                () => item.WeightKg.ShouldBe(90),
                () => item.AttackReachCm.ShouldBe(340),
                () => item.Position.ShouldBe("setter"),
                () => item.Roster.ShouldNotBeNull().Id.ShouldBe(olderRoster.Id));
            var newerItem = inNewer.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            newerItem.Position.ShouldBe("opposite_hitter");
            newerItem.Roster.ShouldNotBeNull().Id.ShouldBe(newerRoster.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ListAPlayerWithSeveralRosterPlayersInTheSeasonOnce()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Roster(_men, season, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, "Zawiercie");
            var player = NameSeeds.Player("Transferred Player");
            await SeedAsync(
                _men,
                season,
                lublin,
                zawiercie,
                player,
                NameSeeds.RosterPlayer(lublin, player, position: PlayerPosition.OutsideHitter, memberFrom: new DateOnly(2023, 9, 1)),
                NameSeeds.RosterPlayer(zawiercie, player, position: PlayerPosition.Libero, memberFrom: new DateOnly(2024, 1, 1)));

            var page = await Handle(new GetSeasonPlayersQuery(season.Id));

            var item = page.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            item.Position.ShouldBe("libero");
            item.Roster.ShouldNotBeNull().ShouldSatisfyAllConditions(
                roster => roster.Id.ShouldBe(zawiercie.Id),
                roster => roster.Name.ShouldBe("Zawiercie"));
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullPosition_When_TheLatestRosterPlayerOfTheSeasonHasNone()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Roster(_men, season, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, "Zawiercie");
            var player = NameSeeds.Player("Transferred Player");
            await SeedAsync(
                _men,
                season,
                lublin,
                zawiercie,
                player,
                NameSeeds.RosterPlayer(lublin, player, position: PlayerPosition.OutsideHitter, memberFrom: new DateOnly(2023, 9, 1)),
                NameSeeds.RosterPlayer(zawiercie, player, memberFrom: new DateOnly(2024, 1, 1)));

            var page = await Handle(new GetSeasonPlayersQuery(season.Id));

            var item = page.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            item.Position.ShouldBeNull();
            item.Roster.ShouldNotBeNull().Id.ShouldBe(zawiercie.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByRoster_AndTakeThePositionFromThatRoster()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Roster(_men, season, "Lublin");
            var zawiercie = NameSeeds.Roster(_men, season, "Zawiercie");
            var transferred = NameSeeds.Player("Transferred Player");
            var stayed = NameSeeds.Player("Stayed In Zawiercie");
            await SeedAsync(
                _men,
                season,
                lublin,
                zawiercie,
                transferred,
                stayed,
                NameSeeds.RosterPlayer(lublin, transferred, position: PlayerPosition.OutsideHitter, memberFrom: new DateOnly(2023, 9, 1)),
                NameSeeds.RosterPlayer(zawiercie, transferred, position: PlayerPosition.Libero, memberFrom: new DateOnly(2024, 1, 1)),
                NameSeeds.RosterPlayer(zawiercie, stayed));

            var inLublin = await Handle(new GetSeasonPlayersQuery(season.Id, RosterId: lublin.Id));
            var inZawiercie = await Handle(new GetSeasonPlayersQuery(season.Id, RosterId: zawiercie.Id));

            inLublin.ShouldNotBeNull().Items.ShouldHaveSingleItem().Position.ShouldBe("outside_hitter");
            inZawiercie.ShouldNotBeNull().Items.Select(player => player.FullName).ShouldBe(
                ["Stayed In Zawiercie", "Transferred Player"]);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheRosterBelongsToAnotherSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var otherRoster = NameSeeds.Roster(_men, otherSeason, "Lublin");
            var player = NameSeeds.Player("Other Season Player");
            await SeedAsync(_men, season, otherSeason, otherRoster, player, NameSeeds.RosterPlayer(otherRoster, player));

            var page = await Handle(new GetSeasonPlayersQuery(season.Id, RosterId: otherRoster.Id));

            page.ShouldNotBeNull().Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByPosition_OnlyWithinTheSeason()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var olderRoster = NameSeeds.Roster(_men, older, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, "Lublin");
            var changed = NameSeeds.Player("Setter Only In Older Season");
            var setterNow = NameSeeds.Player("Setter In Newer Season");
            await SeedAsync(
                _men,
                older,
                newer,
                olderRoster,
                newerRoster,
                changed,
                setterNow,
                NameSeeds.RosterPlayer(olderRoster, changed, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(newerRoster, changed, position: PlayerPosition.OppositeHitter),
                NameSeeds.RosterPlayer(newerRoster, setterNow, position: PlayerPosition.Setter));

            var inOlder = await Handle(new GetSeasonPlayersQuery(older.Id, Position: "setter"));
            var inNewer = await Handle(new GetSeasonPlayersQuery(newer.Id, Position: "setter"));

            inOlder.ShouldNotBeNull().Items.ShouldHaveSingleItem().Id.ShouldBe(changed.Id);
            inNewer.ShouldNotBeNull().Items.ShouldHaveSingleItem().Id.ShouldBe(setterNow.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_SearchByWordsOfTheName_IgnoringOrderCaseAndDiacritics()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var match = NameSeeds.Player("Łukasz Kaczmarek");
            var other = NameSeeds.Player("Łukasz Kowalski");
            await SeedAsync(
                _men,
                season,
                roster,
                match,
                other,
                NameSeeds.RosterPlayer(roster, match),
                NameSeeds.RosterPlayer(roster, other));

            var page = await Handle(new GetSeasonPlayersQuery(season.Id, Search: "KACZMAREK lukasz"));

            page.ShouldNotBeNull().Items.ShouldHaveSingleItem().Id.ShouldBe(match.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_SortDescendingAndPage()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var a = NameSeeds.Player("Adam A");
            var b = NameSeeds.Player("Adam B");
            var c = NameSeeds.Player("Adam C");
            await SeedAsync(
                _men,
                season,
                roster,
                a,
                b,
                c,
                NameSeeds.RosterPlayer(roster, a),
                NameSeeds.RosterPlayer(roster, b),
                NameSeeds.RosterPlayer(roster, c));

            var page = await Handle(new GetSeasonPlayersQuery(season.Id, Sort: "-fullName", Offset: 1, Limit: 1));

            page.ShouldNotBeNull().Items.Select(player => player.FullName).ShouldBe(["Adam B"]);
            page.TotalCount.ShouldBe(3);
        }

        private Task<PagedResponse<PlayerResponse>?> Handle(GetSeasonPlayersQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetSeasonPlayersQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
