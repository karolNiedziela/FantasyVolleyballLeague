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

            PlsTableColoredParser.CanParse(document).ShouldBeTrue();
            var home = PlsTableColoredParser.GetPlayers(document, 0);
            var away = PlsTableColoredParser.GetPlayers(document, 1);

            var first = home[0];
            (first.Number, first.Name, first.ExternalId).ShouldBe((1, "Mariusz Schamlewski", 1516));
            foreach (var player in home.Concat(away))
            {
                player.Number.ShouldNotBeNull();
                player.ExternalId.ShouldNotBeNull();
                player.PointsTotal.ShouldNotBeNull();
            }
            home.Select(p => p.ExternalId).Intersect(away.Select(p => p.ExternalId)).ShouldBeEmpty();
        }

        [Fact]
        public void ArchiveTableParserReadsLineupsWithoutStatistics()
        {
            var document = Fixture.Load("match_1000675.html");

            PlsTableColoredParser.CanParse(document).ShouldBeFalse();
            var home = PlsArchiveTableParser.GetPlayers(document, 0);
            var away = PlsArchiveTableParser.GetPlayers(document, 1);

            var wlazly = home.Where(player => player.ExternalId == 1522).ShouldHaveSingleItem();
            (wlazly.Name, wlazly.Number).ShouldBe(("Mariusz Wlazły", 2));
            wlazly.PointsTotal.ShouldBeNull();
            away.ShouldNotBeEmpty();
            home.Select(p => p.ExternalId).Intersect(away.Select(p => p.ExternalId)).ShouldBeEmpty();
        }

        [Theory]
        [InlineData("match_1104808.html", true)]
        [InlineData("match_1104989.html", false)]
        [InlineData("match_5604.html", false)]
        public void WidgetParserDetectsWidgetEra(string fixture, bool expected)
        {
            PlsWidgetParser.CanParse(Fixture.Load(fixture)).ShouldBe(expected);
        }
    }
}
