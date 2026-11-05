using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsPlayerStatisticsParserTests
    {
        [Fact]
        public void TableColoredParserReadsShirtNumbersAndPlayerIds()
        {
            var document = Fixture.Load("match_5604.html");

            Assert.True(PlsTableColoredParser.CanParse(document));
            var home = PlsTableColoredParser.GetPlayers(document, 0);
            var away = PlsTableColoredParser.GetPlayers(document, 1);

            var first = home[0];
            Assert.Equal((1, "Mariusz Schamlewski", 1516), (first.Number, first.Name, first.ExternalId));
            Assert.All(home.Concat(away), player =>
            {
                Assert.NotNull(player.Number);
                Assert.NotNull(player.ExternalId);
                Assert.NotNull(player.PointsTotal);
            });
            Assert.Empty(home.Select(p => p.ExternalId).Intersect(away.Select(p => p.ExternalId)));
        }

        [Fact]
        public void ArchiveTableParserReadsLineupsWithoutStatistics()
        {
            var document = Fixture.Load("match_1000675.html");

            Assert.False(PlsTableColoredParser.CanParse(document));
            var home = PlsArchiveTableParser.GetPlayers(document, 0);
            var away = PlsArchiveTableParser.GetPlayers(document, 1);

            var wlazly = Assert.Single(home, player => player.ExternalId == 1522);
            Assert.Equal(("Mariusz Wlazły", 2), (wlazly.Name, wlazly.Number));
            Assert.Null(wlazly.PointsTotal);
            Assert.NotEmpty(away);
            Assert.Empty(home.Select(p => p.ExternalId).Intersect(away.Select(p => p.ExternalId)));
        }

        [Theory]
        [InlineData("match_1104808.html", true)]
        [InlineData("match_1104989.html", false)]
        [InlineData("match_5604.html", false)]
        public void WidgetParserDetectsWidgetEra(string fixture, bool expected)
        {
            Assert.Equal(expected, PlsWidgetParser.CanParse(Fixture.Load(fixture)));
        }
    }
}
