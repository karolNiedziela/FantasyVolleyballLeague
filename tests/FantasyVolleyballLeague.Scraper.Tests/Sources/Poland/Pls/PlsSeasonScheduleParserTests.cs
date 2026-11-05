using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsSeasonScheduleParserTests
    {
        [Fact]
        public void ParseCurrentSeasonListsEveryRegularSeasonMatchWithoutTickerMatches()
        {
            var schedule = PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour53.html"));

            // 14 teams, 26 rounds: 14 * 26 / 2 = 182 matches. The page also lists a few ticker matches.
            Assert.Equal(182, schedule.Count);
            Assert.All(schedule, entry => Assert.Equal(("Regular Season", 1), (entry.Phase.Name, entry.Phase.Order)));
            Assert.All(schedule, entry => Assert.Equal(string.Empty, entry.Stage.Name));
            Assert.Equal(26, schedule.Select(entry => entry.Round.Order).Distinct().Count());
            Assert.Contains(schedule, entry => entry.Round is { Name: "Kolejka 1", Order: 1 });
        }

        [Fact]
        public void ParsePlayoffUsesStageAndPairNames()
        {
            var schedule = PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour52.html"));

            var match = Assert.Single(schedule, entry => entry.MatchId == 1104742);
            Assert.Equal(("Playoff", 2), (match.Phase.Name, match.Phase.Order));
            Assert.Equal(("I Ćwierćfinał", 1), (match.Stage.Name, match.Stage.Order));
            Assert.Equal(("Para 1", 1), (match.Round.Name, match.Round.Order));
        }

        [Fact]
        public void ParseArchiveSeasonMapsAllPhases()
        {
            var schedule = PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour5.html"));

            Assert.Equal(
                ["Regular Season", "Playoff", "Relegation"],
                schedule.Select(entry => entry.Phase.Name).Distinct());
            var match = Assert.Single(schedule, entry => entry.MatchId == 1000675);
            Assert.Equal("Playoff", match.Phase.Name);
            Assert.Equal("I Ćwierćfinał", match.Stage.Name);
        }

        [Fact]
        public void ParseTableEraSeasonFindsPlacementMatch()
        {
            var schedule = PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour23.html"));

            var match = Assert.Single(schedule, entry => entry.MatchId == 5604);
            Assert.Equal("Playoff", match.Phase.Name);
            Assert.Equal("I Mecz o 13 miejsce", match.Stage.Name);
        }

        [Theory]
        [InlineData("Runda I: Ćwierćfinał (do 2 zwycięstw)", "I Ćwierćfinał")]
        [InlineData("Para nr: 1", "Para 1")]
        [InlineData("Kolejka: 12", "Kolejka 12")]
        [InlineData("Runda III: Mecze o 5. miejsce", "III Mecze o 5. miejsce")]
        public void NormalizeHeadingMatchesLegacyFolderNames(string heading, string expected)
        {
            Assert.Equal(expected, PlsSeasonScheduleParser.NormalizeHeading(heading));
        }
    }
}
