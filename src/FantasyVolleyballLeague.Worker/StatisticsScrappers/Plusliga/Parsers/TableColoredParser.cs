using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga.Parsers
{
    internal static class TableColoredParser
    {
        internal static MatchTeamStatistics GetTeamStatistics(HtmlDocument doc, int tableIndex)
        {
            var teamNames = doc.DocumentNode.SelectNodes("//h3[@class='notranslate']");
            var teamName = teamNames?[tableIndex]?.InnerText.Trim() ?? "Unknown Team Name";

            var tables = doc.DocumentNode.SelectNodes("//table[contains(@class,'table-colored')]");
            if (tables is null || tables.Count <= tableIndex)
                throw new InvalidOperationException($"Stats table {tableIndex} not found");

            var rows = tables[tableIndex]
                .SelectNodes(".//tbody/tr")
                ?.Where(tr => tr.SelectSingleNode("th") is not null)
                .ToList() ?? [];

            var stats = new MatchTeamStatistics { Name = teamName };
            foreach (var row in rows)
                stats.PlayersStatistics.Add(ParsePlayerRow(row));

            return stats;
        }

        private static PlayerMatchStatistics ParsePlayerRow(HtmlNode row)
        {
            var th = row.SelectSingleNode("th");
            var numberText = th?.SelectSingleNode("span")?.InnerText
                .Replace(" ", string.Empty).Trim() ?? "-1";
            var number = int.TryParse(numberText, out var n) ? n : -1;
            var name = th?.SelectSingleNode("a")?.InnerText.Trim() ?? "Unknown Player Name";

            var tds = row.SelectNodes("td")
                ?.Select(td => td.InnerText.Trim())
                .ToArray() ?? [];

            static int I(string[] c, int i) =>
                i < c.Length && int.TryParse(c[i], out var v) ? v : -1;
            static string S(string[] c, int i) =>
                i < c.Length ? c[i] : "-1";

            // td columns: 0-4=sets, 5=PointsTotal, 6=BP, 7=Bilans,
            //   8=ServeTotal, 9=ServeErrors, 10=Aces, 11=ServeEff%,
            //   12=ReceptionTotal, 13=ReceptionErrors, 14=Pos%, 15=Perf%,
            //   16=AttackTotal, 17=AttackErrors, 18=Blocked, 19=AttackPts, 20=Skut%, 21=Eff%,
            //   22=Blocks, 23=Wyblok, 24=Obrona, 25=Asysta
            return new PlayerMatchStatistics(
                number, name,
                I(tds, 5),  I(tds, 8),  I(tds, 9),  I(tds, 10),
                I(tds, 12), I(tds, 13), S(tds, 14), S(tds, 15),
                I(tds, 16), I(tds, 17), I(tds, 18), I(tds, 19), S(tds, 20),
                I(tds, 22));
        }
    }
}
