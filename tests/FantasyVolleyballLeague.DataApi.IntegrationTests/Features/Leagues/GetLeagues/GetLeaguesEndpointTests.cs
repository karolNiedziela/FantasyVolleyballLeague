using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagues;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Leagues.GetLeagues
{
    public sealed class GetLeaguesEndpointTests : IntegrationTestBase
    {
        private const string Leagues = "/api/v1/leagues";

        public GetLeaguesEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnTheLeaguesWithDefaultPaging_When_NoParametersAreGiven()
        {
            await SeedAsync(
                League("Beta League", 1, LeagueGender.Women),
                League("Alpha League", 1, LeagueGender.Men));

            var response = await Get(Leagues);
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(league => league.Name).ShouldBe(["Alpha League", "Beta League"]);
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnTheDocumentedJsonShape()
        {
            await SeedAsync(League("TAURON Liga", 1, LeagueGender.Men));

            var response = await Get(Leagues);

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            PropertyNames(root).ShouldBe(["items", "offset", "limit", "totalCount", "links"], ignoreOrder: true);
            PropertyNames(root["links"]!.AsObject()).ShouldBe(["self", "first", "prev", "next", "last"], ignoreOrder: true);
            var league = root["items"]!.AsArray().ShouldHaveSingleItem().ShouldNotBeNull().AsObject();
            PropertyNames(league).ShouldBe(["id", "name", "divisionNumber", "country", "gender"], ignoreOrder: true);
            PropertyNames(league["country"]!.AsObject()).ShouldBe(["id", "name"], ignoreOrder: true);
            PropertyNames(league["gender"]!.AsObject()).ShouldBe(["id", "name"], ignoreOrder: true);
            Guid.TryParse(league["id"]!.GetValue<string>(), out _).ShouldBeTrue();
            league["divisionNumber"]!.GetValueKind().ShouldBe(JsonValueKind.Number);
        }

        [Fact]
        public async Task GetLeagues_Should_WriteTheEffectiveOffsetAndLimitInSelf_When_NoPagingIsGiven()
        {
            await SeedAsync(League("Alpha League", 1, LeagueGender.Men));

            var page = await ReadPage(await Get(Leagues));

            page.Links.Self.ShouldBe("http://localhost/api/v1/leagues?offset=0&limit=25");
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnAbsoluteLinksThatKeepTheOtherParameters()
        {
            await SeedAsync(LeagueSeeds.CreateLeagues(5));

            var page = await ReadPage(await Get($"{Leagues}?sort=name&offset=2&limit=2"));

            page.Links.ShouldBe(new PageLinks(
                Self: "http://localhost/api/v1/leagues?sort=name&offset=2&limit=2",
                First: "http://localhost/api/v1/leagues?sort=name&offset=0&limit=2",
                Prev: "http://localhost/api/v1/leagues?sort=name&offset=0&limit=2",
                Next: "http://localhost/api/v1/leagues?sort=name&offset=4&limit=2",
                Last: "http://localhost/api/v1/leagues?sort=name&offset=4&limit=2"));
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnNullPrevAndNext_When_AllLeaguesFitOnOnePage()
        {
            await SeedAsync(League("Alpha League", 1, LeagueGender.Men));

            var page = await ReadPage(await Get(Leagues));

            page.Links.Prev.ShouldBeNull();
            page.Links.Next.ShouldBeNull();
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnAnEmptyPage_When_OffsetIsBeyondTheEnd()
        {
            await SeedAsync(LeagueSeeds.CreateLeagues(3));

            var response = await Get($"{Leagues}?offset=100");
            var page = await ReadPage(response);

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(3);
            page.Links.Prev.ShouldNotBeNull();
            page.Links.Next.ShouldBeNull();
        }

        [Fact]
        public async Task GetLeagues_Should_ApplyTheGenderAndDivisionFiltersFromTheQueryString()
        {
            await SeedAsync(
                League("Men's top", 1, LeagueGender.Men),
                League("Men's second", 2, LeagueGender.Men),
                League("Women's top", 1, LeagueGender.Women),
                League("Women's second", 2, LeagueGender.Women));

            var page = await ReadPage(await Get($"{Leagues}?gender={LeagueGender.Women.Id}&division=2"));

            page.Items.Select(league => league.Name).ShouldBe(["Women's second"]);
        }

        // Poland is the only country in the database, so an unknown id is the only way to see the filter applied.
        [Fact]
        public async Task GetLeagues_Should_ApplyTheCountryFilterFromTheQueryString()
        {
            await SeedAsync(League("Men's top", 1, LeagueGender.Men));

            var page = await ReadPage(await Get($"{Leagues}?country=99"));

            page.Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetLeagues_Should_SortByTheFieldNameFromTheQueryString()
        {
            await SeedAsync(
                League("First", 1, LeagueGender.Men),
                League("Second", 2, LeagueGender.Men),
                League("Third", 3, LeagueGender.Men));

            var page = await ReadPage(await Get($"{Leagues}?sort=-divisionNumber"));

            page.Items.Select(league => league.Name).ShouldBe(["Third", "Second", "First"]);
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnBadRequest_When_SortIsUnknown()
        {
            var response = await Get($"{Leagues}?sort=height");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var response = await Get($"{Leagues}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Theory]
        [InlineData("limit=abc")]
        [InlineData("offset=abc")]
        [InlineData("offset=")]
        [InlineData("gender=abc")]
        [InlineData("division=x")]
        public async Task GetLeagues_Should_ReturnBadRequest_When_AParameterCannotBeParsed(string query)
        {
            var response = await Get($"{Leagues}?{query}");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status400BadRequest);
        }

        private static IEnumerable<string> PropertyNames(JsonObject node)
        {
            return node.Select(property => property.Key);
        }

        private static League League(string name, short divisionNumber, LeagueGender gender)
        {
            return new League(name, divisionNumber, LeagueCountry.Poland.Id, gender.Id);
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }

        private static async Task<PagedResponse<GetLeaguesResponse>> ReadPage(HttpResponseMessage response)
        {
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var page = await response.Content.ReadFromJsonAsync<PagedResponse<GetLeaguesResponse>>(TestContext.Current.CancellationToken);

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
