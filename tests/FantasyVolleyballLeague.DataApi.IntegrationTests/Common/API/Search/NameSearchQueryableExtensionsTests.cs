using FantasyVolleyballLeague.DataApi.Common.API.Search;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Common.API.Search
{
    public sealed class NameSearchQueryableExtensionsTests : IntegrationTestBase
    {
        public NameSearchQueryableExtensionsTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData("rybak", new[] { "Anna Rybak-Czyrniańska" })]
        [InlineData("czyrnianska anna", new[] { "Anna Rybak-Czyrniańska" })]
        [InlineData("Rybak-Czyrniańska", new[] { "Anna Rybak-Czyrniańska" })]
        [InlineData("RYBAK", new[] { "Anna Rybak-Czyrniańska" })]
        [InlineData("kaczm", new[] { "Łukasz Kaczmarek" })]
        [InlineData("lukasz", new[] { "Łukasz Kaczmarek" })]
        [InlineData("ŁUKASZ kaczmarek", new[] { "Łukasz Kaczmarek" })]
        [InlineData("obrien", new[] { "Sean O'Brien" })]
        [InlineData("O'Brien", new[] { "Sean O'Brien" })]
        public async Task WhereNameMatches_Should_FindPlayers_Ignoring_Order_Case_Diacritics_And_Apostrophes(
            string query,
            string[] expected)
        {
            await SeedAsync(
                NameSeeds.Player("Anna Rybak-Czyrniańska", LeagueGender.Women),
                NameSeeds.Player("Łukasz Kaczmarek"),
                NameSeeds.Player("Sean O'Brien"),
                NameSeeds.Player("Jakub Kochanowski"));

            var names = await FindPlayerNamesAsync(query);

            names.ShouldBe(expected, ignoreOrder: true);
        }

        [Fact]
        public async Task WhereNameMatches_Should_RequireEveryWord()
        {
            await SeedAsync(
                NameSeeds.Player("Anna Rybak"),
                NameSeeds.Player("Anna Kowalska"));

            var names = await FindPlayerNamesAsync("anna kowalska");

            names.ShouldBe(["Anna Kowalska"]);
        }

        [Fact]
        public async Task WhereNameMatches_Should_ReturnNothing_When_NoNameMatches()
        {
            await SeedAsync(NameSeeds.Player("Anna Rybak"));

            var names = await FindPlayerNamesAsync("zzz");

            names.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("%")]
        [InlineData("-- '")]
        public async Task WhereNameMatches_Should_FilterNothing_When_TheQueryHasNoSearchableWords(string? query)
        {
            await SeedAsync(
                NameSeeds.Player("Anna Rybak"),
                NameSeeds.Player("Jan Kowalski"));

            var names = await FindPlayerNamesAsync(query);

            names.ShouldBe(["Anna Rybak", "Jan Kowalski"], ignoreOrder: true);
        }

        [Fact]
        public async Task WhereNameMatches_Should_NotTreatPercentAsAWildcard()
        {
            // "k%a" is the words "k" and "a": both are in "anna bak". As a LIKE pattern "%k%a%" would not match it ("a" is before "k").
            await SeedAsync(NameSeeds.Player("Anna Bak"));

            var names = await FindPlayerNamesAsync("k%a");

            names.ShouldBe(["Anna Bak"]);
        }

        [Fact]
        public async Task WhereNameMatches_Should_NotTreatUnderscoreAsAWildcard()
        {
            // "a_b" is the words "a" and "b". As a LIKE pattern it would match "axb" but not "b a".
            await SeedAsync(NameSeeds.Player("Ba Ca"));

            var names = await FindPlayerNamesAsync("a_b");

            names.ShouldBe(["Ba Ca"]);
        }

        [Fact]
        public async Task WhereNameMatches_Should_FindRosters()
        {
            var league = LeagueSeeds.CreateLeagues(1).Single();
            var season = SeasonSeeds.Season(league, 2025);
            var resovia = NameSeeds.Club(league, "ASSECO RESOVIA Rzeszów");
            var jastrzebski = NameSeeds.Club(league, "Jastrzębski Węgiel");
            await SeedAsync(
                league,
                season,
                resovia,
                jastrzebski,
                NameSeeds.Roster(league, season, resovia, "ASSECO RESOVIA Rzeszów"),
                NameSeeds.Roster(league, season, jastrzebski, "Jastrzębski Węgiel"));

            var names = await ExecuteDbContextAsync(dbContext => dbContext.Set<Roster>()
                .WhereNameMatches("resovia rzeszow")
                .Select(roster => roster.Name)
                .ToListAsync(TestContext.Current.CancellationToken));

            names.ShouldBe(["ASSECO RESOVIA Rzeszów"]);
        }

        private async Task<List<string>> FindPlayerNamesAsync(string? query)
        {
            return await ExecuteDbContextAsync(dbContext => dbContext.Set<Player>()
                .WhereNameMatches(query)
                .Select(player => player.FullName)
                .ToListAsync(TestContext.Current.CancellationToken));
        }
    }
}
