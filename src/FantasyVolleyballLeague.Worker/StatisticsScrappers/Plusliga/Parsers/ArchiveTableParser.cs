using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga.Parsers
{
    internal static class ArchiveTableParser
    {
        internal static MatchTeamStatistics GetTeamStatistics(HtmlNode? table, string? teamName)
        {
            var stats = new MatchTeamStatistics { Name = teamName ?? "Unknown Team Name" };
            if (table is null) return stats;

            var rows = table.SelectNodes(".//tbody/tr")?.OfType<HtmlNode>()
                       ?? table.SelectNodes(".//tr")?.OfType<HtmlNode>()
                       ?? [];

            foreach (var row in rows)
            {
                var player = ParsePlayerRow(row);
                if (player is not null)
                    stats.PlayersStatistics.Add(player);
            }

            return stats;
        }

        internal static List<SetScore> GetSetScores(HtmlDocument doc)
        {
            var result = new List<SetScore>();

            foreach (var n in doc.DocumentNode.Descendants("span")
                .Concat(doc.DocumentNode.Descendants("td")))
            {
                var text = n.InnerText.Trim();
                if (!text.Contains(':') || text.Contains('.')) continue;

                var parts = text.Split(':');
                if (parts.Length != 2) continue;
                if (!int.TryParse(parts[0].Trim(), out var a) || !int.TryParse(parts[1].Trim(), out var b)) continue;
                if (a < 10 || b < 10) continue;

                result.Add(new SetScore(a, b));
            }

            return result;
        }

        private static PlayerMatchStatistics? ParsePlayerRow(HtmlNode row)
        {
            var nameNode = row.SelectSingleNode(".//a[contains(@href,'/players/id/')]");
            if (nameNode is null) return null;

            var name = nameNode.InnerText.Trim();
            if (string.IsNullOrWhiteSpace(name)) return null;

            var number = -1;
            foreach (var cell in row.SelectNodes("td|th")?.OfType<HtmlNode>() ?? [])
            {
                if (int.TryParse(cell.InnerText.Trim(), out var n)) { number = n; break; }
            }

            return new PlayerMatchStatistics(
                number, name,
                0, 0, 0, 0,
                0, 0, "0", "0",
                0, 0, 0, 0, "0",
                0);
        }
    }
}
