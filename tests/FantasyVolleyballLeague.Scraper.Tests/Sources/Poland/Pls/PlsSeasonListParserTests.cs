using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Seasons;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsSeasonListParserTests
    {
        private const string BaseUrl = "https://www.tauronliga.pl";

        [Fact]
        public void ParseGamesPageReturnsSeasonsNewestFirstWithTourIds()
        {
            var seasons = PlsSeasonListParser.Parse(Fixture.Load("games_tour53.html"), BaseUrl);

            (seasons[0].StartYear, seasons[0].EndYear, seasons[0].Tour).ShouldBe((2026, 2027, 53));
            (seasons[1].StartYear, seasons[1].Tour).ShouldBe((2025, 52));
            (seasons[2].StartYear, seasons[2].Tour).ShouldBe((2024, 47));
            seasons[0].Url.AbsoluteUri.ShouldBe("https://www.tauronliga.pl/games/tour/53/nocookies/1.html");
            seasons.ShouldContain(s => s.StartYear == 2000);
        }

        [Fact]
        public void ParseGamesPageWithSingleSeasonReadsTheSeasonButton()
        {
            var seasons = PlsSeasonListParser.Parse(Fixture.Load("games_tour62_single_season.html"), "https://www.tauron1ligakobiet.pl");

            var season = seasons.ShouldHaveSingleItem();
            (season.StartYear, season.EndYear, season.Tour).ShouldBe((2026, 2027, 62));
            season.Url.AbsoluteUri.ShouldBe("https://www.tauron1ligakobiet.pl/games/tour/62/nocookies/1.html");
        }

        [Fact]
        public void ParseTeamsPageReturnsTeamSeasonUrls()
        {
            var seasons = PlsSeasonListParser.Parse(Fixture.Load("teams_tour53.html"), BaseUrl);

            seasons[0].Tour.ShouldBe(53);
            seasons[0].Url.AbsoluteUri.ShouldBe("https://www.tauronliga.pl/teams/tour/53/nocookies/1.html");
        }
    }
}
