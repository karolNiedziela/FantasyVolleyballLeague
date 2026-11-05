using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Seasons
{
    internal static class PlsSeasonScrapper
    {
        public static Task<IReadOnlyList<Season>> GetSeasonsAsync(LeagueOptions leagueOptions, PlaywrightSession session)
        {
            return GetSeasonsFromPageAsync(PlsUrls.Games(leagueOptions), leagueOptions.BaseUrl, session);
        }

        public static Task<IReadOnlyList<Season>> GetTeamSeasonsAsync(LeagueOptions leagueOptions, PlaywrightSession session)
        {
            return GetSeasonsFromPageAsync(PlsUrls.Teams(leagueOptions), leagueOptions.BaseUrl, session);
        }

        private static async Task<IReadOnlyList<Season>> GetSeasonsFromPageAsync(string pageUrl, string baseUrl, PlaywrightSession session)
        {
            var html = await session.GetHtmlAsync(pageUrl);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return PlsSeasonListParser.Parse(doc, baseUrl);
        }
    }
}
