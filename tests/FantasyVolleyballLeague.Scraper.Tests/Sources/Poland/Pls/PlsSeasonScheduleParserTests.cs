using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsSeasonScheduleParserTests
    {
        [Fact]
        public void Parse_Should_ListEveryRegularSeasonMatchWithoutTickerMatches()
        {
            var schedule = ParseCurrentSeason();

            // 14 teams, 26 rounds: 14 * 26 / 2 = 182 matches. The page also lists a few ticker matches.
            schedule.Count.ShouldBe(182);
        }

        [Fact]
        public void Parse_Should_PutEveryMatchOfTheCurrentSeasonInTheRegularSeasonWithoutAStage()
        {
            var schedule = ParseCurrentSeason();

            schedule.ShouldAllBe(entry => entry.Phase.Name == "Regular Season" && entry.Phase.Order == 1 && entry.Stage.Name == string.Empty);
        }

        [Fact]
        public void Parse_Should_ReadTheRoundsOfTheCurrentSeason()
        {
            var schedule = ParseCurrentSeason();

            schedule.Select(entry => entry.Round.Order).Distinct().Count().ShouldBe(26);
            schedule.Any(entry => entry.Round is { Name: "Kolejka 1", Order: 1 }).ShouldBeTrue();
        }

        [Fact]
        public void ParsePlayoffUsesStageAndPairNames()
        {
            var schedule = PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour52.html"));

            var match = schedule.Where(entry => entry.MatchId == 1104742).ShouldHaveSingleItem();
            (match.Phase.Name, match.Phase.Order).ShouldBe(("Playoff", 2));
            (match.Stage.Name, match.Stage.Order).ShouldBe(("I Ćwierćfinał", 1));
            (match.Round.Name, match.Round.Order).ShouldBe(("Para 1", 1));
        }

        [Fact]
        public void ParseArchiveSeasonMapsAllPhases()
        {
            var schedule = PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour5.html"));

            schedule.Select(entry => entry.Phase.Name).Distinct().ShouldBe(["Regular Season", "Playoff", "Relegation"]);
            var match = schedule.Where(entry => entry.MatchId == 1000675).ShouldHaveSingleItem();
            match.Phase.Name.ShouldBe("Playoff");
            match.Stage.Name.ShouldBe("I Ćwierćfinał");
        }

        [Fact]
        public void ParseTableEraSeasonFindsPlacementMatch()
        {
            var schedule = PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour23.html"));

            var match = schedule.Where(entry => entry.MatchId == 5604).ShouldHaveSingleItem();
            match.Phase.Name.ShouldBe("Playoff");
            match.Stage.Name.ShouldBe("I Mecz o 13 miejsce");
        }

        [Theory]
        [InlineData("Runda I: Ćwierćfinał (do 2 zwycięstw)", "I Ćwierćfinał")]
        [InlineData("Para nr: 1", "Para 1")]
        [InlineData("Kolejka: 12", "Kolejka 12")]
        [InlineData("Runda III: Mecze o 5. miejsce", "III Mecze o 5. miejsce")]
        public void NormalizeHeadingMatchesLegacyFolderNames(string heading, string expected)
        {
            PlsSeasonScheduleParser.NormalizeHeading(heading).ShouldBe(expected);
        }

        private static IReadOnlyList<ScheduleEntry> ParseCurrentSeason()
        {
            return PlsSeasonScheduleParser.Parse(Fixture.Load("games_tour53.html"));
        }
    }
}
