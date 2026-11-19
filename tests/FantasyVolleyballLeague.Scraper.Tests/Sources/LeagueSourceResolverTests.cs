using FantasyVolleyballLeague.Scraper.Sources;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources
{
    public sealed class LeagueSourceResolverTests
    {
        private readonly FakeLeagueSource _pls = new("Pls");
        private readonly FakeLeagueSource _other = new("Other");

        [Fact]
        public void Resolve_Should_ReturnTheSourceWithTheConfiguredKey()
        {
            var resolver = new LeagueSourceResolver([_other, _pls]);

            var source = resolver.Resolve(League("Pls"));

            source.ShouldBeSameAs(_pls);
        }

        [Fact]
        public void Resolve_Should_IgnoreTheCaseOfTheSourceKey()
        {
            var resolver = new LeagueSourceResolver([_other, _pls]);

            var source = resolver.Resolve(League("pls"));

            source.ShouldBeSameAs(_pls);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void Resolve_Should_Throw_When_TheLeagueHasNoSource(string source)
        {
            var resolver = new LeagueSourceResolver([_pls]);

            var exception = Should.Throw<InvalidOperationException>(() => resolver.Resolve(League(source)));

            exception.Message.ShouldContain("Leagues:Source");
            exception.Message.ShouldContain("TestLeague");
        }

        [Fact]
        public void Resolve_Should_ListTheKnownSources_When_TheSourceIsUnknown()
        {
            var resolver = new LeagueSourceResolver([_other, _pls]);

            var exception = Should.Throw<InvalidOperationException>(() => resolver.Resolve(League("Nope")));

            exception.Message.ShouldContain("unknown source 'Nope'");
            exception.Message.ShouldContain("Pls");
            exception.Message.ShouldContain("Other");
        }

        [Fact]
        public void Constructor_Should_Throw_When_TwoSourcesShareAKey()
        {
            Should.Throw<ArgumentException>(() => new LeagueSourceResolver([_pls, new FakeLeagueSource("PLS")]));
        }

        private static LeagueOptions League(string source)
        {
            return new LeagueOptions { Key = "TestLeague", Source = source };
        }
    }
}
