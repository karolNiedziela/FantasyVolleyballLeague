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
            var club = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(_men, season, club, NameSeeds.Roster(_men, season, club, "Lublin"));

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
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var lancut = NameSeeds.Club(_men, "Łańcut");
            var lublin = NameSeeds.Club(_men, "Lublin");
            var otherSeasonClub = NameSeeds.Club(_men, "Other season club");
            await SeedAsync(
                _men,
                season,
                otherSeason,
                zawiercie,
                lancut,
                lublin,
                otherSeasonClub,
                NameSeeds.Roster(_men, season, zawiercie, "Zawiercie"),
                NameSeeds.Roster(_men, season, lancut, "Łańcut"),
                NameSeeds.Roster(_men, season, lublin, "Lublin"),
                NameSeeds.Roster(_men, otherSeason, otherSeasonClub, "Other season club"));

            var page = await Handle(new GetSeasonRostersQuery(season.Id));

            page.ShouldNotBeNull().Items.Select(roster => roster.Name).ShouldBe(["Lublin", "Łańcut", "Zawiercie"]);
            page.TotalCount.ShouldBe(3);
            page.Items.ShouldAllBe(roster => roster.Season.Id == season.Id);
        }

        [Fact]
        public async Task HandleAsync_Should_SortByNameDescending()
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

            var page = await Handle(new GetSeasonRostersQuery(season.Id, Sort: "-name"));

            page.ShouldNotBeNull().Items.Select(roster => roster.Name).ShouldBe(["Zawiercie", "Lublin"]);
        }

        [Fact]
        public async Task HandleAsync_Should_FindRostersByTheWordsOfTheName()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var resovia = NameSeeds.Club(_men, "ASSECO RESOVIA Rzeszów");
            var jastrzebski = NameSeeds.Club(_men, "Jastrzębski Węgiel");
            await SeedAsync(
                _men,
                season,
                otherSeason,
                resovia,
                jastrzebski,
                NameSeeds.Roster(_men, season, resovia, "ASSECO RESOVIA Rzeszów"),
                NameSeeds.Roster(_men, season, jastrzebski, "Jastrzębski Węgiel"),
                NameSeeds.Roster(_men, otherSeason, resovia, "ASSECO RESOVIA Rzeszów"));

            var page = await Handle(new GetSeasonRostersQuery(season.Id, Search: "rzeszow resovia"));

            var roster = page.ShouldNotBeNull().Items.ShouldHaveSingleItem();
            roster.Name.ShouldBe("ASSECO RESOVIA Rzeszów");
            roster.Season.Id.ShouldBe(season.Id);
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

            var page = await Handle(new GetSeasonRostersQuery(season.Id, Offset: 1, Limit: 1));

            page.ShouldNotBeNull().Items.ShouldHaveSingleItem().Name.ShouldBe("Club 2");
            page.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheSameItemsAsTheListFilteredBySeason()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var resovia = NameSeeds.Club(_men, "Resovia");
            var resoviaReserves = NameSeeds.Club(_men, "Resovia Rzeszów II");
            var lublin = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(
                _men,
                season,
                otherSeason,
                resovia,
                resoviaReserves,
                lublin,
                NameSeeds.Roster(_men, season, resovia, "Resovia"),
                NameSeeds.Roster(_men, season, resoviaReserves, "Resovia Rzeszów II"),
                NameSeeds.Roster(_men, season, lublin, "Lublin"),
                NameSeeds.Roster(_men, otherSeason, resovia, "Resovia"));

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
