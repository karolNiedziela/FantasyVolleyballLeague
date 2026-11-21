using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Seasons;
using FantasyVolleyballLeague.DataApi.Features.Seasons.GetLeagueSeasons;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Seasons.GetLeagueSeasons
{
    public sealed class GetLeagueSeasonsQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _women = new League("Women's league", 1, LeagueCountry.Poland.Id, LeagueGender.Women.Id);

        public GetLeagueSeasonsQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheLeagueDoesNotExist()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2023));

            var page = await Handle(new GetLeagueSeasonsQuery(EntityId.New()));

            page.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheLeagueHasNoSeasons()
        {
            await SeedAsync(_men, _women, SeasonSeeds.Season(_women, 2023));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id))).ShouldNotBeNull();

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnOnlyTheSeasonsOfTheLeague()
        {
            await SeedAsync(_men, _women, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_women, 2023));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id))).ShouldNotBeNull();

            var season = page.Items.ShouldHaveSingleItem();
            season.LeagueId.ShouldBe(_men.Id);
            season.StartYear.ShouldBe(2022);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByStartYearDescendingByDefault()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2021), SeasonSeeds.Season(_men, 2023), SeasonSeeds.Season(_men, 2022));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id))).ShouldNotBeNull();

            page.Items.Select(season => season.StartYear).ShouldBe([2023, 2022, 2021]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByStartYearAscending()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2021), SeasonSeeds.Season(_men, 2023), SeasonSeeds.Season(_men, 2022));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id, Sort: "startYear"))).ShouldNotBeNull();

            page.Items.Select(season => season.StartYear).ShouldBe([2021, 2022, 2023]);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByStartYear()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2023));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id, StartYear: 2022))).ShouldNotBeNull();

            page.Items.ShouldHaveSingleItem().StartYear.ShouldBe(2022);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheLatestSeasonOfTheLeague_When_CurrentIsTrue()
        {
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2022),
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_women, 2024));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id, Current: true))).ShouldNotBeNull();

            var season = page.Items.ShouldHaveSingleItem();
            season.StartYear.ShouldBe(2023);
            season.IsCurrent.ShouldBeTrue();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNothing_When_CurrentAndStartYearDoNotMatch()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2023));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id, StartYear: 2022, Current: true))).ShouldNotBeNull();

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRequestedPageAndCountOnlyTheSeasonsOfTheLeague()
        {
            await SeedAsync(_men, _women, SeasonSeeds.Season(_women, 2030));
            await SeedAsync(SeasonSeeds.CreateSeasons(_men, count: 5, firstStartYear: 2000));

            var page = (await Handle(new GetLeagueSeasonsQuery(_men.Id, Sort: "startYear", Offset: 2, Limit: 2))).ShouldNotBeNull();

            page.Items.Select(season => season.StartYear).ShouldBe([2002, 2003]);
            page.TotalCount.ShouldBe(5);
            page.Offset.ShouldBe(2);
            page.Limit.ShouldBe(2);
        }

        private Task<PagedResponse<SeasonResponse>?> Handle(GetLeagueSeasonsQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetLeagueSeasonsQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
