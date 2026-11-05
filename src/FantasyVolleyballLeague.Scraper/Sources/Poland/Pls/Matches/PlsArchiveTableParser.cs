using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    /// <summary>
    /// Lineups of seasons 2000/01–2007/08. The archive only lists which sets a player took part in,
    /// so counting statistics stay <c>null</c>. Row: <c>&lt;td&gt;&lt;span class="circlenumber"&gt;2&lt;/span&gt;&lt;/td&gt;&lt;td&gt;&lt;a href="/players/id/1522/..."&gt;Name&lt;/a&gt;&lt;/td&gt;</c>.
    /// </summary>
    internal static class PlsArchiveTableParser
    {
        /// <summary>
        /// Other tables (e.g. the one with the MVP) contain a single player link; lineup tables contain many.
        /// </summary>
        private const int MinPlayerRows = 3;

        public static IReadOnlyList<PlayerMatchStatistics> GetPlayers(HtmlDocument document, int tableIndex)
        {
            var table = GetLineupTables(document).ElementAtOrDefault(tableIndex);

            return table?.Descendants("tr")
                .Select(ParsePlayerRow)
                .OfType<PlayerMatchStatistics>()
                .ToList() ?? [];
        }

        private static List<HtmlNode> GetLineupTables(HtmlDocument document)
        {
            return [.. document.DocumentNode.Descendants("table")
                .Where(table => !table.Descendants("table").Any())
                .Where(table => table.Descendants("tr").Count(row => GetPlayerLink(row) is not null) >= MinPlayerRows)];
        }

        private static PlayerMatchStatistics? ParsePlayerRow(HtmlNode row)
        {
            var link = GetPlayerLink(row);
            if (link is null)
            {
                return null;
            }

            var name = link.CleanText();
            if (name.Length == 0)
            {
                return null;
            }

            var number = HtmlNodeExtensions.ParseIntOrNull(row.FindDescendantWithClass("span", "circlenumber")?.InnerText)
                ?? row.Elements("td").Select(td => HtmlNodeExtensions.ParseIntOrNull(td.InnerText)).FirstOrDefault(n => n is not null);

            return new PlayerMatchStatistics
            {
                Number = number,
                Name = name,
                ExternalId = PlsExternalIds.FromPlayerUrl(link.GetAttributeValue("href", string.Empty)),
            };
        }

        private static HtmlNode? GetPlayerLink(HtmlNode row)
        {
            return row.Descendants("a")
                .FirstOrDefault(a => PlsExternalIds.FromPlayerUrl(a.GetAttributeValue("href", string.Empty)) is not null);
        }
    }
}
