using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rounds.GetRounds
{
    public sealed class GetRoundsEndpointTests : IntegrationTestBase
    {
        private const string Rounds = "/api/v1/rounds";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetRoundsEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetRounds_Should_ReturnTheRoundsOfTheSeasonInOrderWithDefaultPaging()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            await SeedAsync(
                _men,
                season,
                phase,
                SeasonSeeds.Round(phase, "Kolejka 2", order: 2),
                SeasonSeeds.Round(phase, "Kolejka 1", order: 1));

            var response = await Get($"{Rounds}?seasonId={season.Id}");
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(round => round.Name).ShouldBe(["Kolejka 1", "Kolejka 2"]);
        }

        [Fact]
        public async Task GetRounds_Should_ReturnTheDocumentedJsonShape()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            await SeedAsync(_men, season, phase, SeasonSeeds.Round(phase, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1));

            var response = await Get($"{Rounds}?seasonId={season.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            PropertyNames(root).ShouldBe(["items", "offset", "limit", "totalCount", "links"], ignoreOrder: true);
            var round = root["items"]!.AsArray().ShouldHaveSingleItem().ShouldNotBeNull().AsObject();
            PropertyNames(round).ShouldBe(["id", "name", "order", "stage", "phase"], ignoreOrder: true);
            PropertyNames(round["stage"]!.AsObject()).ShouldBe(["name", "order"], ignoreOrder: true);
            PropertyNames(round["phase"]!.AsObject()).ShouldBe(["id", "name", "type", "order"], ignoreOrder: true);
            round["phase"]!["type"]!.GetValue<string>().ShouldBe("playoff");
        }

        [Fact]
        public async Task GetRounds_Should_FilterByPhase()
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

            var page = await ReadPage(await Get($"{Rounds}?seasonId={season.Id}&phaseId={playoff.Id}"));

            page.Items.ShouldHaveSingleItem().Name.ShouldBe("Para 1");
        }

        [Fact]
        public async Task GetRounds_Should_ReturnAnEmptyPage_When_TheSeasonDoesNotExist()
        {
            var page = await ReadPage(await Get($"{Rounds}?seasonId={EntityId.New()}"));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task GetRounds_Should_ReturnBadRequest_When_TheSeasonIdIsMissing()
        {
            var response = await Get(Rounds);

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status400BadRequest);
        }

        [Theory]
        [InlineData("seasonId=abc")]
        [InlineData("seasonId=")]
        public async Task GetRounds_Should_ReturnBadRequest_When_TheSeasonIdCannotBeParsed(string query)
        {
            var response = await Get($"{Rounds}?{query}");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
        }

        [Fact]
        public async Task GetRounds_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var response = await Get($"{Rounds}?seasonId={EntityId.New()}&limit=101");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Errors.Keys.ShouldBe(["limit"]);
        }

        private static IEnumerable<string> PropertyNames(JsonObject node)
        {
            return node.Select(property => property.Key);
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
