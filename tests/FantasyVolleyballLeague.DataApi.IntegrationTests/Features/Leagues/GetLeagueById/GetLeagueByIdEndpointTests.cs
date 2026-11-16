using System.Net;
using System.Net.Http.Json;
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
        public async Task GetLeagueById_Should_ReturnTheLeagueWithItsLookups_When_TheLeagueExists()
        {
            var wanted = League("TAURON Liga", 1, LeagueGender.Men);
            await SeedAsync(wanted, League("TAURON Liga Kobiet", 1, LeagueGender.Women));

            var response = await Get($"{Leagues}/{wanted.Id}");
            var league = await response.Content.ReadFromJsonAsync<GetLeagueByIdResponse>(TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
            league.ShouldNotBeNull();
            league.Id.ShouldBe(wanted.Id);
            league.Name.ShouldBe("TAURON Liga");
            league.DivisionNumber.ShouldBe((short)1);
            league.Country.Id.ShouldBe(LeagueCountry.Poland.Id);
            league.Country.Name.ShouldBe(LeagueCountry.Poland.Name);
            league.Gender.Id.ShouldBe(LeagueGender.Men.Id);
            league.Gender.Name.ShouldBe(LeagueGender.Men.Name);
        }

        [Fact]
        public async Task GetLeagueById_Should_ReturnNotFoundAsProblemDetails_When_TheLeagueDoesNotExist()
        {
            await SeedAsync(League("TAURON Liga", 1, LeagueGender.Men));

            var response = await Get($"{Leagues}/{EntityId.New()}");
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
            problem.ShouldNotBeNull().Status.ShouldBe(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        public async Task GetLeagueById_Should_ReturnNotFound_When_TheIdIsNotAGuid(string id)
        {
            var response = await Get($"{Leagues}/{id}");

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
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
