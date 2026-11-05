using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    /// <summary>
    /// Player statistics of seasons 2019/20+, rendered client-side by volleystation widgets
    /// (<c>iframe.widget-team-a</c> = home, <c>iframe.widget-team-b</c> = away).
    /// The widget does not expose player ids.
    /// </summary>
    internal static class PlsWidgetParser
    {
        public const string HomeIframeClass = "widget-team-a";
        public const string AwayIframeClass = "widget-team-b";

        private const int MinRowItems = 8;

        public static bool CanParse(HtmlDocument matchDocument)
        {
            return GetIframeSource(matchDocument, HomeIframeClass) is not null;
        }

        public static async Task<IReadOnlyList<PlayerMatchStatistics>> GetPlayersAsync(
            PlaywrightSession session, HtmlDocument matchDocument, string iframeClass)
        {
            var iframeSource = GetIframeSource(matchDocument, iframeClass)
                ?? throw new InvalidOperationException($"Iframe '{iframeClass}' not found");

            var page = await session.Context.NewPageAsync();
            try
            {
                await page.GotoAsync(iframeSource);
                await page.WaitForSelectorAsync(".team-stats-widget");

                var widgetDocument = new HtmlDocument();
                widgetDocument.LoadHtml(await page.ContentAsync());

                return Parse(widgetDocument);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public static IReadOnlyList<PlayerMatchStatistics> Parse(HtmlDocument widgetDocument)
        {
            var tableMatchNode = widgetDocument.DocumentNode.FindDescendantWithClass("div", "table", "match")
                ?? throw new InvalidOperationException("Widget table (div.table.match) not found");

            return tableMatchNode.Descendants("div")
                .Where(div => div.HasClass("table-row") && !div.HasClass("table-header") && !div.HasClass("summary"))
                .Select(ParsePlayerRow)
                .OfType<PlayerMatchStatistics>()
                .ToList();
        }

        private static string? GetIframeSource(HtmlDocument matchDocument, string iframeClass)
        {
            var source = matchDocument.DocumentNode.FindDescendantWithClass("iframe", iframeClass)
                ?.GetAttributeValue("src", string.Empty);

            return string.IsNullOrEmpty(source) ? null : HtmlEntity.DeEntitize(source);
        }

        private static PlayerMatchStatistics? ParsePlayerRow(HtmlNode row)
        {
            var items = row.ChildNodes
                .Where(n => n.NodeType == HtmlNodeType.Element)
                .ToArray();

            var name = items.ElementAtOrDefault(1)?.FindDescendantWithClass("span", "name")?.CleanText();
            if (items.Length < MinRowItems || string.IsNullOrEmpty(name))
            {
                return null;
            }

            // items: [0]=number, [1]=name, [2]=sets, [3]=points, [4]=serve,
            //        [5]=reception, [6]=attack, [7]=block, [8]=other
            var points = GetColumns(items[3]);
            var serve = GetColumns(items[4]);
            var reception = GetColumns(items[5]);
            var attack = GetColumns(items[6]);
            var block = GetColumns(items[7]);

            return new PlayerMatchStatistics
            {
                Number = HtmlNodeExtensions.ParseIntOrNull(items[0].InnerText),
                Name = name,
                PointsTotal = GetInt(points, 0),
                ServeTotal = GetInt(serve, 0),
                ServeErrors = GetInt(serve, 1),
                Aces = GetInt(serve, 2),
                ReceptionTotal = GetInt(reception, 0),
                ReceptionErrors = GetInt(reception, 1),
                ReceptionPositivePercent = GetText(reception, 2),
                ReceptionPerfectPercent = GetText(reception, 3),
                AttackTotal = GetInt(attack, 0),
                AttackErrors = GetInt(attack, 1),
                AttackBlocked = GetInt(attack, 2),
                AttackPoints = GetInt(attack, 3),
                AttackEfficiencyPercent = GetText(attack, 4),
                Blocks = GetInt(block, 0),
            };
        }

        private static string[] GetColumns(HtmlNode section)
        {
            return [.. section.Descendants("div")
                .Where(d => d.HasClass("columns-item"))
                .Select(d => d.CleanText())];
        }

        private static int? GetInt(string[] columns, int index)
        {
            return index < columns.Length ? HtmlNodeExtensions.ParseIntOrNull(columns[index]) : null;
        }

        private static string? GetText(string[] columns, int index)
        {
            return index < columns.Length && columns[index].Length > 0 ? columns[index] : null;
        }
    }
}
