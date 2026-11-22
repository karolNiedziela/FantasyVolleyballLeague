using FantasyVolleyballLeague.DataApi.Features.Players.GetPlayerById;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetPlayerById
{
    public sealed class GetPlayerByIdQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetPlayerByIdQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_ThePlayerDoesNotExist()
        {
            await SeedAsync(NameSeeds.Player("Jan Kowalski"));

            var result = await Handle(EntityId.New());

            result.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_MapTheProfileAndTheExternalId()
        {
            var player = new Player(LeagueGender.Women.Id, "Anna Nowak", externalId: 123);
            player.UpdateProfile("Anna Nowak", new DateOnly(1999, 2, 3), 185, 70, 305);
            await SeedAsync(player);

            var result = await Handle(player.Id);

            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(player.Id),
                () => result.ExternalId.ShouldBe(123),
                () => result.FullName.ShouldBe("Anna Nowak"),
                () => result.Gender.ShouldBe("women"),
                () => result.DateOfBirth.ShouldBe(new DateOnly(1999, 2, 3)),
                () => result.HeightCm.ShouldBe(185),
                () => result.WeightKg.ShouldBe(70),
                () => result.AttackReachCm.ShouldBe(305));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullExternalIdAndProfileFields_When_TheyAreUnknown()
        {
            var player = NameSeeds.Player("Unresolved Player");
            await SeedAsync(player);

            var result = await Handle(player.Id);

            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.ExternalId.ShouldBeNull(),
                () => result.DateOfBirth.ShouldBeNull(),
                () => result.HeightCm.ShouldBeNull(),
                () => result.WeightKg.ShouldBeNull(),
                () => result.AttackReachCm.ShouldBeNull());
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullCurrentRoster_When_ThePlayerHasNoRosterPlayer()
        {
            var player = NameSeeds.Player("Without Roster");
            await SeedAsync(player);

            var result = await Handle(player.Id);

            result.ShouldNotBeNull().CurrentRoster.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_MapTheCurrentRosterWithItsSeasonShirtNumberAndPosition()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(
                _men,
                season,
                roster,
                player,
                NameSeeds.RosterPlayer(roster, player, shirtNumber: 9, position: PlayerPosition.OppositeHitter));

            var result = await Handle(player.Id);

            var current = result.ShouldNotBeNull().CurrentRoster.ShouldNotBeNull();
            current.ShouldSatisfyAllConditions(
                () => current.Roster.Id.ShouldBe(roster.Id),
                () => current.Roster.Name.ShouldBe("Lublin"),
                () => current.Season.Id.ShouldBe(season.Id),
                () => current.Season.Name.ShouldBe("2023-2024"),
                () => current.Season.StartYear.ShouldBe(2023),
                () => current.Season.EndYear.ShouldBe(2024),
                () => current.ShirtNumber.ShouldBe(9),
                () => current.Position.ShouldBe("opposite_hitter"));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullShirtNumberAndPosition_When_TheyAreUnknown()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(_men, season, roster, player, NameSeeds.RosterPlayer(roster, player));

            var result = await Handle(player.Id);

            var current = result.ShouldNotBeNull().CurrentRoster.ShouldNotBeNull();
            current.ShirtNumber.ShouldBeNull();
            current.Position.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_TakeTheCurrentRosterFromTheLatestSeason()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var olderRoster = NameSeeds.Roster(_men, older, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, "Zawiercie");
            var player = NameSeeds.Player("Moved Club");
            await SeedAsync(
                _men,
                older,
                newer,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(newerRoster, player, shirtNumber: 2, position: PlayerPosition.Libero),
                NameSeeds.RosterPlayer(olderRoster, player, shirtNumber: 7, position: PlayerPosition.Setter));

            var result = await Handle(player.Id);

            var current = result.ShouldNotBeNull().CurrentRoster.ShouldNotBeNull();
            current.Roster.Id.ShouldBe(newerRoster.Id);
            current.Season.StartYear.ShouldBe(2023);
            current.ShirtNumber.ShouldBe(2);
            current.Position.ShouldBe("libero");
        }

        [Fact]
        public async Task HandleAsync_Should_TakeTheCurrentRosterWithTheLatestMemberFrom_When_ThePlayerMovedWithinTheSeason()
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
                NameSeeds.RosterPlayer(zawiercie, player, shirtNumber: 5, memberFrom: new DateOnly(2024, 1, 1)),
                NameSeeds.RosterPlayer(lublin, player, shirtNumber: 9, memberFrom: new DateOnly(2023, 9, 1)));

            var result = await Handle(player.Id);

            var current = result.ShouldNotBeNull().CurrentRoster.ShouldNotBeNull();
            current.Roster.Id.ShouldBe(zawiercie.Id);
            current.ShirtNumber.ShouldBe(5);
        }

        [Fact]
        public async Task HandleAsync_Should_PreferADatedRosterPlayer_OverOneWithoutMemberFrom_InTheSameSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var undated = NameSeeds.Roster(_men, season, "Lublin");
            var dated = NameSeeds.Roster(_men, season, "Zawiercie");
            var player = NameSeeds.Player("Partly Dated");
            await SeedAsync(
                _men,
                season,
                undated,
                dated,
                player,
                NameSeeds.RosterPlayer(dated, player, memberFrom: new DateOnly(2023, 9, 1)),
                NameSeeds.RosterPlayer(undated, player));

            var result = await Handle(player.Id);

            result.ShouldNotBeNull().CurrentRoster.ShouldNotBeNull().Roster.Id.ShouldBe(dated.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullPosition_When_TheCurrentRosterPlayerHasNone_EvenIfAnEarlierOneDid()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var olderRoster = NameSeeds.Roster(_men, older, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, "Lublin");
            var player = NameSeeds.Player("Unknown Latest Position");
            await SeedAsync(
                _men,
                older,
                newer,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(newerRoster, player));

            var result = await Handle(player.Id);

            var current = result.ShouldNotBeNull().CurrentRoster.ShouldNotBeNull();
            current.Roster.Id.ShouldBe(newerRoster.Id);
            current.Position.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_NotTakeTheRosterOfAnotherPlayer()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var otherRoster = NameSeeds.Roster(_men, season, "Zawiercie");
            var player = NameSeeds.Player("Own Player");
            var other = NameSeeds.Player("Other Player");
            await SeedAsync(
                _men,
                season,
                roster,
                otherRoster,
                player,
                other,
                NameSeeds.RosterPlayer(roster, player),
                NameSeeds.RosterPlayer(otherRoster, other));

            var result = await Handle(player.Id);

            result.ShouldNotBeNull().CurrentRoster.ShouldNotBeNull().Roster.Id.ShouldBe(roster.Id);
        }

        private Task<GetPlayerByIdResponse?> Handle(Guid id)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetPlayerByIdQueryHandler(dbContext).HandleAsync(new GetPlayerByIdQuery(id), TestContext.Current.CancellationToken));
        }
    }
}
