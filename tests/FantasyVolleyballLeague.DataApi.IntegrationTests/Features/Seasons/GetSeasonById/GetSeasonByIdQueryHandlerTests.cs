using FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasonById;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Seasons.GetSeasonById
{
    public sealed class GetSeasonByIdQueryHandlerTests : IntegrationTestBase
    {
        private const int ExternalId = 53;

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _women = new League("Women's league", 1, LeagueCountry.Poland.Id, LeagueGender.Women.Id);

        public GetSeasonByIdQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheSeasonDoesNotExist()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2023));

            var season = await Handle(new GetSeasonByIdQuery(EntityId.New()));

            season.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheSeasonFields()
        {
            var seasonEntity = new Season(2023, 2024, _men.Id, "PlusLiga", ExternalId);
            await SeedAsync(_men, seasonEntity);

            var season = (await Handle(new GetSeasonByIdQuery(seasonEntity.Id))).ShouldNotBeNull();

            season.ShouldSatisfyAllConditions(
                () => season.Id.ShouldBe(seasonEntity.Id),
                () => season.ExternalId.ShouldBe(ExternalId),
                () => season.Name.ShouldBe("2023-2024"),
                () => season.StartYear.ShouldBe(2023),
                () => season.EndYear.ShouldBe(2024),
                () => season.LeagueName.ShouldBe("PlusLiga"),
                () => season.League.Name.ShouldBe("Men's league"));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheLeagueSummaryWithCodes()
        {
            var seasonEntity = SeasonSeeds.Season(_women, 2023);
            await SeedAsync(_men, _women, seasonEntity);

            var season = (await Handle(new GetSeasonByIdQuery(seasonEntity.Id))).ShouldNotBeNull();

            season.League.ShouldSatisfyAllConditions(
                () => season.League.Id.ShouldBe(_women.Id),
                () => season.League.Name.ShouldBe("Women's league"),
                () => season.League.DivisionNumber.ShouldBe((short)1),
                () => season.League.Country.ShouldBe("pl"),
                () => season.League.Gender.ShouldBe("women"));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnThePreviousAndNextSeasonOfTheLeague()
        {
            var middle = SeasonSeeds.Season(_men, 2022);
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2020),
                SeasonSeeds.Season(_men, 2021),
                middle,
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_men, 2024),
                SeasonSeeds.Season(_women, 2022));

            var season = (await Handle(new GetSeasonByIdQuery(middle.Id))).ShouldNotBeNull();

            season.PreviousSeason.ShouldNotBeNull().StartYear.ShouldBe(2021);
            season.NextSeason.ShouldNotBeNull().StartYear.ShouldBe(2023);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNoPreviousSeason_When_ItIsTheFirstSeasonOfTheLeague()
        {
            var first = SeasonSeeds.Season(_men, 2022);
            await SeedAsync(_men, _women, first, SeasonSeeds.Season(_men, 2023), SeasonSeeds.Season(_women, 2020));

            var season = (await Handle(new GetSeasonByIdQuery(first.Id))).ShouldNotBeNull();

            season.PreviousSeason.ShouldBeNull();
            season.NextSeason.ShouldNotBeNull().StartYear.ShouldBe(2023);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNoNextSeasonAndMarkItCurrent_When_ItIsTheLatestSeasonOfTheLeague()
        {
            var latest = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, _women, SeasonSeeds.Season(_men, 2022), latest, SeasonSeeds.Season(_women, 2030));

            var season = (await Handle(new GetSeasonByIdQuery(latest.Id))).ShouldNotBeNull();

            season.NextSeason.ShouldBeNull();
            season.IsCurrent.ShouldBeTrue();
        }

        [Fact]
        public async Task HandleAsync_Should_NotMarkAnEarlierSeasonCurrent()
        {
            var earlier = SeasonSeeds.Season(_men, 2022);
            await SeedAsync(_men, earlier, SeasonSeeds.Season(_men, 2023));

            var season = (await Handle(new GetSeasonByIdQuery(earlier.Id))).ShouldNotBeNull();

            season.IsCurrent.ShouldBeFalse();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNoPhases_When_TheSeasonHasNone()
        {
            var seasonEntity = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, seasonEntity);

            var season = (await Handle(new GetSeasonByIdQuery(seasonEntity.Id))).ShouldNotBeNull();

            season.Phases.ShouldBeEmpty();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnThePhasesInOrderWithTheirTypeCodes()
        {
            var seasonEntity = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                seasonEntity,
                SeasonSeeds.Phase(seasonEntity, PhaseType.Relegation, order: 3),
                SeasonSeeds.Phase(seasonEntity, PhaseType.RegularSeason, order: 1),
                SeasonSeeds.Phase(seasonEntity, PhaseType.Playoff, order: 2));

            var season = (await Handle(new GetSeasonByIdQuery(seasonEntity.Id))).ShouldNotBeNull();

            season.Phases.Select(phase => (phase.Name, phase.Type, phase.Order)).ShouldBe(
            [
                ("Regular Season", "regular_season", 1),
                ("Playoff", "playoff", 2),
                ("Relegation", "relegation", 3),
            ]);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRoundsOfEachPhaseInOrder()
        {
            var seasonEntity = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(seasonEntity, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(seasonEntity, PhaseType.Playoff, order: 2);
            await SeedAsync(
                _men,
                seasonEntity,
                regular,
                playoff,
                SeasonSeeds.Round(regular, "Kolejka 2", order: 2),
                SeasonSeeds.Round(regular, "Kolejka 1", order: 1),
                SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "II Półfinał", stageOrder: 2),
                SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1));

            var season = (await Handle(new GetSeasonByIdQuery(seasonEntity.Id))).ShouldNotBeNull();

            season.Phases[0].Rounds.Select(round => round.Name).ShouldBe(["Kolejka 1", "Kolejka 2"]);
            season.Phases[1].Rounds.Select(round => round.Stage!.Name).ShouldBe(["I Ćwierćfinał", "II Półfinał"]);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheStageOfARound_OnlyWhenItHasOne()
        {
            var seasonEntity = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(seasonEntity, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(seasonEntity, PhaseType.Playoff, order: 2);
            await SeedAsync(
                _men,
                seasonEntity,
                regular,
                playoff,
                SeasonSeeds.Round(regular, "Kolejka 1", order: 1),
                SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1));

            var season = (await Handle(new GetSeasonByIdQuery(seasonEntity.Id))).ShouldNotBeNull();

            season.Phases[0].Rounds.ShouldHaveSingleItem().Stage.ShouldBeNull();
            var stage = season.Phases[1].Rounds.ShouldHaveSingleItem().Stage.ShouldNotBeNull();
            stage.Name.ShouldBe("I Ćwierćfinał");
            stage.Order.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_NotReturnThePhasesOrRoundsOfOtherSeasons()
        {
            var requested = SeasonSeeds.Season(_men, 2023);
            var other = SeasonSeeds.Season(_men, 2022);
            var requestedPhase = SeasonSeeds.Phase(requested, PhaseType.RegularSeason, order: 1);
            var otherPhase = SeasonSeeds.Phase(other, PhaseType.RegularSeason, order: 1);
            await SeedAsync(
                _men,
                requested,
                other,
                requestedPhase,
                otherPhase,
                SeasonSeeds.Round(requestedPhase, "Kolejka 1", order: 1),
                SeasonSeeds.Round(otherPhase, "Kolejka 1", order: 1),
                SeasonSeeds.Round(otherPhase, "Kolejka 2", order: 2));

            var season = (await Handle(new GetSeasonByIdQuery(requested.Id))).ShouldNotBeNull();

            var phase = season.Phases.ShouldHaveSingleItem();
            phase.Id.ShouldBe(requestedPhase.Id);
            phase.Rounds.ShouldHaveSingleItem();
        }

        private Task<GetSeasonByIdResponse?> Handle(GetSeasonByIdQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetSeasonByIdQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
