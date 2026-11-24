using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Rosters;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rosters.GetRosters
{
    public sealed class GetRostersEndpointTests : IntegrationTestBase
    {
        private const string Rosters = "/api/v1/rosters";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _women = new League("Women's league", 1, LeagueCountry.Poland.Id, LeagueGender.Women.Id);

        public GetRostersEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetRosters_Should_ReturnTheRostersByNameWithDefaultPaging()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var lublin = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(
                _men,
                season,
                zawiercie,
                lublin,
                NameSeeds.Roster(_men, season, zawiercie, "Zawiercie"),
                NameSeeds.Roster(_men, season, lublin, "Lublin"));

            var response = await Get(Rosters);
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(roster => roster.Name).ShouldBe(["Lublin", "Zawiercie"]);
        }

        [Fact]
        public async Task GetRosters_Should_ApplyTheFiltersAndTheSearchFromTheQueryString()
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

            var byLeague = await ReadPage(await Get($"{Rosters}?leagueId={_women.Id}"));
            var bySeason = await ReadPage(await Get($"{Rosters}?seasonId={menSeason.Id}&q=RESOVIA"));

            byLeague.Items.ShouldHaveSingleItem().LeagueId.ShouldBe(_women.Id);
            bySeason.Items.ShouldHaveSingleItem().Name.ShouldBe("Resovia");
        }

        [Fact]
        public async Task GetRosters_Should_SortByTheFieldNameFromTheQueryString()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            await SeedAsync(
                _men,
                older,
                newer,
                lublin,
                zawiercie,
                NameSeeds.Roster(_men, older, lublin, "Lublin"),
                NameSeeds.Roster(_men, newer, zawiercie, "Zawiercie"));

            var page = await ReadPage(await Get($"{Rosters}?sort=-startYear"));

            page.Items.Select(roster => roster.Name).ShouldBe(["Zawiercie", "Lublin"]);
        }

        [Fact]
        public async Task GetRosters_Should_ReturnAnEmptyPage_When_TheOffsetIsBeyondTheEnd()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(_men, season, club, NameSeeds.Roster(_men, season, club, "Lublin"));

            var page = await ReadPage(await Get($"{Rosters}?offset=1000"));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task GetRosters_Should_ReturnBadRequest_When_SortIsUnknown()
        {
            var response = await Get($"{Rosters}?sort=city");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetRosters_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var response = await Get($"{Rosters}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Fact]
        public async Task GetRosters_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            var response = await Get($"{Rosters}?offset=-1");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["offset"]);
        }

        [Theory]
        [InlineData("leagueId=abc")]
        [InlineData("seasonId=abc")]
        public async Task GetRosters_Should_ReturnBadRequest_When_AnIdCannotBeParsed(string query)
        {
            var response = await Get($"{Rosters}?{query}");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }

        private static async Task<PagedResponse<RosterResponse>> ReadPage(HttpResponseMessage response)
        {
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var page = await response.Content.ReadFromJsonAsync<PagedResponse<RosterResponse>>(TestContext.Current.CancellationToken);

            return page.ShouldNotBeNull();
        }

        private static async Task<HttpValidationProblemDetails> ReadProblem(HttpResponseMessage response)
        {
            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(TestContext.Current.CancellationToken);

            return problem.ShouldNotBeNull();
        }
    }
}
