using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsUrlsTests
    {
        private static readonly LeagueOptions League = new() { BaseUrl = "https://www.tauronliga.pl" };

        [Fact]
        public void Games_Should_ReturnTheGamesPageOfTheLeague()
        {
            PlsUrls.Games(League).ShouldBe("https://www.tauronliga.pl/games.html");
        }

        [Fact]
        public void Teams_Should_ReturnTheTeamsPageOfTheLeague()
        {
            PlsUrls.Teams(League).ShouldBe("https://www.tauronliga.pl/teams.html");
        }

        [Fact]
        public void Match_Should_ReturnTheMatchPage()
        {
            PlsUrls.Match(League, 1104808).ShouldBe("https://www.tauronliga.pl/games/action/show/id/1104808.html");
        }

        [Fact]
        public void Team_Should_ReturnTheTeamPageOfTheSeason()
        {
            PlsUrls.Team(League, 18, 53).ShouldBe("https://www.tauronliga.pl/teams/id/18/tour/53.html");
        }

        [Fact]
        public void Team_Should_ReturnTheTeamPageWithoutASeason_When_TheTourIsNull()
        {
            PlsUrls.Team(League, 18, null).ShouldBe("https://www.tauronliga.pl/teams/id/18.html");
        }

        [Fact]
        public void Player_Should_ReturnThePlayerPageOfTheSeason()
        {
            PlsUrls.Player(League, 2433, 53).ShouldBe("https://www.tauronliga.pl/players/id/2433/tour/53.html");
        }
    }
}
