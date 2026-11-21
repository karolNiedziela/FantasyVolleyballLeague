using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Features.Rounds.GetSeasonRounds;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rounds.GetSeasonRounds
{
    public sealed class GetSeasonRoundsQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetSeasonRoundsQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheSeasonDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var page = await Handle(new GetSeasonRoundsQuery(EntityId.New()));

            page.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheSeasonHasNoRounds()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var page = (await Handle(new GetSeasonRoundsQuery(season.Id))).ShouldNotBeNull();

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
                SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1),
                SeasonSeeds.Round(regular, "Kolejka 2", order: 2),
                SeasonSeeds.Round(regular, "Kolejka 1", order: 1));

            var page = (await Handle(new GetSeasonRoundsQuery(season.Id))).ShouldNotBeNull();

            page.Items.Select(round => round.Name).ShouldBe(["Kolejka 1", "Kolejka 2", "Para 1"]);
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

            var page = (await Handle(new GetSeasonRoundsQuery(season.Id))).ShouldNotBeNull();

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

            var page = (await Handle(new GetSeasonRoundsQuery(season.Id, PhaseId: playoff.Id))).ShouldNotBeNull();

            page.Items.ShouldHaveSingleItem().Id.ShouldBe(playoffRound.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRequestedPageAndTheTotalCount()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            await SeedAsync(_men, season, phase);
            await SeedAsync(Enumerable.Range(1, 5).Select(number => SeasonSeeds.Round(phase, $"Kolejka {number}", order: number)));

            var page = (await Handle(new GetSeasonRoundsQuery(season.Id, Offset: 2, Limit: 2))).ShouldNotBeNull();

            page.Items.Select(round => round.Name).ShouldBe(["Kolejka 3", "Kolejka 4"]);
            page.TotalCount.ShouldBe(5);
        }

        private Task<PagedResponse<RoundSummaryResponse>?> Handle(GetSeasonRoundsQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetSeasonRoundsQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
