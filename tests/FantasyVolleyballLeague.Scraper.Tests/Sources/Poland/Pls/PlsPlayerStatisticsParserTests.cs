using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsPlayerStatisticsParserTests
    {
        private const string TableColoredMatch = "match_5604.html";
        private const string ArchiveMatch = "match_1000675.html";

        [Theory]
        [InlineData(TableColoredMatch, true)]
        [InlineData(ArchiveMatch, false)]
        public void CanParse_Should_DetectTheTableColoredEra(string fixture, bool expected)
        {
            PlsTableColoredParser.CanParse(Fixture.Load(fixture)).ShouldBe(expected);
        }

        [Fact]
        public void TableColoredGetPlayers_Should_ReadTheFirstPlayerOfTheHomeTable()
        {
            var home = PlsTableColoredParser.GetPlayers(Fixture.Load(TableColoredMatch), 0);

            var first = home[0];
            (first.Number, first.Name, first.ExternalId).ShouldBe((1, "Mariusz Schamlewski", 1516));
        }

        [Fact]
        public void TableColoredGetPlayers_Should_ReadNumberIdAndPointsOfEveryPlayer()
        {
            var document = Fixture.Load(TableColoredMatch);
            var players = PlsTableColoredParser.GetPlayers(document, 0)
                .Concat(PlsTableColoredParser.GetPlayers(document, 1))
                .ToList();

            players.ShouldNotBeEmpty();
            players.ShouldAllBe(player => player.Number != null && player.ExternalId != null && player.PointsTotal != null);
        }

        [Fact]
        public void TableColoredGetPlayers_Should_ReturnDifferentPlayersForTheHomeAndAwayTables()
        {
            var document = Fixture.Load(TableColoredMatch);
            var home = PlsTableColoredParser.GetPlayers(document, 0);
            var away = PlsTableColoredParser.GetPlayers(document, 1);

            home.Select(player => player.ExternalId).Intersect(away.Select(player => player.ExternalId)).ShouldBeEmpty();
        }

        [Fact]
        public void TableColoredGetPlayers_Should_Throw_When_TheTableDoesNotExist()
        {
            var document = Fixture.Load(ArchiveMatch);

            var exception = Should.Throw<InvalidOperationException>(() => PlsTableColoredParser.GetPlayers(document, 1));

            exception.Message.ShouldContain("Stats table 1 not found");
        }

        [Fact]
        public void ArchiveGetPlayers_Should_ReadTheLineupWithoutStatistics()
        {
            var home = PlsArchiveTableParser.GetPlayers(Fixture.Load(ArchiveMatch), 0);

            var wlazly = home.Where(player => player.ExternalId == 1522).ShouldHaveSingleItem();
            (wlazly.Name, wlazly.Number).ShouldBe(("Mariusz Wlazły", 2));
            wlazly.PointsTotal.ShouldBeNull();
        }

        [Fact]
        public void ArchiveGetPlayers_Should_ReturnDifferentPlayersForTheHomeAndAwayTables()
        {
            var document = Fixture.Load(ArchiveMatch);
            var home = PlsArchiveTableParser.GetPlayers(document, 0);
            var away = PlsArchiveTableParser.GetPlayers(document, 1);

            away.ShouldNotBeEmpty();
            home.Select(player => player.ExternalId).Intersect(away.Select(player => player.ExternalId)).ShouldBeEmpty();
        }

        [Theory]
        [InlineData("match_1104808.html", true)]
        [InlineData("match_1104989.html", false)]
        [InlineData(TableColoredMatch, false)]
        public void WidgetParserDetectsWidgetEra(string fixture, bool expected)
        {
            PlsWidgetParser.CanParse(Fixture.Load(fixture)).ShouldBe(expected);
        }
    }
}
