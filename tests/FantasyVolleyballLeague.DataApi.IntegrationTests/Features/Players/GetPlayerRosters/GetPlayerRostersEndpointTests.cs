using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json.Nodes;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetPlayerRosters
{
    public sealed class GetPlayerRostersEndpointTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetPlayerRostersEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetPlayerRosters_Should_ReturnTheDocumentedJsonShapeWithDefaultPaging()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(
                _men,
                season,
                club,
                roster,
                player,
                NameSeeds.RosterPlayer(
                    roster,
                    player,
                    shirtNumber: 9,
                    position: PlayerPosition.MiddleBlocker,
                    memberFrom: new DateOnly(2023, 9, 1)));

            var response = await Get(Url(player.Id));

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
            root["offset"]!.GetValue<int>().ShouldBe(0);
            root["limit"]!.GetValue<int>().ShouldBe(25);
            root["totalCount"]!.GetValue<int>().ShouldBe(1);

            var item = root["items"]!.AsArray().ShouldHaveSingleItem()!.AsObject();
            PropertyNames(item).ShouldBe(
                ["roster", "season", "league", "shirtNumber", "position", "memberFrom", "memberTo"],
                ignoreOrder: true);
            PropertyNames(item["roster"]!.AsObject()).ShouldBe(["id", "name"], ignoreOrder: true);
            PropertyNames(item["season"]!.AsObject()).ShouldBe(["id", "name", "startYear", "endYear"], ignoreOrder: true);
            PropertyNames(item["league"]!.AsObject()).ShouldBe(
                ["id", "name", "divisionNumber", "country", "gender"],
                ignoreOrder: true);
            item["shirtNumber"]!.GetValue<int>().ShouldBe(9);
            item["position"]!.GetValue<string>().ShouldBe("middle_blocker");
            item["memberFrom"]!.GetValue<string>().ShouldBe("2023-09-01");
            item["memberTo"].ShouldBeNull();
        }

        [Fact]
        public async Task GetPlayerRosters_Should_ReturnNewestSeasonFirst_AndApplyTheSortFromTheQueryString()
        {
            var older = SeasonSeeds.Season(_men, 2022);
            var newer = SeasonSeeds.Season(_men, 2023);
            var lublin = NameSeeds.Club(_men, "Lublin");
            var zawiercie = NameSeeds.Club(_men, "Zawiercie");
            var olderRoster = NameSeeds.Roster(_men, older, lublin, "Lublin");
            var newerRoster = NameSeeds.Roster(_men, newer, zawiercie, "Zawiercie");
            var player = NameSeeds.Player("Moved Club");
            await SeedAsync(
                _men,
                older,
                newer,
                lublin,
                zawiercie,
                olderRoster,
                newerRoster,
                player,
                NameSeeds.RosterPlayer(olderRoster, player),
                NameSeeds.RosterPlayer(newerRoster, player));

            var byDefault = await ReadItems(await Get(Url(player.Id)));
            var ascending = await ReadItems(await Get($"{Url(player.Id)}?sort=season"));

            byDefault.Select(item => item["roster"]!["name"]!.GetValue<string>()).ShouldBe(["Zawiercie", "Lublin"]);
            ascending.Select(item => item["roster"]!["name"]!.GetValue<string>()).ShouldBe(["Lublin", "Zawiercie"]);
        }

        [Fact]
        public async Task GetPlayerRosters_Should_ReturnAnEmptyPage_When_TheOffsetIsBeyondTheEnd()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(_men, season, club, roster, player, NameSeeds.RosterPlayer(roster, player));

            var items = await ReadItems(await Get($"{Url(player.Id)}?offset=1000"));

            items.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetPlayerRosters_Should_ReturnNotFoundAsProblemDetails_When_ThePlayerDoesNotExist()
        {
            await SeedAsync(NameSeeds.Player("Jan Kowalski"));

            var response = await Get(Url(EntityId.New()));

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetPlayerRosters_Should_ReturnNotFound_When_ThePlayerIdIsNotAGuid(string playerId)
        {
            var response = await Get($"/api/v1/players/{playerId}/rosters");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("name")]
        [InlineData("startYear")]
        public async Task GetPlayerRosters_Should_ReturnBadRequest_When_SortIsUnknown(string sort)
        {
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(player);

            var response = await Get($"{Url(player.Id)}?sort={sort}");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetPlayerRosters_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(player);

            var response = await Get($"{Url(player.Id)}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Fact]
        public async Task GetPlayerRosters_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            var player = NameSeeds.Player("Jan Kowalski");
            await SeedAsync(player);

            var response = await Get($"{Url(player.Id)}?offset=-1");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["offset"]);
        }

        private static string Url(Guid playerId)
        {
            return $"/api/v1/players/{playerId}/rosters";
        }

        private static IEnumerable<string> PropertyNames(JsonObject node)
        {
            return node.Select(property => property.Key);
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }

        private static async Task<List<JsonObject>> ReadItems(HttpResponseMessage response)
        {
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var root = (await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken)).ShouldNotBeNull();

            return root["items"]!.AsArray().Select(item => item!.AsObject()).ToList();
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
