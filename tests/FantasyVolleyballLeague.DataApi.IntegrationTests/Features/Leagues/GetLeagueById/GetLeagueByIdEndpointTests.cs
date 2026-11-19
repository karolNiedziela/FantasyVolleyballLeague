using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagueById;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Leagues.GetLeagueById
{
    public sealed class GetLeagueByIdEndpointTests : IntegrationTestBase
    {
        private const string Leagues = "/api/v1/leagues";

        public GetLeagueByIdEndpointTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetLeagueById_Should_ReturnTheLeague_When_TheLeagueExists()
        {
            var wanted = League("TAURON Liga", 1, LeagueGender.Men);
            await SeedAsync(wanted, League("TAURON Liga Kobiet", 1, LeagueGender.Women));

            var response = await Get($"{Leagues}/{wanted.Id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
            response.ShouldHaveMediaType(MediaTypeNames.Application.Json);
            var league = await response.Content.ReadFromJsonAsync<GetLeagueByIdResponse>(TestContext.Current.CancellationToken);
            league.ShouldNotBeNull();
            league.Id.ShouldBe(wanted.Id);
            league.Name.ShouldBe("TAURON Liga");
        }

        [Fact]
        public async Task GetLeagueById_Should_ReturnNotFoundAsProblemDetails_When_TheLeagueDoesNotExist()
        {
            await SeedAsync(League("TAURON Liga", 1, LeagueGender.Men));

            var response = await Get($"{Leagues}/{EntityId.New()}");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetLeagueById_Should_ReturnNotFound_When_TheIdIsNotAGuid(string id)
        {
            var response = await Get($"{Leagues}/{id}");

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        }

        private static League League(string name, short divisionNumber, LeagueGender gender)
        {
            return new League(name, divisionNumber, LeagueCountry.Poland.Id, gender.Id);
        }

        private async Task<HttpResponseMessage> Get(string pathAndQuery)
        {
            return await Client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), TestContext.Current.CancellationToken);
        }
    }
}
