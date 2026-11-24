using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Features.Players;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetPlayers
{
    public sealed class GetPlayersEndpointTests : IntegrationTestBase
    {
        private const string Players = "/api/v1/players";

        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetPlayersEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetPlayers_Should_ReturnThePlayersByNameWithDefaultPaging()
        {
            await SeedAsync(NameSeeds.Player("Zenon Zalewski"), NameSeeds.Player("Adam Nowak"));

            var response = await Get(Players);
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(player => player.FullName).ShouldBe(["Adam Nowak", "Zenon Zalewski"]);
        }

        [Fact]
        public async Task GetPlayers_Should_ReturnTheJsonShapeOfAPlayer()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var player = NameSeeds.Player(
                "Jan Kowalski",
                dateOfBirth: new DateOnly(1990, 1, 31),
                heightCm: 200,
                weightKg: 95,
                attackReachCm: 345);
            await SeedAsync(
                _men,
                season,
                club,
                roster,
                player,
                NameSeeds.RosterPlayer(roster, player, position: PlayerPosition.MiddleBlocker));

            var response = await Get(Players);

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            var item = json.RootElement.GetProperty("items")[0];
            item.EnumerateObject().Select(property => property.Name).ShouldBe(
                ["id", "fullName", "gender", "dateOfBirth", "heightCm", "weightKg", "attackReachCm", "roster", "position"],
                ignoreOrder: true);
            item.GetProperty("gender").GetString().ShouldBe("men");
            item.GetProperty("dateOfBirth").GetString().ShouldBe("1990-01-31");
            item.GetProperty("position").GetString().ShouldBe("middle_blocker");
            var rosterJson = item.GetProperty("roster");
            rosterJson.EnumerateObject().Select(property => property.Name).ShouldBe(["id", "name"], ignoreOrder: true);
            rosterJson.GetProperty("id").GetGuid().ShouldBe(roster.Id);
            rosterJson.GetProperty("name").GetString().ShouldBe("Lublin");
        }

        [Fact]
        public async Task GetPlayers_Should_ApplyTheFiltersAndTheSearchFromTheQueryString()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var club = NameSeeds.Club(_men, "Lublin");
            var roster = NameSeeds.Roster(_men, season, club, "Lublin");
            var setter = NameSeeds.Player("Jan Setter");
            var libero = NameSeeds.Player("Jan Libero");
            var woman = NameSeeds.Player("Anna Setter", LeagueGender.Women);
            await SeedAsync(
                _men,
                season,
                club,
                roster,
                setter,
                libero,
                woman,
                NameSeeds.RosterPlayer(roster, setter, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(roster, libero, position: PlayerPosition.Libero));

            var bySeasonAndPosition = await ReadPage(await Get($"{Players}?seasonId={season.Id}&position=setter&gender=men&q=JAN"));
            var byGender = await ReadPage(await Get($"{Players}?gender=women"));
            var byRoster = await ReadPage(await Get($"{Players}?rosterId={roster.Id}"));
            var byLeague = await ReadPage(await Get($"{Players}?leagueId={_men.Id}"));

            bySeasonAndPosition.Items.ShouldHaveSingleItem().Id.ShouldBe(setter.Id);
            byGender.Items.ShouldHaveSingleItem().Id.ShouldBe(woman.Id);
            byRoster.Items.Count.ShouldBe(2);
            byLeague.Items.Count.ShouldBe(2);
        }

        [Fact]
        public async Task GetPlayers_Should_SortByTheFieldNameFromTheQueryString()
        {
            await SeedAsync(NameSeeds.Player("Adam Nowak"), NameSeeds.Player("Zenon Zalewski"));

            var page = await ReadPage(await Get($"{Players}?sort=-fullName"));

            page.Items.Select(player => player.FullName).ShouldBe(["Zenon Zalewski", "Adam Nowak"]);
        }

        [Fact]
        public async Task GetPlayers_Should_ReturnAnEmptyPage_When_TheOffsetIsBeyondTheEnd()
        {
            await SeedAsync(NameSeeds.Player("Adam Nowak"));

            var page = await ReadPage(await Get($"{Players}?offset=1000"));

            page.Items.ShouldBeEmpty();
            page.TotalCount.ShouldBe(1);
        }

        [Theory]
        [InlineData("gender", "female")]
        [InlineData("gender", "Women")]
        [InlineData("position", "striker")]
        [InlineData("position", "Setter")]
        public async Task GetPlayers_Should_ReturnBadRequest_When_ALookupCodeIsUnknown(string parameter, string value)
        {
            var response = await Get($"{Players}?{parameter}={value}");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe([parameter]);
            problem.Errors[parameter].ShouldHaveSingleItem().ShouldContain($"'{value}'");
        }

        [Theory]
        [InlineData("heightCm")]
        [InlineData("dateOfBirth")]
        [InlineData("name")]
        public async Task GetPlayers_Should_ReturnBadRequest_When_SortIsUnknown(string sort)
        {
            var response = await Get($"{Players}?sort={sort}");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetPlayers_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var response = await Get($"{Players}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Fact]
        public async Task GetPlayers_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            var response = await Get($"{Players}?offset=-1");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["offset"]);
        }

        [Theory]
        [InlineData("rosterId=abc")]
        [InlineData("seasonId=abc")]
        [InlineData("leagueId=abc")]
        public async Task GetPlayers_Should_ReturnBadRequest_When_AnIdCannotBeParsed(string query)
        {
            var response = await Get($"{Players}?{query}");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }

        private static async Task<PagedResponse<PlayerResponse>> ReadPage(HttpResponseMessage response)
        {
            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            var page = await response.Content.ReadFromJsonAsync<PagedResponse<PlayerResponse>>(TestContext.Current.CancellationToken);

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
