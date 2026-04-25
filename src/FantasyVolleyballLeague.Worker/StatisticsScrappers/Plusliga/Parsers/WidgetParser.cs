using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga.Parsers
{
    internal static class WidgetParser
    {
        internal static async Task<MatchTeamStatistics> GetTeamStatisticsAsync(IPage page, IBrowserContext context, string iframeClass)
        {
            var iframeElement = await page.WaitForSelectorAsync(iframeClass);
            if (iframeElement is null)
            {
                Console.WriteLine($"iframe with class '{iframeClass}' not found");
                throw new InvalidOperationException($"Iframe with class '{iframeClass}' not found");
            }

            var iframeSrc = await iframeElement.GetAttributeAsync("src");
            if (string.IsNullOrEmpty(iframeSrc))
            {
                Console.WriteLine("iframe src is null or empty");
                throw new InvalidOperationException("Iframe src is null or empty");
            }

            var framePage = await context.NewPageAsync();
            try
            {
                await framePage.GotoAsync(iframeSrc!);
                await framePage.WaitForSelectorAsync(".team-stats-widget");

                var html = await framePage.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var tableMatchNode = doc.DocumentNode.FindDescendantWithClass("div", "table", "match");
                if (tableMatchNode is null)
                {
                    Console.WriteLine("Table match node not found");
                    throw new InvalidOperationException("Table match node not found");
                }

                var allTableRows = tableMatchNode.Descendants("div")
                    .Where(div => div.HasClass("table-row") && !div.HasClass("table-header") && !div.HasClass("summary"))
                    .ToList();

                var teamName = doc.DocumentNode.FindDescendantWithClass("div", "title-container")
                    ?.Descendants("h2").FirstOrDefault()?.InnerText.Trim() ?? "Unknown Team Name";

                var matchTeamStatistics = new MatchTeamStatistics { Name = teamName };
                foreach (var tableRow in allTableRows)
                    matchTeamStatistics.PlayersStatistics.Add(ParsePlayerRow(tableRow));

                return matchTeamStatistics;
            }
            finally
            {
                await framePage.CloseAsync();
            }
        }

        private static PlayerMatchStatistics ParsePlayerRow(HtmlNode row)
        {
            var items = row.ChildNodes
                .Where(n => n.NodeType == HtmlNodeType.Element)
                .ToArray();

            static string[] Cols(HtmlNode section) =>
                section.Descendants("div")
                    .Where(d => d.HasClass("columns-item"))
                    .Select(d => d.InnerText.Trim())
                    .ToArray();

            // items: [0]=number, [1]=name, [2]=sets, [3]=points, [4]=serve,
            //        [5]=reception, [6]=attack, [7]=block, [8]=other
            var pts = Cols(items[3]);
            var srv = Cols(items[4]);
            var rec = Cols(items[5]);
            var atk = Cols(items[6]);
            var blk = Cols(items[7]);

            return new PlayerMatchStatistics(
                int.Parse(items[0].InnerText.Trim()),
                items[1].Descendants("span").First(s => s.HasClass("name")).InnerText.Trim(),
                int.Parse(pts[0]),
                int.Parse(srv[0]),
                int.Parse(srv[1]),
                int.Parse(srv[2]),
                int.Parse(rec[0]),
                int.Parse(rec[1]),
                rec[2],
                rec[3],
                int.Parse(atk[0]),
                int.Parse(atk[1]),
                int.Parse(atk[2]),
                int.Parse(atk[3]),
                atk[4],
                int.Parse(blk[0])
            );
        }
    }
}
