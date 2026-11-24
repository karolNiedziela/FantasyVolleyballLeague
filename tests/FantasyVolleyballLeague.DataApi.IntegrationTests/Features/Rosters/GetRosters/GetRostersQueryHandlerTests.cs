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
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var lancut = NameSeeds.Club(_men, "Łańcut");
            var lublin = NameSeeds.Club(_men, "Lublin");
            var slask = NameSeeds.Club(_men, "Śląsk");
            var suwalki = NameSeeds.Club(_men, "Suwałki");
            await SeedAsync(
                _men,
                season,
                zawiercie,
                lancut,
                lublin,
                slask,
                suwalki,
                NameSeeds.Roster(_men, season, zawiercie, "Zawiercie"),
                NameSeeds.Roster(_men, season, lancut, "Łańcut"),
                NameSeeds.Roster(_men, season, lublin, "Lublin"),
                NameSeeds.Roster(_men, season, slask, "Śląsk"),
                NameSeeds.Roster(_men, season, suwalki, "Suwałki"));

            var page = await Handle(new GetRostersQuery());

            page.Items.Select(roster => roster.Name).ShouldBe(["Lublin", "Łańcut", "Suwałki", "Śląsk", "Zawiercie"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByNameDescending()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var lancut = NameSeeds.Club(_men, "Łańcut");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            await SeedAsync(
                _men,
                season,
                lublin,
                lancut,
                zawiercie,
                NameSeeds.Roster(_men, season, lublin, "Lublin"),
                NameSeeds.Roster(_men, season, lancut, "Łańcut"),
                NameSeeds.Roster(_men, season, zawiercie, "Zawiercie"));

            var page = await Handle(new GetRostersQuery(Sort: "-name"));

            page.Items.Select(roster => roster.Name).ShouldBe(["Zawiercie", "Łańcut", "Lublin"]);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByStartYearThenByName()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var lublin = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(
                _men,
                older,
                newer,
                zawiercie,
                lublin,
                NameSeeds.Roster(_men, older, zawiercie, "Zawiercie"),
                NameSeeds.Roster(_men, newer, zawiercie, "Zawiercie"),
                NameSeeds.Roster(_men, older, lublin, "Lublin"),
                NameSeeds.Roster(_men, newer, lublin, "Lublin"));

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
            var menClub = NameSeeds.Club(_men, "Men's club");
            var womenClub = NameSeeds.Club(_women, "Women's club");
            await SeedAsync(
                _men,
                _women,
                menSeason,
                womenSeason,
                menClub,
                womenClub,
                NameSeeds.Roster(_men, menSeason, menClub, "Men's club"),
                NameSeeds.Roster(_women, womenSeason, womenClub, "Women's club"));

            var page = await Handle(new GetRostersQuery(LeagueId: _women.Id));

            page.Items.ShouldHaveSingleItem().LeagueId.ShouldBe(_women.Id);
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterBySeason()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var oldClub = NameSeeds.Club(_men, "Old club");
            var newClub = NameSeeds.Club(_men, "New club");
            await SeedAsync(
                _men,
                older,
                newer,
                oldClub,
                newClub,
                NameSeeds.Roster(_men, older, oldClub, "Old club"),
                NameSeeds.Roster(_men, newer, newClub, "New club"));

            var page = await Handle(new GetRostersQuery(SeasonId: newer.Id));

            page.Items.ShouldHaveSingleItem().Name.ShouldBe("New club");
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnAnEmptyPage_When_TheLeagueDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(_men, season, club, NameSeeds.Roster(_men, season, club, "Lublin"));

            var page = await Handle(new GetRostersQuery(LeagueId: EntityId.New()));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task HandleAsync_Should_FindRostersByTheWordsOfTheName_InAnyOrderIgnoringCaseAndDiacritics()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var resovia = NameSeeds.Club(_men, "ASSECO RESOVIA Rzeszów");
            var jastrzebski = NameSeeds.Club(_men, "Jastrzębski Węgiel");
            var ksJastrzebski = NameSeeds.Club(_men, "KS Jastrzębski Węgiel S.A.");
            await SeedAsync(
                _men,
                season,
                resovia,
                jastrzebski,
                ksJastrzebski,
                NameSeeds.Roster(_men, season, resovia, "ASSECO RESOVIA Rzeszów"),
                NameSeeds.Roster(_men, season, jastrzebski, "Jastrzębski Węgiel"),
                NameSeeds.Roster(_men, season, ksJastrzebski, "KS Jastrzębski Węgiel S.A."));

            var byWords = await Handle(new GetRostersQuery(Search: "rzeszow RESOVIA"));
            var byDiacritics = await Handle(new GetRostersQuery(Search: "wegiel jastrzebski"));

            byWords.Items.ShouldHaveSingleItem().Name.ShouldBe("ASSECO RESOVIA Rzeszów");
            byDiacritics.Items.Select(roster => roster.Name).ShouldBe(["Jastrzębski Węgiel", "KS Jastrzębski Węgiel S.A."]);
        }

        [Fact]
        public async Task HandleAsync_Should_FilterNothing_When_TheSearchHasNoLettersOrDigits()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            await SeedAsync(
                _men,
                season,
                lublin,
                zawiercie,
                NameSeeds.Roster(_men, season, lublin, "Lublin"),
                NameSeeds.Roster(_men, season, zawiercie, "Zawiercie"));

            var page = await Handle(new GetRostersQuery(Search: "%"));

            page.TotalCount.ShouldBe(2);
        }

        [Fact]
        public async Task HandleAsync_Should_CombineTheFiltersAndTheSearch()
        {
            var menSeason = SeasonSeeds.Season(_men, 2023);
            var womenSeason = SeasonSeeds.Season(_women, 2023);
            var menResovia = NameSeeds.Club(_men, "Resovia");
            var womenResovia = NameSeeds.Club(_women, "Resovia");
            var lublin = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(
                _men,
                _women,
                menSeason,
                womenSeason,
                menResovia,
                womenResovia,
                lublin,
                NameSeeds.Roster(_men, menSeason, menResovia, "Resovia"),
                NameSeeds.Roster(_women, womenSeason, womenResovia, "Resovia"),
                NameSeeds.Roster(_men, menSeason, lublin, "Lublin"));

            var page = await Handle(new GetRostersQuery(LeagueId: _men.Id, SeasonId: menSeason.Id, Search: "resovia"));

            var roster = page.Items.ShouldHaveSingleItem();
            roster.LeagueId.ShouldBe(_men.Id);
            roster.Name.ShouldBe("Resovia");
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheRequestedPageAndTheTotalCount()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club1 = NameSeeds.Club(_men, "Club 1");
            var club2 = NameSeeds.Club(_men, "Club 2");
            var club3 = NameSeeds.Club(_men, "Club 3");
            await SeedAsync(
                _men,
                season,
                club1,
                club2,
                club3,
                NameSeeds.Roster(_men, season, club1, "Club 1"),
                NameSeeds.Roster(_men, season, club2, "Club 2"),
                NameSeeds.Roster(_men, season, club3, "Club 3"));

            var page = await Handle(new GetRostersQuery(Offset: 1, Limit: 1));

            page.Items.ShouldHaveSingleItem().Name.ShouldBe("Club 2");
            page.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task HandleAsync_Should_MapTheRosterToTheResponse()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            await SeedAsync(_men, season, club, roster);

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
