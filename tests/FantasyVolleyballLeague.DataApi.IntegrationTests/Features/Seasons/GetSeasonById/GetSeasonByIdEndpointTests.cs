using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Seasons.GetSeasonById
{
    public sealed class GetSeasonByIdEndpointTests : IntegrationTestBase
    {
        private const string Seasons = "/api/v1/seasons";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetSeasonByIdEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetSeasonById_Should_ReturnTheDocumentedJsonShape()
        {
            var previous = SeasonSeeds.Season(_men, 2022);
            var season = SeasonSeeds.Season(_men, 2023);
            var regular = SeasonSeeds.Phase(season, PhaseType.RegularSeason, order: 1);
            var playoff = SeasonSeeds.Phase(season, PhaseType.Playoff, order: 2);
            await SeedAsync(
                _men,
                previous,
                season,
                regular,
                playoff,
                SeasonSeeds.Round(regular, "Kolejka 1", order: 1),
                SeasonSeeds.Round(playoff, "Para 1", order: 1, stage: "I Ćwierćfinał", stageOrder: 1));

            var response = await Get($"{Seasons}/{season.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            PropertyNames(root).ShouldBe(
                ["id", "externalId", "name", "startYear", "endYear", "isCurrent", "league", "previousSeason", "nextSeason", "phases"],
                ignoreOrder: true);
            root["externalId"].ShouldBeNull();
            root["isCurrent"]!.GetValueKind().ShouldBe(JsonValueKind.True);
            root["nextSeason"].ShouldBeNull();
            PropertyNames(root["league"]!.AsObject()).ShouldBe(["id", "name", "divisionNumber", "country", "gender"], ignoreOrder: true);
            PropertyNames(root["previousSeason"]!.AsObject()).ShouldBe(["id", "name", "startYear", "endYear"], ignoreOrder: true);

            var phases = root["phases"]!.AsArray();
            phases.Count.ShouldBe(2);
            var firstPhase = phases[0]!.AsObject();
            PropertyNames(firstPhase).ShouldBe(["id", "name", "type", "order", "rounds"], ignoreOrder: true);
            firstPhase["type"]!.GetValue<string>().ShouldBe("regular_season");
            var regularRound = firstPhase["rounds"]!.AsArray().ShouldHaveSingleItem().ShouldNotBeNull().AsObject();
            PropertyNames(regularRound).ShouldBe(["id", "name", "order", "stage"], ignoreOrder: true);
            regularRound["stage"].ShouldBeNull();
            var playoffRound = phases[1]!["rounds"]!.AsArray().ShouldHaveSingleItem().ShouldNotBeNull().AsObject();
            PropertyNames(playoffRound["stage"]!.AsObject()).ShouldBe(["name", "order"], ignoreOrder: true);
        }

        [Fact]
        public async Task GetSeasonById_Should_ReturnNotFoundAsProblemDetails_When_TheSeasonDoesNotExist()
        {
            await SeedAsync(_men, SeasonSeeds.Season(_men, 2023));

            var response = await Get($"{Seasons}/{EntityId.New()}");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetSeasonById_Should_ReturnNotFound_When_TheIdIsNotAGuid(string id)
        {
            var response = await Get($"{Seasons}/{id}");

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
