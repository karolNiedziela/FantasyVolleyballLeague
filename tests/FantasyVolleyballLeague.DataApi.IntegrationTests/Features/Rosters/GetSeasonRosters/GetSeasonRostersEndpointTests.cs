using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Rosters;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rosters.GetSeasonRosters
{
    public sealed class GetSeasonRostersEndpointTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetSeasonRostersEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetSeasonRosters_Should_ReturnTheRostersOfTheSeasonByNameWithDefaultPaging()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var lublin = NameSeeds.Club(_men, "Lublin");
            var otherSeasonClub = NameSeeds.Club(_men, "Other season club");
            await SeedAsync(
                _men,
                season,
                otherSeason,
                zawiercie,
                lublin,
                otherSeasonClub,
                NameSeeds.Roster(_men, season, zawiercie, "Zawiercie"),
                NameSeeds.Roster(_men, season, lublin, "Lublin"),
                NameSeeds.Roster(_men, otherSeason, otherSeasonClub, "Other season club"));

            var response = await Get(Url(season.Id));
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(roster => roster.Name).ShouldBe(["Lublin", "Zawiercie"]);
            page.Items.ShouldAllBe(roster => roster.Season.Id == season.Id && roster.Season.StartYear == 2023);
        }

        [Fact]
        public async Task GetSeasonRosters_Should_ReturnNotFoundAsProblemDetails_When_TheSeasonDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(_men, season, club, NameSeeds.Roster(_men, season, club, "Lublin"));

            var response = await Get(Url(EntityId.New()));

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetSeasonRosters_Should_ReturnNotFound_When_TheSeasonIdIsNotAGuid(string seasonId)
        {
            var response = await Get($"/api/v1/seasons/{seasonId}/rosters");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSeasonRosters_Should_ApplyTheSearchAndTheSortFromTheQueryString()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var resovia = NameSeeds.Club(_men, "Resovia Rzeszów");
            var resoviaReserves = NameSeeds.Club(_men, "Resovia Rzeszów II");
            var lublin = NameSeeds.Club(_men, "Lublin");
            await SeedAsync(
                _men,
                season,
                resovia,
                resoviaReserves,
                lublin,
                NameSeeds.Roster(_men, season, resovia, "Resovia Rzeszów"),
                NameSeeds.Roster(_men, season, resoviaReserves, "Resovia Rzeszów II"),
                NameSeeds.Roster(_men, season, lublin, "Lublin"));

            var page = await ReadPage(await Get($"{Url(season.Id)}?q=RESOVIA&sort=-name"));

            page.Items.Select(roster => roster.Name).ShouldBe(["Resovia Rzeszów II", "Resovia Rzeszów"]);
        }

        [Fact]
        public async Task GetSeasonRosters_Should_ReturnBadRequest_When_SortIsUnknown()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?sort=city");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetSeasonRosters_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Fact]
        public async Task GetSeasonRosters_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?offset=-1");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["offset"]);
        }

        private static string Url(Guid seasonId)
        {
            return $"/api/v1/seasons/{seasonId}/rosters";
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
