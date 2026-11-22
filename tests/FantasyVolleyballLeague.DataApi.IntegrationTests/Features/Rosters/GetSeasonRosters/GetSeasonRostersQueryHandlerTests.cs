using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Rosters;
using FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosters;
using FantasyVolleyballLeague.DataApi.Features.Rosters.GetSeasonRosters;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rosters.GetSeasonRosters
{
    public sealed class GetSeasonRostersQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetSeasonRostersQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheSeasonDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season, NameSeeds.Roster(_men, season, "Lublin"));

            var page = await Handle(new GetSeasonRostersQuery(EntityId.New()));

            page.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheSeasonHasNoRosters()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var page = await Handle(new GetSeasonRostersQuery(season.Id));

            page.ShouldNotBeNull().Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnOnlyTheRostersOfTheSeason_SortedByNameWithThePolishCollation()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            await SeedAsync(
                _men,
                season,
                otherSeason,
                NameSeeds.Roster(_men, season, "Zawiercie"),
                NameSeeds.Roster(_men, season, "Łańcut"),
                NameSeeds.Roster(_men, season, "Lublin"),
                NameSeeds.Roster(_men, otherSeason, "Other season club"));

            var page = await Handle(new GetSeasonRostersQuery(season.Id));

            page.ShouldNotBeNull().Items.Select(roster => roster.Name).ShouldBe(["Lublin", "Łańcut", "Zawiercie"]);
            page.TotalCount.ShouldBe(3);
            page.Items.ShouldAllBe(roster => roster.Season.Id == season.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByNameDescending()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                season,
                NameSeeds.Roster(_men, season, "Lublin"),
                NameSeeds.Roster(_men, season, "Zawiercie"));

            var page = await Handle(new GetSeasonRostersQuery(season.Id, Sort: "-name"));

            page.ShouldNotBeNull().Items.Select(roster => roster.Name).ShouldBe(["Zawiercie", "Lublin"]);
        }

        [Fact]
        public async Task HandleAsync_Should_FindRostersByTheWordsOfTheName()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            await SeedAsync(
                _men,
                season,
                otherSeason,
                NameSeeds.Roster(_men, season, "ASSECO RESOVIA Rzeszów"),
                NameSeeds.Roster(_men, season, "Jastrzębski Węgiel"),
                NameSeeds.Roster(_men, otherSeason, "ASSECO RESOVIA Rzeszów"));

            var page = await Handle(new GetSeasonRostersQuery(season.Id, Search: "rzeszow resovia"));

            var roster = page.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            roster.Name.ShouldBe("ASSECO RESOVIA Rzeszów");
            roster.Season.Id.ShouldBe(season.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRequestedPageAndTheTotalCount()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                season,
                NameSeeds.Roster(_men, season, "Club 1"),
                NameSeeds.Roster(_men, season, "Club 2"),
                NameSeeds.Roster(_men, season, "Club 3"));

            var page = await Handle(new GetSeasonRostersQuery(season.Id, Offset: 1, Limit: 1));

            page.ShouldNotBeNull().Items.ShouldHaveSingleItem().Name.ShouldBe("Club 2");
            page.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheSameItemsAsTheListFilteredBySeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            await SeedAsync(
                _men,
                season,
                otherSeason,
                NameSeeds.Roster(_men, season, "Resovia"),
                NameSeeds.Roster(_men, season, "Resovia Rzeszów II"),
                NameSeeds.Roster(_men, season, "Lublin"),
                NameSeeds.Roster(_men, otherSeason, "Resovia"));

            var alias = await Handle(new GetSeasonRostersQuery(season.Id, Search: "resovia", Sort: "-name", Offset: 0, Limit: 5));
            var list = await ExecuteDbContextAsync(dbContext => new GetRostersQueryHandler(dbContext).HandleAsync(
                new GetRostersQuery(SeasonId: season.Id, Search: "resovia", Sort: "-name", Offset: 0, Limit: 5),
                TestContext.Current.CancellationToken));

            alias.ShouldNotBeNull();
            alias.TotalCount.ShouldBe(list.TotalCount);
            alias.Items.Select(roster => roster.Id).ShouldBe(list.Items.Select(roster => roster.Id));
        }

        private Task<PagedResponse<RosterResponse>?> Handle(GetSeasonRostersQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetSeasonRostersQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
