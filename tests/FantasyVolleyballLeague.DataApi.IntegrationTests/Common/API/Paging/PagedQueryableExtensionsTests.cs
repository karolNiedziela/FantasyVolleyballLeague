using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Common.API.Paging
{
    public sealed class PagedQueryableExtensionsTests : IntegrationTestBase
    {
        public PagedQueryableExtensionsTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ToPagedResponseAsyncReturnsTheRequestedSliceInOrder()
        {
            await SeedAsync(LeagueSeeds.CreateLeagues(60));

            var page = await ExecuteDbContextAsync(dbContext => dbContext.Set<League>()
                .OrderBy(league => league.Name)
                .ThenBy(league => league.Id)
                .ToPagedResponseAsync(offset: 25, limit: 25, TestContext.Current.CancellationToken));

            page.Items.Select(league => league.Name).ShouldBe(
                Enumerable.Range(26, 25).Select(LeagueSeeds.LeagueName));
            page.Offset.ShouldBe(25);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(60);
        }

        [Theory]
        [InlineData(10, 0, 25, 10)]
        [InlineData(60, 0, 25, 60)]
        [InlineData(25, 0, 25, 25)]
        [InlineData(60, 25, 25, 60)]
        [InlineData(60, 500, 25, 60)]
        public async Task ToPagedResponseAsyncReturnsTheTotalCount(int leagueCount, int offset, int limit, int expectedTotalCount)
        {
            await SeedAsync(LeagueSeeds.CreateLeagues(leagueCount));

            var page = await ExecuteDbContextAsync(dbContext => dbContext.Set<League>()
                .OrderBy(league => league.Name)
                .ThenBy(league => league.Id)
                .ToPagedResponseAsync(offset, limit, TestContext.Current.CancellationToken));

            page.TotalCount.ShouldBe(expectedTotalCount);
        }

        [Fact]
        public async Task ToPagedResponseAsyncReturnsAnEmptyPageBeyondTheEnd()
        {
            await SeedAsync(LeagueSeeds.CreateLeagues(60));

            var page = await ExecuteDbContextAsync(dbContext => dbContext.Set<League>()
                .OrderBy(league => league.Name)
                .ThenBy(league => league.Id)
                .ToPagedResponseAsync(offset: 500, limit: 25, TestContext.Current.CancellationToken));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(60);
        }

        [Fact]
        public async Task OrderByNameFollowsPolishAlphabeticalOrder()
        {
            string[] names = ["Żory", "Suwałki", "Śląsk", "Łańcut", "Lublin"];
            await SeedAsync(names.Select((name, index) =>
                new League(name, (short)(index + 1), LeagueCountry.Poland.Id, LeagueGender.Men.Id)));

            var sortedNames = await ExecuteDbContextAsync(dbContext => dbContext.Set<League>()
                .OrderBy(league => league.Name)
                .Select(league => league.Name)
                .ToListAsync(TestContext.Current.CancellationToken));

            sortedNames.ShouldBe(["Lublin", "Łańcut", "Suwałki", "Śląsk", "Żory"]);
        }
    }
}
