using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Seasons;
using FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasons;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Seasons.GetSeasons
{
    public sealed class GetSeasonsQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _women = new League("Women's league", 1, LeagueCountry.Poland.Id, LeagueGender.Women.Id);

        public GetSeasonsQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_ThereAreNoSeasons()
        {
            var page = await Handle(new GetSeasonsQuery());

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByStartYearDescendingByDefault()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2021), SeasonSeeds.Season(_men, 2023), SeasonSeeds.Season(_men, 2022));

            var page = await Handle(new GetSeasonsQuery());

            page.Items.Select(season => season.StartYear).ShouldBe([2023, 2022, 2021]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByStartYearAscending()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2021), SeasonSeeds.Season(_men, 2023), SeasonSeeds.Season(_men, 2022));

            var page = await Handle(new GetSeasonsQuery(Sort: "startYear"));

            page.Items.Select(season => season.StartYear).ShouldBe([2021, 2022, 2023]);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByLeague()
        {
            await SeedAsync(_men, _women, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_women, 2023));

            var page = await Handle(new GetSeasonsQuery(LeagueId: _women.Id));

            page.Items.ShouldHaveSingleItem().LeagueId.ShouldBe(_women.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheLeagueDoesNotExist()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022));

            var page = await Handle(new GetSeasonsQuery(LeagueId: Guid.Parse("00000000-0000-0000-0000-000000000001")));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByStartYear()
        {
            await SeedAsync(_men, _women, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2023), SeasonSeeds.Season(_women, 2022));

            var page = await Handle(new GetSeasonsQuery(StartYear: 2022));

            page.Items.Select(season => season.LeagueId).ShouldBe([_men.Id, _women.Id], ignoreOrder: true);
            page.Items.ShouldAllBe(season => season.StartYear == 2022);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheLatestSeasonOfEachLeague_When_CurrentIsTrue()
        {
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2022),
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_women, 2020),
                SeasonSeeds.Season(_women, 2021));

            var page = await Handle(new GetSeasonsQuery(Current: true));

            page.Items.Select(season => (season.LeagueId, season.StartYear))
                .ShouldBe([(_men.Id, 2023), (_women.Id, 2021)], ignoreOrder: true);
            page.TotalCount.ShouldBe(2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public async Task HandleAsync_Should_NotFilterByCurrent_When_CurrentIsNotTrue(bool? current)
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2023));

            var page = await Handle(new GetSeasonsQuery(Current: current));

            page.Items.Count.ShouldBe(2);
        }

        [Fact]
        public async Task HandleAsync_Should_MarkOnlyTheLatestSeasonOfEachLeagueAsCurrent()
        {
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2022),
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_women, 2021));

            var page = await Handle(new GetSeasonsQuery());

            page.Items.Where(season => season.IsCurrent).Select(season => (season.LeagueId, season.StartYear))
                .ShouldBe([(_men.Id, 2023), (_women.Id, 2021)], ignoreOrder: true);
            page.Items.Count(season => !season.IsCurrent).ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_CombineFilters()
        {
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2022),
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_women, 2023));

            var page = await Handle(new GetSeasonsQuery(LeagueId: _men.Id, StartYear: 2023, Current: true));

            var season = page.Items.ShouldHaveSingleItem();
            season.LeagueId.ShouldBe(_men.Id);
            season.StartYear.ShouldBe(2023);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNothing_When_CurrentAndStartYearDoNotMatch()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2023));

            var page = await Handle(new GetSeasonsQuery(StartYear: 2022, Current: true));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRequestedPageAndTheTotalCount()
        {
            await SeedAsync(_men);
            await SeedAsync(SeasonSeeds.CreateSeasons(_men, count: 5, firstStartYear: 2000));

            var page = await Handle(new GetSeasonsQuery(Sort: "startYear", Offset: 2, Limit: 2));

            page.Items.Select(season => season.StartYear).ShouldBe([2002, 2003]);
            page.TotalCount.ShouldBe(5);
            page.Offset.ShouldBe(2);
            page.Limit.ShouldBe(2);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheSeasonFields()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var item = (await Handle(new GetSeasonsQuery())).Items.ShouldHaveSingleItem();

            item.ShouldSatisfyAllConditions(
                () => item.Id.ShouldBe(season.Id),
                () => item.Name.ShouldBe("2023-2024"),
                () => item.StartYear.ShouldBe(2023),
                () => item.EndYear.ShouldBe(2024),
                () => item.LeagueId.ShouldBe(_men.Id),
                () => item.IsCurrent.ShouldBeTrue());
        }

        private Task<PagedResponse<SeasonResponse>> Handle(GetSeasonsQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetSeasonsQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
