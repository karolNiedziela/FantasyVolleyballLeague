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
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Players.GetSeasonPlayers
{
    public sealed class GetSeasonPlayersEndpointTests : IntegrationTestBase
    {
        private readonly League _men = new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);

        public GetSeasonPlayersEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetSeasonPlayers_Should_ReturnThePlayersOfTheSeasonByNameWithDefaultPaging()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var otherSeason = SeasonSeeds.Season(_men, 2022);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var otherRoster = NameSeeds.Roster(_men, otherSeason, "Lublin");
            var zalewski = NameSeeds.Player("Zenon Zalewski");
            var nowak = NameSeeds.Player("Adam Nowak");
            var other = NameSeeds.Player("Other Season Player");
            await SeedAsync(
                _men,
                season,
                otherSeason,
                roster,
                otherRoster,
                zalewski,
                nowak,
                other,
                NameSeeds.RosterPlayer(roster, zalewski),
                NameSeeds.RosterPlayer(roster, nowak),
                NameSeeds.RosterPlayer(otherRoster, other));

            var response = await Get(Url(season.Id));
            var page = await ReadPage(response);

            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            page.Offset.ShouldBe(0);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(2);
            page.Items.Select(player => player.FullName).ShouldBe(["Adam Nowak", "Zenon Zalewski"]);
        }

        [Fact]
        public async Task GetSeasonPlayers_Should_ReturnTheSamePlayersAsTheListWithTheSeasonFilter()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var player = NameSeeds.Player("Jan Kowalski", heightCm: 200);
            await SeedAsync(
                _men,
                season,
                roster,
                player,
                NameSeeds.RosterPlayer(roster, player, position: PlayerPosition.Libero));

            var alias = await ReadPage(await Get(Url(season.Id)));
            var list = await ReadPage(await Get($"/api/v1/players?seasonId={season.Id}"));

            alias.Items.ShouldHaveSingleItem();
            JsonSerializer.Serialize(alias.Items).ShouldBe(JsonSerializer.Serialize(list.Items));
        }

        [Fact]
        public async Task GetSeasonPlayers_Should_ReturnNotFoundAsProblemDetails_When_TheSeasonDoesNotExist()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get(Url(EntityId.New()));

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetSeasonPlayers_Should_ReturnNotFound_When_TheSeasonIdIsNotAGuid(string seasonId)
        {
            var response = await Get($"/api/v1/seasons/{seasonId}/players");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSeasonPlayers_Should_ApplyThePositionRosterSearchAndSortFromTheQueryString()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            var roster = NameSeeds.Roster(_men, season, "Lublin");
            var otherRoster = NameSeeds.Roster(_men, season, "Zawiercie");
            var first = NameSeeds.Player("Jan Setter");
            var second = NameSeeds.Player("Piotr Setter");
            var libero = NameSeeds.Player("Jan Libero");
            var elsewhere = NameSeeds.Player("Jan Elsewhere");
            await SeedAsync(
                _men,
                season,
                roster,
                otherRoster,
                first,
                second,
                libero,
                elsewhere,
                NameSeeds.RosterPlayer(roster, first, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(roster, second, position: PlayerPosition.Setter),
                NameSeeds.RosterPlayer(roster, libero, position: PlayerPosition.Libero),
                NameSeeds.RosterPlayer(otherRoster, elsewhere, position: PlayerPosition.Setter));

            var bySearch = await ReadPage(await Get($"{Url(season.Id)}?position=setter&rosterId={roster.Id}&q=JAN"));
            var sorted = await ReadPage(await Get($"{Url(season.Id)}?position=setter&sort=-fullName"));

            bySearch.Items.ShouldHaveSingleItem().Id.ShouldBe(first.Id);
            sorted.Items.Select(player => player.FullName).ShouldBe(["Piotr Setter", "Jan Setter", "Jan Elsewhere"]);
        }

        [Theory]
        [InlineData("position", "striker")]
        [InlineData("position", "Setter")]
        public async Task GetSeasonPlayers_Should_ReturnBadRequest_When_ALookupCodeIsUnknown(string parameter, string value)
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?{parameter}={value}");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe([parameter]);
        }

        [Theory]
        [InlineData("heightCm")]
        [InlineData("name")]
        public async Task GetSeasonPlayers_Should_ReturnBadRequest_When_SortIsUnknown(string sort)
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?sort={sort}");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["sort"]);
        }

        [Fact]
        public async Task GetSeasonPlayers_Should_ReturnBadRequest_When_LimitIsAboveTheMaximum()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?limit=101");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["limit"]);
        }

        [Fact]
        public async Task GetSeasonPlayers_Should_ReturnBadRequest_When_OffsetIsNegative()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?offset=-1");
            var problem = await ReadProblem(response);

            problem.Errors.Keys.ShouldBe(["offset"]);
        }

        [Fact]
        public async Task GetSeasonPlayers_Should_ReturnBadRequest_When_TheRosterIdCannotBeParsed()
        {
            var season = SeasonSeeds.Season(_men, 2023);
            await SeedAsync(_men, season);

            var response = await Get($"{Url(season.Id)}?rosterId=abc");

            await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
        }

        private static string Url(Guid seasonId)
        {
            return $"/api/v1/seasons/{seasonId}/players";
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
