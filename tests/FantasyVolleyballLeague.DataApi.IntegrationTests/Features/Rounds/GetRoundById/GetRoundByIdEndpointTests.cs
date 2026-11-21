using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rounds.GetRoundById
{
    public sealed class GetRoundByIdEndpointTests : IntegrationTestBase
    {
        private const string Rounds = "/api/v1/rounds";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetRoundByIdEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetRoundById_Should_ReturnTheDocumentedJsonShape()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            var last = SeasonSeeds.Round(regular, "Kolejka 26", order: 26);
            var middle = SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1);
            var final = SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "III Finał", stageOrder: 3);
            await SeedAsync(_men, season, regular, playoff, last, middle, final);

            var response = await Get($"{Rounds}/{middle.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            PropertyNames(root).ShouldBe(
                ["id", "name", "order", "stage", "phase", "season", "previousRound", "nextRound"],
                ignoreOrder: true);
            PropertyNames(root["stage"]!.AsObject()).ShouldBe(["name", "order"], ignoreOrder: true);
            PropertyNames(root["phase"]!.AsObject()).ShouldBe(["id", "name", "type", "order"], ignoreOrder: true);
            PropertyNames(root["season"]!.AsObject()).ShouldBe(["id", "name", "startYear", "endYear"], ignoreOrder: true);

            var previous = root["previousRound"]!.AsObject();
            PropertyNames(previous).ShouldBe(["id", "name", "order", "stage", "phase"], ignoreOrder: true);
            previous["stage"].ShouldBeNull();
            previous["phase"]!["type"]!.GetValue<string>().ShouldBe("regular_season");
            root["nextRound"]!["stage"]!["name"]!.GetValue<string>().ShouldBe("III Finał");
        }

        [Fact]
        public async Task GetRoundById_Should_ReturnNullNeighbours_When_TheSeasonHasOneRound()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var phase = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var only = SeasonSeeds.Round(phase, "Kolejka 1", order: 1);
            await SeedAsync(_men, season, phase, only);

            var response = await Get($"{Rounds}/{only.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            root["previousRound"].ShouldBeNull();
            root["nextRound"].ShouldBeNull();
        }

        [Fact]
        public async Task GetRoundById_Should_ReturnNotFoundAsProblemDetails_When_TheRoundDoesNotExist()
        {
            var response = await Get($"{Rounds}/{EntityId.New()}");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetRoundById_Should_ReturnNotFound_When_TheIdIsNotAGuid(string id)
        {
            var response = await Get($"{Rounds}/{id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        }

        private static IEnumerable<string> PropertyNames(JsonObject node)
        {
            return node.Select(property => property.Key);
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }
    }
}
