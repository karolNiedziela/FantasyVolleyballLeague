using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    /// <summary>
    /// Player statistics of seasons 2008/09–2018/19, rendered as two <c>table.table-colored</c> tables
    /// (home first). Row header: <c>&lt;th&gt;&lt;span&gt;&amp;nbsp;1 &amp;nbsp;&lt;/span&gt;&lt;a href="/players/id/1516/..."&gt;Name&lt;/a&gt;&lt;/th&gt;</c>.
    /// </summary>
    internal static class PlsTableColoredParser
    {
        public static bool CanParse(HtmlDocument document)
        {
            return GetTables(document).Count >= 2;
        }

        public static IReadOnlyList<PlayerMatchStatistics> GetPlayers(HtmlDocument document, int tableIndex)
        {
            var tables = GetTables(document);
            if (tables.Count <= tableIndex)
            {
                throw new InvalidOperationException($"Stats table {tableIndex} not found");
            }

            return tables[tableIndex]
                .SelectNodes(".//tbody/tr")
                ?.Select(ParsePlayerRow)
                .OfType<PlayerMatchStatistics>()
                .ToList() ?? [];
        }

        private static List<HtmlNode> GetTables(HtmlDocument document)
        {
            return [.. document.DocumentNode.FindDescendantsWithClass("table", "table-colored")];
        }

        private static PlayerMatchStatistics? ParsePlayerRow(HtmlNode row)
        {
            var th = row.Element("th");
            var link = th?.Descendants("a").FirstOrDefault(a => PlsExternalIds.FromPlayerUrl(a.GetAttributeValue("href", string.Empty)) is not null);
            if (th is null || link is null)
            {
                return null;
            }

            var cells = row.Elements("td")
                .Select(td => td.CleanText())
                .ToArray();

            // td columns: 0-4=sets, 5=PointsTotal, 6=BP, 7=Bilans,
            //   8=ServeTotal, 9=ServeErrors, 10=Aces, 11=ServeEff%,
            //   12=ReceptionTotal, 13=ReceptionErrors, 14=Pos%, 15=Perf%,
            //   16=AttackTotal, 17=AttackErrors, 18=Blocked, 19=AttackPts, 20=Skut%, 21=Eff%,
            //   22=Blocks, 23=Wyblok, 24=Obrona, 25=Asysta
            return new PlayerMatchStatistics
            {
                Number = HtmlNodeExtensions.ParseIntOrNull(th.Element("span")?.InnerText),
                Name = link.CleanText(),
                ExternalId = PlsExternalIds.FromPlayerUrl(link.GetAttributeValue("href", string.Empty)),
                PointsTotal = GetInt(cells, 5),
                ServeTotal = GetInt(cells, 8),
                ServeErrors = GetInt(cells, 9),
                Aces = GetInt(cells, 10),
                ReceptionTotal = GetInt(cells, 12),
                ReceptionErrors = GetInt(cells, 13),
                ReceptionPositivePercent = GetText(cells, 14),
                ReceptionPerfectPercent = GetText(cells, 15),
                AttackTotal = GetInt(cells, 16),
                AttackErrors = GetInt(cells, 17),
                AttackBlocked = GetInt(cells, 18),
                AttackPoints = GetInt(cells, 19),
                AttackEfficiencyPercent = GetText(cells, 20),
                Blocks = GetInt(cells, 22),
            };
        }

        private static int? GetInt(string[] cells, int index)
        {
            return index < cells.Length ? HtmlNodeExtensions.ParseIntOrNull(cells[index]) : null;
        }

        private static string? GetText(string[] cells, int index)
        {
            return index < cells.Length && cells[index].Length > 0 ? cells[index] : null;
        }
    }
}
