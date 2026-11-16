using FantasyVolleyballLeague.DataApi.Features.Leagues.GetLeagueById;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Features.Leagues.GetLeagueById
{
    public sealed class GetLeagueByIdQueryHandlerTests : IntegrationTestBase
    {
        public GetLeagueByIdQueryHandlerTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNull_When_TheLeagueDoesNotExist()
        {
            await SeedAsync(League("TAURON Liga", 1, LeagueGender.Men));

            var result = await Handle(new GetLeagueByIdQuery(EntityId.New()));

            result.ShouldBeNull();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTheLeagueWithTheGivenId()
        {
            var wanted = League("TAURON Liga", 1, LeagueGender.Men);
            await SeedAsync(wanted, League("TAURON 1. Liga", 2, LeagueGender.Men));

            var result = await Handle(new GetLeagueByIdQuery(wanted.Id));

            var league = result.ShouldNotBeNull();
            league.Id.ShouldBe(wanted.Id);
            league.Name.ShouldBe("TAURON Liga");
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnDivisionCountryAndGenderAsIdAndName()
        {
            var wanted = League("TAURON Liga Kobiet", 1, LeagueGender.Women);
            await SeedAsync(wanted);

            var result = await Handle(new GetLeagueByIdQuery(wanted.Id));

            var league = result.ShouldNotBeNull();
            league.DivisionNumber.ShouldBe((short)1);
            league.Country.Id.ShouldBe(LeagueCountry.Poland.Id);
            league.Country.Name.ShouldBe(LeagueCountry.Poland.Name);
            league.Gender.Id.ShouldBe(LeagueGender.Women.Id);
            league.Gender.Name.ShouldBe(LeagueGender.Women.Name);
        }

        private static League League(string name, short divisionNumber, LeagueGender gender)
        {
            return new League(name, divisionNumber, LeagueCountry.Poland.Id, gender.Id);
        }

        private Task<GetLeagueByIdResponse?> Handle(GetLeagueByIdQuery query)
        {
            return ExecuteDbContextAsync(dbContext =>
                new GetLeagueByIdQueryHandler(dbContext).HandleAsync(query, TestContext.Current.CancellationToken));
        }
    }
}
