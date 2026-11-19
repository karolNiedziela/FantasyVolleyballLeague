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

        [Fact]
        public async Task HandleAsync_Should_ReturnTheLatestSeasonAsCurrentSeason()
        {
            var wanted = League("TAURON Liga", 1, LeagueGender.Men);
            var other = League("TAURON 1. Liga", 2, LeagueGender.Men);
            await SeedAsync(
                wanted,
                other,
                Season(2024, wanted),
                Season(2026, wanted),
                Season(2025, wanted),
                Season(2027, other));

            var result = await Handle(new GetLeagueByIdQuery(wanted.Id));

            var currentSeason = result.ShouldNotBeNull().CurrentSeason.ShouldNotBeNull();
            currentSeason.Name.ShouldBe("2026-2027");
            currentSeason.StartYear.ShouldBe(2026);
            currentSeason.EndYear.ShouldBe(2027);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnNullCurrentSeason_When_TheLeagueHasNoSeasons()
        {
            var wanted = League("TAURON Liga", 1, LeagueGender.Men);
            var other = League("TAURON 1. Liga", 2, LeagueGender.Men);
            await SeedAsync(wanted, other, Season(2026, other));

            var result = await Handle(new GetLeagueByIdQuery(wanted.Id));

            result.ShouldNotBeNull().CurrentSeason.ShouldBeNull();
        }

        private static Season Season(int startYear, League league)
        {
            return new Season(startYear, startYear + 1, league.Id, externalId: null);
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
