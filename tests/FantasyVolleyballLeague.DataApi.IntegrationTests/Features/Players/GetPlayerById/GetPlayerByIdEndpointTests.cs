using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetPlayerById
{
    public sealed class GetPlayerByIdEndpointTests : IntegrationTestBase
    {
        private const string Players = "/api/v1/players";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetPlayerByIdEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetPlayerById_Should_ReturnTheDocumentedJsonShape()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var player = new Player(LeagueGender.Men.Id, "Jan Kowalski", externalId: 77);
            player.UpdateProfile("Jan Kowalski", new DateOnly(1990, 1, 31), 200, 95, 345);
            await SeedAsync(
                _men,
                season,
                club,
                roster,
                player,
                NameSeeds.RosterPlayer(roster, player, shirtNumber: 9, position: PlayerPosition.MiddleBlocker));

            var response = await Get($"{Players}/{player.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            PropertyNames(root).ShouldBe(
                ["id", "externalId", "fullName", "gender", "dateOfBirth", "heightCm", "weightKg", "attackReachCm", "currentRoster"],
                ignoreOrder: true);
            root["externalId"]!.GetValue<int>().ShouldBe(77);
            root["gender"]!.GetValue<string>().ShouldBe("men");
            root["dateOfBirth"]!.GetValue<string>().ShouldBe("1990-01-31");

            var current = root["currentRoster"]!.AsObject();
            PropertyNames(current).ShouldBe(["roster", "season", "shirtNumber", "position"], ignoreOrder: true);
            PropertyNames(current["roster"]!.AsObject()).ShouldBe(["id", "name"], ignoreOrder: true);
            PropertyNames(current["season"]!.AsObject()).ShouldBe(["id", "name", "startYear", "endYear"], ignoreOrder: true);
            current["roster"]!["name"]!.GetValue<string>().ShouldBe("Lublin");
            current["shirtNumber"]!.GetValue<int>().ShouldBe(9);
            current["position"]!.GetValue<string>().ShouldBe("middle_blocker");
        }

        [Fact]
        public async Task GetPlayerById_Should_ReturnNullCurrentRosterAndExternalId_When_ThePlayerHasNeither()
        {
            var player = NameSeeds.Player("Unresolved Player");
            await SeedAsync(player);

            var response = await Get($"{Players}/{player.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            root["externalId"].ShouldBeNull();
            root["currentRoster"].ShouldBeNull();
        }

        [Fact]
        public async Task GetPlayerById_Should_ReturnNotFoundAsProblemDetails_When_ThePlayerDoesNotExist()
        {
            await SeedAsync(NameSeeds.Player("Jan Kowalski"));

            var response = await Get($"{Players}/{EntityId.New()}");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetPlayerById_Should_ReturnNotFound_When_TheIdIsNotAGuid(string id)
        {
            var response = await Get($"{Players}/{id}");

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
