using System.Globalization;
using FantasyVolleyballLeague.Worker.StatisticsScrapper;
using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga
{
    public sealed class PlusligaMatchStatisticsScrapper : IMatchStatisticsScrapper
    {
        public async Task<(MatchTeamStatistics FirstTeamStatistics, MatchTeamStatistics SecondTeamStatitics)?> GetMatchStatisticsAsync(int matchId, PlaywrightSession session)
        {
            var context = session.Context;
            var page = await context.NewPageAsync();

            await page.GotoAsync($"{UrlConstants.PlusLigaMatchDetailsUrl}{matchId}.html");

            var gameDateText = await page.Locator("div.game-date").First.InnerTextAsync();
            if (!TryParseMatchDate(gameDateText, out var matchDate) || matchDate > DateTime.Now)
            {
                await page.CloseAsync();
                return null;
            }

            var firstTeamStatistics = await GetTeamPlayerStatistics(page, context, "iframe.widget-team-a");
            var secondTeamStatistics = await GetTeamPlayerStatistics(page, context, "iframe.widget-team-b");

            var (firstTeamSetsWon, secondTeamSetsWon) = await GetSetsWonByTeam(page);

            firstTeamStatistics.SetsWon = firstTeamSetsWon;
            firstTeamStatistics.Won = firstTeamSetsWon > secondTeamSetsWon;
            secondTeamStatistics.SetsWon = secondTeamSetsWon;
            secondTeamStatistics.Won = secondTeamSetsWon > firstTeamSetsWon;

            await page.CloseAsync();

            return (firstTeamStatistics, secondTeamStatistics);
        }

        private static async Task<MatchTeamStatistics> GetTeamPlayerStatistics(IPage page, IBrowserContext context, string iframeClass)
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
            {
                matchTeamStatistics.PlayersStatistics.Add(new PlayerMatchStatistics(
                    GetPlayerNumber(tableRow),
                    GetPlayerName(tableRow),
                    GetPlayerPointsTotal(tableRow),
                    GetPlayerServeTotal(tableRow),
                    GetPlayerServeErrors(tableRow),
                    GetPlayerServeAces(tableRow),
                    GetPlayerReceptionTotal(tableRow),
                    GetPlayerReceptionErrors(tableRow),
                    GetPlayerReceptionPositivePercent(tableRow),
                    GetPlayerReceptionPerfectPercent(tableRow),
                    GetPlayerAttackTotal(tableRow),
                    GetPlayerAttackErrors(tableRow),
                    GetPlayerAttackBlocked(tableRow),
                    GetPlayerAttackPoints(tableRow),
                    GetPlayerAttackEfficiencyPercent(tableRow),
                    GetPlayerBlocks(tableRow)
                ));
            }

            await framePage.CloseAsync();

            return matchTeamStatistics;
        }

        private static int GetPlayerNumber(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantWithClass("div", "player-number")?.InnerText.Trim() ?? "-1");

        private static string GetPlayerName(HtmlNode tableRow)
            => tableRow.Descendants("a")
                .FirstOrDefault(a => a.HasClass("player-name"))?.InnerText.Trim() ?? "Unknown Player Name";

        private static int GetPlayerPointsTotal(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantWithClass("div", "sum", "attack")?.InnerText.Trim() ?? "-1");

        private static int GetPlayerServeTotal(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div => div.HasClass("sum") && !div.HasClass("attack"))?.InnerText.Trim() ?? "-1");

        private static int GetPlayerServeErrors(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantWithClass("div", "error")?.InnerText.Trim() ?? "-1");

        private static int GetPlayerServeAces(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantWithClass("div", "ace")?.InnerText.Trim() ?? "-1");

        private static int GetPlayerReceptionTotal(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .Where(div => div.HasClass("sum") && !div.HasClass("attack"))
                .Skip(2).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static int GetPlayerReceptionErrors(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantsWithClass("div", "error")
                .Skip(1).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static string GetPlayerReceptionPositivePercent(HtmlNode tableRow)
            => tableRow.FindDescendantWithClass("div", "pos-ratio")?.InnerText.Trim() ?? "-1";

        private static string GetPlayerReceptionPerfectPercent(HtmlNode tableRow)
            => tableRow.FindDescendantWithClass("div", "perfect-ratio")?.InnerText.Trim() ?? "-1";

        private static int GetPlayerAttackTotal(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .Where(div => div.HasClass("sum") && !div.HasClass("attack"))
                .Skip(3).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static int GetPlayerAttackErrors(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantsWithClass("div", "error")
                .Skip(2).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static int GetPlayerAttackBlocked(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantWithClass("div", "block")?.InnerText.Trim() ?? "-1");

        private static int GetPlayerAttackPoints(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div => div.HasClass("pkt") && !div.HasClass("block"))?.InnerText.Trim() ?? "-1");

        private static string GetPlayerAttackEfficiencyPercent(HtmlNode tableRow)
            => tableRow.FindDescendantsWithClass("div", "perfect-ratio")
                .Skip(1).FirstOrDefault()?.InnerText.Trim() ?? "-1";

        private static int GetPlayerBlocks(HtmlNode tableRow)
            => int.Parse(tableRow.FindDescendantWithClass("div", "pkt", "block")?.InnerText.Trim() ?? "-1");

        private static bool TryParseMatchDate(string text, out DateTime matchDate)
            => DateTime.TryParseExact(text.Trim(), "dd.MM.yyyy, HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out matchDate);

        private static async Task<(int FirstTeamSetsWon, int SecondTeamSetsWon)> GetSetsWonByTeam(IPage page)
        {
            var html = await page.ContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return (GetSetsWon(doc, "teamAStatus"), GetSetsWon(doc, "teamBStatus"));
        }

        private static int GetSetsWon(HtmlDocument document, string teamStatus)
        {
            var setsWon = document.DocumentNode.Descendants("span")
                .FirstOrDefault(span =>
                {
                    var teamAttribute = span.Attributes
                        .FirstOrDefault(attr => attr.Name.Equals("data-synced-games-class", StringComparison.Ordinal));
                    return teamAttribute != null && teamAttribute.Value.Equals(teamStatus, StringComparison.Ordinal);
                })?.InnerText.Trim() ?? "-1";

            return int.Parse(setsWon);
        }
    }
}
