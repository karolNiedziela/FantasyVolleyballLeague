using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Seasons;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Seasons.GetLeagueSeasons
{
    public sealed class GetLeagueSeasonsEndpointTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        private readonly League _women = new League("Women's league", 1, LeagueCountry.Poland.Id, LeagueGender.Women.Id);

        public GetLeagueSeasonsEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetLeagueSeasons_Should_ReturnTheSeasonsOfTheLeagueNewestFirstWithDefaultPaging()
        {
            await SeedAsync(
                _men,
                _women,
                SeasonSeeds.Season(_men, 2021),
                SeasonSeeds.Season(_men, 2023),
                SeasonSeeds.Season(_men, 2022),
                SeasonSeeds.Season(_women, 2024));

            var response = await Get(Url(_men.Id));
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(3);
            page.Items.Select(season => season.StartYear).ShouldBe([2023, 2022, 2021]);
            page.Items.ShouldAllBe(season => season.LeagueId == _men.Id);
        }

        [Fact]
        public async Task GetLeagueSeasons_Should_ReturnNotFoundAsProblemDetails_When_TheLeagueDoesNotExist()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2023));

            var response = await Get(Url(EntityId.New()));

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetLeagueSeasons_Should_ReturnNotFound_When_TheLeagueIdIsNotAGuid(string leagueId)
        {
            var response = await Get($"/api/v1/leagues/{leagueId}/seasons");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetLeagueSeasons_Should_ApplyTheFiltersFromTheQueryString()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2023));

            var byStartYear = await ReadPage(await Get($"{Url(_men.Id)}?startYear=2022"));
            var current = await ReadPage(await Get($"{Url(_men.Id)}?current=true"));

            byStartYear.Items.ShouldHaveSingleItem().StartYear.ShouldBe(2022);
            current.Items.ShouldHaveSingleItem().StartYear.ShouldBe(2023);
        }

        [Fact]
        public async Task GetLeagueSeasons_Should_SortByTheFieldNameFromTheQueryString()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2022), SeasonSeeds.Season(_men, 2021), SeasonSeeds.Season(_men, 2023));

            var page = await ReadPage(await Get($"{Url(_men.Id)}?sort=startYear"));

            page.Items.Select(season => season.StartYear).ShouldBe([2021, 2022, 2023]);
        }

        [Fact]
        public async Task GetLeagueSeasons_Should_ReturnBadRequest_When_SortIsUnknown()
        {
            await SeedAsync(_men);

            var response = await Get($"{Url(_men.Id)}?sort=name");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetLeagueSeasons_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            await SeedAsync(_men);

            var response = await Get($"{Url(_men.Id)}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Fact]
        public async Task GetLeagueSeasons_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            await SeedAsync(_men);

            var response = await Get($"{Url(_men.Id)}?offset=-1");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["offset"]);
        }

        private static string Url(Guid leagueId)
        {
            return $"/api/v1/leagues/{leagueId}/seasons";
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
