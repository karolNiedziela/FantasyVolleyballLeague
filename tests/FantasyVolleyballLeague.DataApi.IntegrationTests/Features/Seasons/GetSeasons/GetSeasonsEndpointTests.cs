using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Seasons;
using FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasons;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Seasons.GetSeasons
{
    public sealed class GetSeasonsEndpointTests : IntegrationTestBase
    {
        private const string Seasons = "/api/v1/seasons";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _women = new League("Women's league", 1, LeagueCountry.Poland.Id, LeagueGender.Women.Id);

        public GetSeasonsEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetSeasons_Should_ReturnTheNewestSeasonsFirstWithDefaultPaging_When_NoParametersAreGiven()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2021), SeasonSeeds.Season(_men, 2023), SeasonSeeds.Season(_men, 2022));

            var response = await Get(Seasons);
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(3);
            page.Items.Select(season => season.StartYear).ShouldBe([2023, 2022, 2021]);
        }

        [Fact]
        public async Task GetSeasons_Should_ReturnTheDocumentedJsonShape()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2023));

            var response = await Get(Seasons);

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            var season = root["items"]!.AsArray().ShouldHaveSingleItem().ShouldNotBeNull().AsObject();
            PropertyNames(season).ShouldBe(["id", "name", "startYear", "endYear", "leagueName", "isCurrent", "leagueId"], ignoreOrder: true);
            season["name"]!.GetValue<string>().ShouldBe("2023-2024");
            season["startYear"]!.GetValueKind().ShouldBe(JsonValueKind.Number);
            season["endYear"]!.GetValueKind().ShouldBe(JsonValueKind.Number);
            season["isCurrent"]!.GetValueKind().ShouldBe(JsonValueKind.True);
            Guid.TryParse(season["id"]!.GetValue<string>(), out _).ShouldBeTrue();
            season["leagueId"]!.GetValue<string>().ShouldBe(_men.Id.ToString());
        }

        [Fact]
        public async Task GetSeasons_Should_ApplyTheFiltersFromTheQueryString()
        {
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2022),
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_women, 2022),
                SeasonSeeds.Season(_women, 2023));

            var page = await ReadPage(await Get($"{Seasons}?leagueId={_women.Id}&startYear=2022"));

            var season = page.Items.ShouldHaveSingleItem();
            season.LeagueId.ShouldBe(_women.Id);
            season.StartYear.ShouldBe(2022);
        }

        [Fact]
        public async Task GetSeasons_Should_ReturnOnlyTheLatestSeasonOfEachLeague_When_CurrentIsTrue()
        {
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2022),
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_women, 2021));

            var page = await ReadPage(await Get($"{Seasons}?current=true"));

            page.Items.Select(season => (season.LeagueId, season.StartYear))
                .ShouldBe([(_men.Id, 2023), (_women.Id, 2021)], ignoreOrder: true);
        }

        [Fact]
        public async Task GetSeasons_Should_SortByTheFieldNameFromTheQueryString()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2021), SeasonSeeds.Season(_men, 2023));

            var page = await ReadPage(await Get($"{Seasons}?sort=startYear"));

            page.Items.Select(season => season.StartYear).ShouldBe([2021, 2022, 2023]);
        }

        [Fact]
        public async Task GetSeasons_Should_ReturnAnEmptyPage_When_OffsetIsBeyondTheEnd()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022));

            var page = await ReadPage(await Get($"{Seasons}?offset=100"));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(1);
        }

        [Fact]
        public async Task GetSeasons_Should_ReturnBadRequest_When_SortIsUnknown()
        {
            var response = await Get($"{Seasons}?sort=name");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetSeasons_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var response = await Get($"{Seasons}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Fact]
        public async Task GetSeasons_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            var response = await Get($"{Seasons}?offset=-1");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["offset"]);
        }

        [Theory]
        [InlineData("leagueId=x")]
        [InlineData("startYear=x")]
        [InlineData("current=x")]
        [InlineData("limit=abc")]
        public async Task GetSeasons_Should_ReturnBadRequest_When_AParameterCannotBeParsed(string query)
        {
            var response = await Get($"{Seasons}?{query}");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status400BadRequest);
        }

        private static IEnumerable<string> PropertyNames(JsonObject node)
        {
            return node.Select(property => property.Key);
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }

        private static async Task<PagedResponse<SeasonResponse>> ReadPage(HttpResponseMessage response)
        {
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var page = await response.Content.ReadFromJsonAsync<PagedResponse<SeasonResponse>>(TestContext.Current.CancellationToken);

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
