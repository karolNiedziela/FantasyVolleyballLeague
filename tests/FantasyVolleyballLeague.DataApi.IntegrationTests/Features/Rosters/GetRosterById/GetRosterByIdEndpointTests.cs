using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Rosters.GetRosterById
{
    public sealed class GetRosterByIdEndpointTests : IntegrationTestBase
    {
        private const string Rosters = "/api/v1/rosters";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetRosterByIdEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetRosterById_Should_ReturnTheDocumentedJsonShape()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin", externalId: 56);
            var player = NameSeeds.Player("Łukasz Kaczmarek");
            await SeedAsync(
                _men,
                season,
                roster,
                player,
                NameSeeds.RosterPlayer(
                    roster,
                    player,
                    shirtNumber: 9,
                    position: PlayerPosition.OppositeHitter,
                    memberFrom: new DateOnly(2023, 9, 1)));

            var response = await Get($"{Rosters}/{roster.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            PropertyNames(root).ShouldBe(["id", "externalId", "name", "league", "season", "players"], ignoreOrder: true);
            root["externalId"]!.GetValue<int>().ShouldBe(56);
            PropertyNames(root["league"]!.AsObject()).ShouldBe(["id", "name", "divisionNumber", "country", "gender"], ignoreOrder: true);
            PropertyNames(root["season"]!.AsObject()).ShouldBe(["id", "name", "startYear", "endYear"], ignoreOrder: true);

            var playerNode = root["players"]!.AsArray().ShouldHaveSingleItem()!.AsObject();
            PropertyNames(playerNode).ShouldBe(
                ["playerId", "fullName", "shirtNumber", "position", "memberFrom", "memberTo"],
                ignoreOrder: true);
            playerNode["position"]!.GetValue<string>().ShouldBe("opposite_hitter");
            playerNode["memberFrom"]!.GetValue<string>().ShouldBe("2023-09-01");
            playerNode["memberTo"].ShouldBeNull();
        }

        [Fact]
        public async Task GetRosterById_Should_ReturnNotFoundAsProblemDetails_When_TheRosterDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season, NameSeeds.Roster(_men, season, "Lublin"));

            var response = await Get($"{Rosters}/{EntityId.New()}");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetRosterById_Should_ReturnNotFound_When_TheIdIsNotAGuid(string id)
        {
            var response = await Get($"{Rosters}/{id}");

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
