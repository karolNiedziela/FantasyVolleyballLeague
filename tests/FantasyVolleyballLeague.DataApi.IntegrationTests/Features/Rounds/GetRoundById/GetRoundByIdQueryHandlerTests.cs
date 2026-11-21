using FantasyVolleyballLeague.DataApi.Features.Rounds.GetRoundById;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rounds.GetRoundById
{
    public sealed class GetRoundByIdQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetRoundByIdQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheRoundDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            await SeedAsync(_men, season, phase, SeasonSeeds.Round(phase, "Kolejka 1", order: 1));

            var round = await Handle(EntityId.New());

            round.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRoundFieldsWithoutAStage_When_TheSourceHasNone()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var roundEntity = SeasonSeeds.Round(phase, "Kolejka 3", order: 3);
            await SeedAsync(_men, season, phase, roundEntity);

            var round = (await Handle(roundEntity.Id)).ShouldNotBeNull();

            round.ShouldSatisfyAllConditions(
                () => round.Id.ShouldBe(roundEntity.Id),
                () => round.Name.ShouldBe("Kolejka 3"),
                () => round.Order.ShouldBe(3),
                () => round.Stage.ShouldBeNull());
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheStage_When_TheRoundHasOne()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var roundEntity = SeasonSeeds.Round(phase, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1);
            await SeedAsync(_men, season, phase, roundEntity);

            var round = (await Handle(roundEntity.Id)).ShouldNotBeNull();

            var stage = round.Stage.ShouldNotBeNull();
            stage.Name.ShouldBe("I Ćwierćfinał");
            stage.Order.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnThePhaseWithItsTypeCode()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var roundEntity = SeasonSeeds.Round(phase, "Para 1", order: 1);
            await SeedAsync(_men, season, phase, roundEntity);

            var round = (await Handle(roundEntity.Id)).ShouldNotBeNull();

            round.Phase.ShouldSatisfyAllConditions(
                () => round.Phase.Id.ShouldBe(phase.Id),
                () => round.Phase.Name.ShouldBe("Playoff"),
                () => round.Phase.Type.ShouldBe("playoff"),
                () => round.Phase.Order.ShouldBe(2));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheSeasonSummary()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var roundEntity = SeasonSeeds.Round(phase, "Kolejka 1", order: 1);
            await SeedAsync(_men, season, phase, roundEntity);

            var round = (await Handle(roundEntity.Id)).ShouldNotBeNull();

            round.Season.ShouldSatisfyAllConditions(
                () => round.Season.Id.ShouldBe(season.Id),
                () => round.Season.Name.ShouldBe("2023-2024"),
                () => round.Season.StartYear.ShouldBe(2023),
                () => round.Season.EndYear.ShouldBe(2024));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNoPreviousRound_When_ItIsTheFirstRoundOfTheSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var first = SeasonSeeds.Round(phase, "Kolejka 1", order: 1);
            var second = SeasonSeeds.Round(phase, "Kolejka 2", order: 2);
            await SeedAsync(_men, season, phase, first, second);

            var round = (await Handle(first.Id)).ShouldNotBeNull();

            round.PreviousRound.ShouldBeNull();
            round.NextRound.ShouldNotBeNull().Id.ShouldBe(second.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNoNextRound_When_ItIsTheLastRoundOfTheSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var first = SeasonSeeds.Round(phase, "Kolejka 1", order: 1);
            var second = SeasonSeeds.Round(phase, "Kolejka 2", order: 2);
            await SeedAsync(_men, season, phase, first, second);

            var round = (await Handle(second.Id)).ShouldNotBeNull();

            round.NextRound.ShouldBeNull();
            round.PreviousRound.ShouldNotBeNull().Id.ShouldBe(first.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheNeighboursByRoundOrder_NotByInsertionOrder()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var first = SeasonSeeds.Round(phase, "Kolejka 1", order: 1);
            var second = SeasonSeeds.Round(phase, "Kolejka 2", order: 2);
            var third = SeasonSeeds.Round(phase, "Kolejka 3", order: 3);
            await SeedAsync(_men, season, phase, third, first, second);

            var round = (await Handle(second.Id)).ShouldNotBeNull();

            round.PreviousRound.ShouldNotBeNull().Id.ShouldBe(first.Id);
            round.NextRound.ShouldNotBeNull().Id.ShouldBe(third.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ContinueIntoTheNextPhase_FromTheLastRoundOfAPhase()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var lastRegular = SeasonSeeds.Round(regular, "Kolejka 26", order: 26);
            var firstPlayoff = SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1);
            await SeedAsync(_men, season, regular, playoff, lastRegular, firstPlayoff);

            var round = (await Handle(lastRegular.Id)).ShouldNotBeNull();

            var next = round.NextRound.ShouldNotBeNull();
            next.Id.ShouldBe(firstPlayoff.Id);
            next.Phase.Type.ShouldBe("playoff");
            next.Stage.ShouldNotBeNull().Name.ShouldBe("I Ćwierćfinał");
        }

        [Fact]
        public async Task HandleAsync_Should_ContinueIntoThePreviousPhase_FromTheFirstRoundOfAPhase()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var lastRegular = SeasonSeeds.Round(regular, "Kolejka 26", order: 26);
            var firstPlayoff = SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1);
            await SeedAsync(_men, season, regular, playoff, lastRegular, firstPlayoff);

            var round = (await Handle(firstPlayoff.Id)).ShouldNotBeNull();

            var previous = round.PreviousRound.ShouldNotBeNull();
            previous.Id.ShouldBe(lastRegular.Id);
            previous.Phase.Type.ShouldBe("regular_season");
            previous.Stage.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_FollowThePhaseOrder_NotTheInsertionOrder()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var regularRound = SeasonSeeds.Round(regular, "Kolejka 1", order: 1);
            var playoffRound = SeasonSeeds.Round(playoff, "Para 1", order: 1);
            await SeedAsync(_men, season, playoff, regular, playoffRound, regularRound);

            var round = (await Handle(regularRound.Id)).ShouldNotBeNull();

            round.PreviousRound.ShouldBeNull();
            round.NextRound.ShouldNotBeNull().Id.ShouldBe(playoffRound.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_FollowTheStageOrder_WithinAPhase()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var quarterfinal = SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1);
            var semifinal = SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "II Półfinał", stageOrder: 2);
            await SeedAsync(_men, season, playoff, semifinal, quarterfinal);

            var round = (await Handle(quarterfinal.Id)).ShouldNotBeNull();

            round.PreviousRound.ShouldBeNull();
            var next = round.NextRound.ShouldNotBeNull();
            next.Id.ShouldBe(semifinal.Id);
            next.Stage.ShouldNotBeNull().Name.ShouldBe("II Półfinał");
        }

        [Fact]
        public async Task HandleAsync_Should_NotUseTheRoundsOfAnotherSeasonAsNeighbours()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var otherPhase = SeasonSeeds.Phase(otherSeason, PhaseType.RegularSeason, order: 1);
            var only = SeasonSeeds.Round(phase, "Kolejka 1", order: 1);
            await SeedAsync(
                _men,
                season,
                otherSeason,
                phase,
                otherPhase,
                only,
                SeasonSeeds.Round(otherPhase, "Kolejka 0", order: 0),
                SeasonSeeds.Round(otherPhase, "Kolejka 2", order: 2));

            var round = (await Handle(only.Id)).ShouldNotBeNull();

            round.PreviousRound.ShouldBeNull();
            round.NextRound.ShouldBeNull();
        }

        private Task<GetRoundByIdResponse?> Handle(Guid id)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetRoundByIdQueryHandler(dbContext).HandleAsync(new GetRoundByIdQuery(id), TestContext.Current.CancellationToken));
        }
    }
}
