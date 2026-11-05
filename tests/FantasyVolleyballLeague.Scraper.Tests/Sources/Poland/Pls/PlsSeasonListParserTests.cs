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

            Assert.Equal((2026, 2027, 53), (seasons[0].StartYear, seasons[0].EndYear, seasons[0].Tour));
            Assert.Equal((2025, 52), (seasons[1].StartYear, seasons[1].Tour));
            Assert.Equal((2024, 47), (seasons[2].StartYear, seasons[2].Tour));
            Assert.Equal("https://www.tauronliga.pl/games/tour/53/nocookies/1.html", seasons[0].Url.AbsoluteUri);
            Assert.Contains(seasons, s => s.StartYear == 2000);
        }

        [Fact]
        public void ParseTeamsPageReturnsTeamSeasonUrls()
        {
            var seasons = PlsSeasonListParser.Parse(Fixture.Load("teams_tour53.html"), BaseUrl);

            Assert.Equal(53, seasons[0].Tour);
            Assert.Equal("https://www.tauronliga.pl/teams/tour/53/nocookies/1.html", seasons[0].Url.AbsoluteUri);
        }
    }
}
