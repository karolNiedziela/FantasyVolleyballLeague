using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Rosters;
using FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosters;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rosters.GetRosters
{
    public sealed class GetRostersQueryHandlerTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _women = new League("Women's league", 1, LeagueCountry.Poland.Id, LeagueGender.Women.Id);

        public GetRostersQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_ThereAreNoRosters()
        {
            var page = await Handle(new GetRostersQuery());

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByNameAscendingByDefault_UsingThePolishCollation()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                season,
                NameSeeds.Roster(_men, season, "Zawiercie"),
                NameSeeds.Roster(_men, season, "Łańcut"),
                NameSeeds.Roster(_men, season, "Lublin"),
                NameSeeds.Roster(_men, season, "Śląsk"),
                NameSeeds.Roster(_men, season, "Suwałki"));

            var page = await Handle(new GetRostersQuery());

            page.Items.Select(roster => roster.Name).ShouldBe(["Lublin", "Łańcut", "Suwałki", "Śląsk", "Zawiercie"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByNameDescending()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                season,
                NameSeeds.Roster(_men, season, "Lublin"),
                NameSeeds.Roster(_men, season, "Łańcut"),
                NameSeeds.Roster(_men, season, "Zawiercie"));

            var page = await Handle(new GetRostersQuery(Sort: "-name"));

            page.Items.Select(roster => roster.Name).ShouldBe(["Zawiercie", "Łańcut", "Lublin"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByStartYearThenByName()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                older,
                newer,
                NameSeeds.Roster(_men, older, "Zawiercie"),
                NameSeeds.Roster(_men, newer, "Zawiercie"),
                NameSeeds.Roster(_men, older, "Lublin"),
                NameSeeds.Roster(_men, newer, "Lublin"));

            var page = await Handle(new GetRostersQuery(Sort: "-startYear,name"));

            page.Items.Select(roster => (roster.Season.Id, roster.Name)).ShouldBe(
            [
                (newer.Id, "Lublin"),
                (newer.Id, "Zawiercie"),
                (older.Id, "Lublin"),
                (older.Id, "Zawiercie")
            ]);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterByLeague()
        {
            var menSeason = SeasonSeeds.Season(_men, 2023);
            var womenSeason = SeasonSeeds.Season(_women, 2023);
            await SeedAsync(
                _men,
                _women,
                menSeason,
                womenSeason,
                NameSeeds.Roster(_men, menSeason, "Men's club"),
                NameSeeds.Roster(_women, womenSeason, "Women's club"));

            var page = await Handle(new GetRostersQuery(LeagueId: _women.Id));

            page.Items.ShouldHaveSingleItem().LeagueId.ShouldBe(_women.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterBySeason()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                older,
                newer,
                NameSeeds.Roster(_men, older, "Old club"),
                NameSeeds.Roster(_men, newer, "New club"));

            var page = await Handle(new GetRostersQuery(SeasonId: newer.Id));

            page.Items.ShouldHaveSingleItem().Name.ShouldBe("New club");
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheLeagueDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season, NameSeeds.Roster(_men, season, "Lublin"));

            var page = await Handle(new GetRostersQuery(LeagueId: EntityId.New()));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_FindRostersByTheWordsOfTheName_InAnyOrderIgnoringCaseAndDiacritics()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                season,
                NameSeeds.Roster(_men, season, "ASSECO RESOVIA Rzeszów"),
                NameSeeds.Roster(_men, season, "Jastrzębski Węgiel"),
                NameSeeds.Roster(_men, season, "KS Jastrzębski Węgiel S.A."));

            var byWords = await Handle(new GetRostersQuery(Search: "rzeszow RESOVIA"));
            var byDiacritics = await Handle(new GetRostersQuery(Search: "wegiel jastrzebski"));

            byWords.Items.ShouldHaveSingleItem().Name.ShouldBe("ASSECO RESOVIA Rzeszów");
            byDiacritics.Items.Select(roster => roster.Name).ShouldBe(["Jastrzębski Węgiel", "KS Jastrzębski Węgiel S.A."]);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterNothing_When_TheSearchHasNoLettersOrDigits()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(
                _men,
                season,
                NameSeeds.Roster(_men, season, "Lublin"),
                NameSeeds.Roster(_men, season, "Zawiercie"));

            var page = await Handle(new GetRostersQuery(Search: "%"));

            page.TotalCount.ShouldBe(2);
        }

        [Fact]
        public async Task HandleAsync_Should_CombineTheFiltersAndTheSearch()
        {
            var menSeason = SeasonSeeds.Season(_men, 2023);
            var womenSeason = SeasonSeeds.Season(_women, 2023);
            await SeedAsync(
                _men,
                _women,
                menSeason,
                womenSeason,
                NameSeeds.Roster(_men, menSeason, "Resovia"),
                NameSeeds.Roster(_women, womenSeason, "Resovia"),
                NameSeeds.Roster(_men, menSeason, "Lublin"));

            var page = await Handle(new GetRostersQuery(LeagueId: _men.Id, SeasonId: menSeason.Id, Search: "resovia"));

            var roster = page.Items.ShouldHaveSingleItem();
            roster.LeagueId.ShouldBe(_men.Id);
            roster.Name.ShouldBe("Resovia");
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

            var page = await Handle(new GetRostersQuery(Offset: 1, Limit: 1));

            page.Items.ShouldHaveSingleItem().Name.ShouldBe("Club 2");
            page.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task HandleAsync_Should_MapTheRosterToTheResponse()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            await SeedAsync(_men, season, roster);

            var page = await Handle(new GetRostersQuery());

            var item = page.Items.ShouldHaveSingleItem();
            item.ShouldSatisfyAllConditions(
                () => item.Id.ShouldBe(roster.Id),
                () => item.Name.ShouldBe("Lublin"),
                () => item.LeagueId.ShouldBe(_men.Id),
                () => item.Season.Id.ShouldBe(season.Id),
                () => item.Season.Name.ShouldBe("2023-2024"),
                () => item.Season.StartYear.ShouldBe(2023),
                () => item.Season.EndYear.ShouldBe(2024));
        }

        private Task<PagedResponse<RosterResponse>> Handle(GetRostersQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetRostersQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
