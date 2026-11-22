using FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosterById;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rosters.GetRosterById
{
    public sealed class GetRosterByIdQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetRosterByIdQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheRosterDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season, NameSeeds.Roster(_men, season, "Lublin"));

            var result = await Handle(EntityId.New());

            result.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_MapTheRosterWithItsLeagueAndSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin", externalId: 56);
            await SeedAsync(_men, season, roster);

            var result = await Handle(roster.Id);

            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(roster.Id),
                () => result.ExternalId.ShouldBe(56),
                () => result.Name.ShouldBe("Lublin"),
                () => result.League.Id.ShouldBe(_men.Id),
                () => result.League.Name.ShouldBe("Men's league"),
                () => result.League.DivisionNumber.ShouldBe((short)1),
                () => result.League.Country.ShouldBe("pl"),
                () => result.League.Gender.ShouldBe("men"),
                () => result.Season.Id.ShouldBe(season.Id),
                () => result.Season.Name.ShouldBe("2023-2024"),
                () => result.Season.StartYear.ShouldBe(2023),
                () => result.Season.EndYear.ShouldBe(2024));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPlayerList_When_TheRosterHasNoPlayers()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            await SeedAsync(_men, season, roster);

            var result = await Handle(roster.Id);

            result.ShouldNotBeNull().Players.ShouldBeEmpty();
            result.ExternalId.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_MapThePlayerWithTheProfileShirtPositionAndMembership()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var player = NameSeeds.Player(
                "Łukasz Kaczmarek",
                dateOfBirth: new DateOnly(1994, 12, 2),
                heightCm: 204,
                weightKg: 101,
                attackReachCm: 352);
            await SeedAsync(
                _men,
                season,
                roster,
                player,
                NameSeeds.RosterPlayer(
                    roster,
                    player,
                    shirtNumber: 9,
                    position: PlayerPosition.OppositeHitter,
                    memberFrom: new DateOnly(2023, 9, 1),
                    memberTo: new DateOnly(2024, 4, 30)));

            var result = await Handle(roster.Id);

            var item = result.ShouldNotBeNull().Players.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.PlayerId.ShouldBe(player.Id),
                () => item.FullName.ShouldBe("Łukasz Kaczmarek"),
                () => item.DateOfBirth.ShouldBe(new DateOnly(1994, 12, 2)),
                () => item.HeightCm.ShouldBe(204),
                () => item.WeightKg.ShouldBe(101),
                () => item.AttackReachCm.ShouldBe(352),
                () => item.ShirtNumber.ShouldBe(9),
                () => item.Position.ShouldBe("opposite_hitter"),
                () => item.MemberFrom.ShouldBe(new DateOnly(2023, 9, 1)),
                () => item.MemberTo.ShouldBe(new DateOnly(2024, 4, 30)));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullProfilePositionAndDates_When_TheyAreUnknown()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(_men, season, roster, player, NameSeeds.RosterPlayer(roster, player));

            var result = await Handle(roster.Id);

            var item = result.ShouldNotBeNull().Players.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.DateOfBirth.ShouldBeNull(),
                () => item.HeightCm.ShouldBeNull(),
                () => item.WeightKg.ShouldBeNull(),
                () => item.AttackReachCm.ShouldBeNull(),
                () => item.ShirtNumber.ShouldBeNull(),
                () => item.Position.ShouldBeNull(),
                () => item.MemberFrom.ShouldBeNull(),
                () => item.MemberTo.ShouldBeNull());
        }

        [Fact]
        public async Task HandleAsync_Should_SortThePlayersByShirtNumber_ThenWithoutOneByName_WithThePolishCollation()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var number10 = NameSeeds.Player("Adam Nowak");
            var number2 = NameSeeds.Player("Zbigniew Zieliński");
            var withoutNumberZ = NameSeeds.Player("Zenon Zalewski");
            var withoutNumberL = NameSeeds.Player("Łukasz Kowalski");
            var withoutNumberM = NameSeeds.Player("Marek Lis");
            await SeedAsync(
                _men,
                season,
                roster,
                number10,
                number2,
                withoutNumberZ,
                withoutNumberL,
                withoutNumberM,
                NameSeeds.RosterPlayer(roster, number10, shirtNumber: 10),
                NameSeeds.RosterPlayer(roster, number2, shirtNumber: 2),
                NameSeeds.RosterPlayer(roster, withoutNumberZ),
                NameSeeds.RosterPlayer(roster, withoutNumberL),
                NameSeeds.RosterPlayer(roster, withoutNumberM));

            var result = await Handle(roster.Id);

            result.ShouldNotBeNull().Players.Select(player => player.FullName).ShouldBe(
                ["Zbigniew Zieliński", "Adam Nowak", "Łukasz Kowalski", "Marek Lis", "Zenon Zalewski"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortPolishLettersAfterTheirBaseLetter_WhenTheDefaultCollationWouldNot()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var withAccent = NameSeeds.Player("Łazarz Nowak");
            var withoutAccent = NameSeeds.Player("Leon Nowak");
            var accentS = NameSeeds.Player("Śliwka Adam");
            var plainS = NameSeeds.Player("Sobolewski Adam");
            await SeedAsync(
                _men,
                season,
                roster,
                withAccent,
                withoutAccent,
                accentS,
                plainS,
                NameSeeds.RosterPlayer(roster, withAccent),
                NameSeeds.RosterPlayer(roster, withoutAccent),
                NameSeeds.RosterPlayer(roster, accentS),
                NameSeeds.RosterPlayer(roster, plainS));

            var result = await Handle(roster.Id);

            result.ShouldNotBeNull().Players.Select(player => player.FullName).ShouldBe(
                ["Leon Nowak", "Łazarz Nowak", "Sobolewski Adam", "Śliwka Adam"]);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnOnlyThePlayersOfTheRoster()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var otherRoster = NameSeeds.Roster(_men, season, "Zawiercie");
            var own = NameSeeds.Player("Own Player");
            var other = NameSeeds.Player("Other Player");
            await SeedAsync(
                _men,
                season,
                roster,
                otherRoster,
                own,
                other,
                NameSeeds.RosterPlayer(roster, own, shirtNumber: 1),
                NameSeeds.RosterPlayer(otherRoster, other, shirtNumber: 1));

            var result = await Handle(roster.Id);

            result.ShouldNotBeNull().Players.ShouldHaveSingleItem().PlayerId.ShouldBe(own.Id);
        }

        private Task<GetRosterByIdResponse?> Handle(Guid id)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetRosterByIdQueryHandler(dbContext).HandleAsync(new GetRosterByIdQuery(id), TestContext.Current.CancellationToken));
        }
    }
}
