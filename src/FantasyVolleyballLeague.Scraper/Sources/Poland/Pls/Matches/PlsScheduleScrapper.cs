using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    internal static class PlsScheduleScrapper
    {
        public static async Task<IReadOnlyList<ScheduleEntry>> GetScheduleAsync(Season season, PlaywrightSession session)
        {
            var html = await session.GetHtmlAsync(season.Url.AbsoluteUri);

            var document = new HtmlDocument();
            document.LoadHtml(html);

            return PlsSeasonScheduleParser.Parse(document);
        }
    }
}
