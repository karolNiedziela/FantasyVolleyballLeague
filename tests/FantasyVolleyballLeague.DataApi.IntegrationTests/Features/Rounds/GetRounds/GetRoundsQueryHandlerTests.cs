using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Features.Rounds.GetRounds;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rounds.GetRounds
{
    public sealed class GetRoundsQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetRoundsQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheSeasonDoesNotExist()
        {
            var page = await Handle(new GetRoundsQuery(EntityId.New()));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheSeasonHasNoRounds()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season, SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1));

            var page = await Handle(new GetRoundsQuery(season.Id));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRoundsInSeasonOrder_NotInsertionOrder()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            await SeedAsync(
                _men,
                season,
                playoff,
                regular,
                SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "II Półfinał", stageOrder: 2),
                SeasonSeeds.Round(regular, "Kolejka 2", order: 2),
                SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1),
                SeasonSeeds.Round(regular, "Kolejka 1", order: 1));

            var page = await Handle(new GetRoundsQuery(season.Id));

            page.Items.Select(round => (round.Phase.Type, round.Name, StageName: round.Stage?.Name)).ShouldBe(
            [
                ("regular_season", "Kolejka 1", null),
                ("regular_season", "Kolejka 2", null),
                ("playoff", "Para 1", "I Ćwierćfinał"),
                ("playoff", "Para 1", "II Półfinał"),
            ]);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnOnlyTheRoundsOfTheSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var otherPhase = SeasonSeeds.Phase(otherSeason, PhaseType.RegularSeason, order: 1);
            var round = SeasonSeeds.Round(phase, "Kolejka 1", order: 1);
            await SeedAsync(_men, season, otherSeason, phase, otherPhase, round, SeasonSeeds.Round(otherPhase, "Kolejka 1", order: 1));

            var page = await Handle(new GetRoundsQuery(season.Id));

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(round.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByPhase()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var playoffRound = SeasonSeeds.Round(playoff, "Para 1", order: 1);
            await SeedAsync(_men, season, regular, playoff, SeasonSeeds.Round(regular, "Kolejka 1", order: 1), playoffRound);

            var page = await Handle(new GetRoundsQuery(season.Id, PhaseId: playoff.Id));

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(playoffRound.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_ThePhaseBelongsToAnotherSeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var otherPhase = SeasonSeeds.Phase(otherSeason, PhaseType.RegularSeason, order: 1);
            await SeedAsync(
                _men,
                season,
                otherSeason,
                phase,
                otherPhase,
                SeasonSeeds.Round(phase, "Kolejka 1", order: 1),
                SeasonSeeds.Round(otherPhase, "Kolejka 1", order: 1));

            var page = await Handle(new GetRoundsQuery(season.Id, PhaseId: otherPhase.Id));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRequestedPageAndTheTotalCount()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            await SeedAsync(_men, season, phase);
            await SeedAsync(Enumerable.Range(1, 5).Select(number => SeasonSeeds.Round(phase, $"Kolejka {number}", order: number)));

            var page = await Handle(new GetRoundsQuery(season.Id, Offset: 2, Limit: 2));

            page.Items.Select(round => round.Name).ShouldBe(["Kolejka 3", "Kolejka 4"]);
            page.TotalCount.ShouldBe(5);
            page.Offset.ShouldBe(2);
            page.Limit.ShouldBe(2);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRoundFieldsWithTheStageOnlyWhenItHasOne()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var regularRound = SeasonSeeds.Round(regular, "Kolejka 1", order: 1);
            var playoffRound = SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1);
            await SeedAsync(_men, season, regular, playoff, regularRound, playoffRound);

            var page = await Handle(new GetRoundsQuery(season.Id));

            var first = page.Items[0];
            first.ShouldSatisfyAllConditions(
                () => first.Id.ShouldBe(regularRound.Id),
                () => first.Order.ShouldBe(1),
                () => first.Stage.ShouldBeNull(),
                () => first.Phase.Id.ShouldBe(regular.Id),
                () => first.Phase.Name.ShouldBe("Regular Season"),
                () => first.Phase.Type.ShouldBe("regular_season"),
                () => first.Phase.Order.ShouldBe(1));
            var stage = page.Items[1].Stage.ShouldNotBeNull();
            stage.Name.ShouldBe("I Ćwierćfinał");
            stage.Order.ShouldBe(1);
        }

        private Task<PagedResponse<RoundSummaryResponse>> Handle(GetRoundsQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetRoundsQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
