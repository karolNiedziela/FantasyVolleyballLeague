using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rounds.GetSeasonRounds
{
    public sealed class GetSeasonRoundsEndpointTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetSeasonRoundsEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetSeasonRounds_Should_ReturnTheRoundsOfTheSeasonInOrderWithDefaultPaging()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            await SeedAsync(
                _men,
                season,
                phase,
                SeasonSeeds.Round(phase, "Kolejka 2", order: 2),
                SeasonSeeds.Round(phase, "Kolejka 1", order: 1));

            var response = await Get(Url(season.Id));
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(round => round.Name).ShouldBe(["Kolejka 1", "Kolejka 2"]);
        }

        [Fact]
        public async Task GetSeasonRounds_Should_ReturnNotFoundAsProblemDetails_When_TheSeasonDoesNotExist()
        {
            var response = await Get(Url(EntityId.New()));

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetSeasonRounds_Should_ReturnNotFound_When_TheSeasonIdIsNotAGuid(string seasonId)
        {
            var response = await Get($"/api/v1/seasons/{seasonId}/rounds");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSeasonRounds_Should_FilterByPhase()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            await SeedAsync(
                _men,
                season,
                regular,
                playoff,
                SeasonSeeds.Round(regular, "Kolejka 1", order: 1),
                SeasonSeeds.Round(playoff, "Para 1", order: 1));

            var page = await ReadPage(await Get($"{Url(season.Id)}?phaseId={playoff.Id}"));

            page.Items.ShouldHaveSingleItem().Name.ShouldBe("Para 1");
        }

        [Fact]
        public async Task GetSeasonRounds_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?limit=101");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Errors.Keys.ShouldBe(["limit"]);
        }

        private static string Url(Guid seasonId)
        {
            return $"/api/v1/seasons/{seasonId}/rounds";
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }

        private static async Task<PagedResponse<RoundSummaryResponse>> ReadPage(HttpResponseMessage response)
        {
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var page = await response.Content.ReadFromJsonAsync<PagedResponse<RoundSummaryResponse>>(TestContext.Current.CancellationToken);

            return page.ShouldNotBeNull();
        }
    }
}
