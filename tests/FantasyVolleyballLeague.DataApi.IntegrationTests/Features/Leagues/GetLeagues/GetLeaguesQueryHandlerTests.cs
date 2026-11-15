using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Leagues.GetLeagues
{
    public sealed class GetLeaguesQueryHandlerTests : IntegrationTestBase
    {
        public GetLeaguesQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_ThereAreNoLeagues()
        {
            var page = await Handle(new GetLeaguesQuery());

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByNameByDefault()
        {
            await SeedAsync(
                League("Gamma League", 1, LeagueGender.Women),
                League("Beta League", 1, LeagueGender.Men),
                League("Alpha League", 2, LeagueGender.Men));

            var page = await Handle(new GetLeaguesQuery());

            page.Items.Select(league => league.Name).ShouldBe(["Alpha League", "Beta League", "Gamma League"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortNamesInPolishOrder()
        {
            string[] names = ["Żory", "Suwałki", "Śląsk", "Łańcut", "Lublin"];
            await SeedAsync(names.Select((name, index) => League(name, (short)(index + 1), LeagueGender.Men)));

            var page = await Handle(new GetLeaguesQuery());

            page.Items.Select(league => league.Name).ShouldBe(["Lublin", "Łańcut", "Suwałki", "Śląsk", "Żory"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByDivisionNumberDescending()
        {
            await SeedAsync(
                League("First", 1, LeagueGender.Men),
                League("Second", 2, LeagueGender.Men),
                League("Third", 3, LeagueGender.Men));

            var page = await Handle(new GetLeaguesQuery(Sort: "-divisionNumber"));

            page.Items.Select(league => league.DivisionNumber).ShouldBe([(short)3, (short)2, (short)1]);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByGender()
        {
            await SeedAsync(
                League("Men's league", 1, LeagueGender.Men),
                League("Women's league", 1, LeagueGender.Women));

            var page = await Handle(new GetLeaguesQuery(Gender: LeagueGender.Women.Id));

            page.Items.Select(league => league.Name).ShouldBe(["Women's league"]);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByDivision()
        {
            await SeedAsync(
                League("Top league", 1, LeagueGender.Men),
                League("Second league", 2, LeagueGender.Men),
                League("Women's second league", 2, LeagueGender.Women));

            var page = await Handle(new GetLeaguesQuery(Division: 2, Sort: "name"));

            page.Items.Select(league => league.Name).ShouldBe(["Second league", "Women's second league"]);
        }

        [Fact]
        public async Task HandleAsync_Should_CombineFilters()
        {
            await SeedAsync(
                League("Men's top", 1, LeagueGender.Men),
                League("Men's second", 2, LeagueGender.Men),
                League("Women's top", 1, LeagueGender.Women),
                League("Women's second", 2, LeagueGender.Women));

            var page = await Handle(new GetLeaguesQuery(Gender: LeagueGender.Women.Id, Division: 2));

            page.Items.Select(league => league.Name).ShouldBe(["Women's second"]);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRequestedPageAndTheTotalCount()
        {
            await SeedAsync(LeagueSeeds.CreateLeagues(5));

            var page = await Handle(new GetLeaguesQuery(Offset: 2, Limit: 2));

            page.Items.Select(league => league.Name).ShouldBe([LeagueSeeds.LeagueName(3), LeagueSeeds.LeagueName(4)]);
            page.TotalCount.ShouldBe(5);
            page.Offset.ShouldBe(2);
            page.Limit.ShouldBe(2);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPageAndZeroTotal_When_NothingMatches()
        {
            await SeedAsync(League("Men's league", 1, LeagueGender.Men));

            var page = await Handle(new GetLeaguesQuery(Gender: LeagueGender.Women.Id));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnCountryAndGenderAsIdAndName()
        {
            await SeedAsync(League("TAURON Liga", 1, LeagueGender.Men));

            var league = (await Handle(new GetLeaguesQuery())).Items.ShouldHaveSingleItem();

            league.Name.ShouldBe("TAURON Liga");
            league.DivisionNumber.ShouldBe((short)1);
            league.Country.Id.ShouldBe(LeagueCountry.Poland.Id);
            league.Country.Name.ShouldBe(LeagueCountry.Poland.Name);
            league.Gender.Id.ShouldBe(LeagueGender.Men.Id);
            league.Gender.Name.ShouldBe(LeagueGender.Men.Name);
        }

        private static League League(string name, short divisionNumber, LeagueGender gender)
        {
            return new League(name, divisionNumber, LeagueCountry.Poland.Id, gender.Id);
        }

        private Task<PagedResponse<GetLeaguesResponse>> Handle(GetLeaguesQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetLeaguesQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
