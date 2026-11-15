using System.Net;
using System.Net.Http.Json;
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
        public async Task GetLeagues_Should_ReturnTheLeaguesWithTheirLookupsAndDefaultPaging_When_NoParametersAreGiven()
        {
            await SeedAsync(
                League("Beta League", 1, LeagueGender.Women),
                League("Alpha League", 1, LeagueGender.Men));

            var response = await Get(Leagues);
            var page = await ReadPage(response);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(league => league.Name).ShouldBe(["Alpha League", "Beta League"]);
            var first = page.Items[0];
            first.DivisionNumber.ShouldBe((short)1);
            first.Country.Id.ShouldBe(LeagueCountry.Poland.Id);
            first.Country.Name.ShouldBe(LeagueCountry.Poland.Name);
            first.Gender.Id.ShouldBe(LeagueGender.Men.Id);
            first.Gender.Name.ShouldBe(LeagueGender.Men.Name);
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
            page.Items.Select(league => league.Name).ShouldBe([LeagueSeeds.LeagueName(3), LeagueSeeds.LeagueName(4)]);
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

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(3);
            page.Links.Prev.ShouldNotBeNull();
            page.Links.Next.ShouldBeNull();
        }

        [Theory]
        [InlineData("gender=2", "Women's top")]
        [InlineData("division=2", "Men's second")]
        [InlineData("gender=1&division=2", "Men's second")]
        [InlineData("gender=2&division=2", "")]
        public async Task GetLeagues_Should_ApplyTheFiltersFromTheQueryString(string query, string expectedNames)
        {
            await SeedAsync(
                League("Men's top", 1, LeagueGender.Men),
                League("Men's second", 2, LeagueGender.Men),
                League("Women's top", 1, LeagueGender.Women));

            var page = await ReadPage(await Get($"{Leagues}?{query}"));

            string.Join(", ", page.Items.Select(league => league.Name)).ShouldBe(expectedNames);
        }

        [Theory]
        [InlineData("sort=-divisionNumber")]
        [InlineData("sort=-DIVISIONNUMBER")]
        public async Task GetLeagues_Should_SortByTheFieldNamesFromTheQueryString(string query)
        {
            await SeedAsync(
                League("First", 1, LeagueGender.Men),
                League("Second", 2, LeagueGender.Men),
                League("Third", 3, LeagueGender.Men));

            var page = await ReadPage(await Get($"{Leagues}?{query}"));

            page.Items.Select(league => league.Name).ShouldBe(["Third", "Second", "First"]);
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnBadRequest_When_SortIsUnknown()
        {
            var response = await Get($"{Leagues}?sort=height");
            var problem = await ReadProblem(response);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
            var message = problem.Errors["sort"].ShouldHaveSingleItem();
            message.ShouldContain("Unknown sort field 'height'");
            message.ShouldContain("name, divisionNumber", Case.Sensitive);
        }

        [Theory]
        [InlineData("limit=0")]
        [InlineData("limit=101")]
        [InlineData("limit=1000")]
        public async Task GetLeagues_Should_ReturnBadRequest_When_LimitIsOutOfRange(string query)
        {
            var response = await Get($"{Leagues}?{query}");
            var problem = await ReadProblem(response);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            problem.Errors["limit"].ShouldHaveSingleItem().ShouldContain("The limit must be between 1 and 100");
        }

        [Fact]
        public async Task GetLeagues_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            var response = await Get($"{Leagues}?offset=-1");
            var problem = await ReadProblem(response);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            problem.Errors["offset"].ShouldHaveSingleItem().ShouldContain("The offset must be 0 or greater");
        }

        [Theory]
        [InlineData("limit=abc", "Limit")]
        [InlineData("offset=abc", "Offset")]
        [InlineData("offset=", "Offset")]
        [InlineData("gender=abc", "Gender")]
        [InlineData("division=x", "Division")]
        public async Task GetLeagues_Should_ReturnBadRequestNamingTheParameter_When_AParameterCannotBeParsed(string query, string parameter)
        {
            var response = await Get($"{Leagues}?{query}");
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status400BadRequest);
            problem.Detail.ShouldNotBeNull().ShouldContain(parameter);
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
            var page = await response.Content.ReadFromJsonAsync<PagedResponse<GetLeaguesResponse>>(TestContext.Current.CancellationToken);

            return page.ShouldNotBeNull();
        }

        private static async Task<HttpValidationProblemDetails> ReadProblem(HttpResponseMessage response)
        {
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(TestContext.Current.CancellationToken);

            return problem.ShouldNotBeNull();
        }
    }
}
