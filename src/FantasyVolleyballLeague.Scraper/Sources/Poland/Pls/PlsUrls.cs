using System.Globalization;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls
{
    /// <summary>
    /// URL patterns of the PLS websites, relative to <see cref="LeagueOptions.BaseUrl"/>.
    /// </summary>
    internal static class PlsUrls
    {
        public static string Games(LeagueOptions league)
        {
            return $"{league.BaseUrl}/games.html";
        }

        public static string Teams(LeagueOptions league)
        {
            return $"{league.BaseUrl}/teams.html";
        }

        public static string Match(LeagueOptions league, int matchId)
        {
            return $"{league.BaseUrl}/games/action/show/id/{matchId.ToString(CultureInfo.InvariantCulture)}.html";
        }

        public static string Team(LeagueOptions league, int clubId, int? tour)
        {
            var club = clubId.ToString(CultureInfo.InvariantCulture);
            return tour is null
                ? $"{league.BaseUrl}/teams/id/{club}.html"
                : $"{league.BaseUrl}/teams/id/{club}/tour/{tour.Value.ToString(CultureInfo.InvariantCulture)}.html";
        }
    }
}
