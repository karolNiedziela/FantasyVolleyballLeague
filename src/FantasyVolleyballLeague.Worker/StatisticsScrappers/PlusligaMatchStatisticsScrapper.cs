using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;
using static System.Collections.Specialized.BitVector32;

namespace FantasyVolleyballLeague.Worker.StatisticsScrapper
{
    public sealed class PlusligaMatchStatisticsScrapper : IMatchStatisticsScrapper
    {
        private const string _matchDetailsUrl = "https://www.plusliga.pl/games/action/show/id/";

        public async Task<(MatchTeamStatistics FirstTeamStatistics, MatchTeamStatistics SecondTeamStatitics)> GetMatchStatisticsAsync(int matchId)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                Locale = "pl-PL"
            });
            await context.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', { get: () => undefined });");
            var page = await context.NewPageAsync();

            await page.GotoAsync($"{_matchDetailsUrl}{matchId}.html");

            var firstTeamStatistics = await GetTeamPlayerStatistics(page, context, "iframe.widget-team-a");
            var secondTeamStatistics = await GetTeamPlayerStatistics(page, context, "iframe.widget-team-b"); 
            
            var (firstTeamSetsWon, secondTeamSetsWon) = await GetSetsWonByTeam(page);

            firstTeamStatistics.SetsWon = firstTeamSetsWon;
            firstTeamStatistics.Won = firstTeamSetsWon > secondTeamSetsWon;
            secondTeamStatistics.SetsWon = secondTeamSetsWon;
            secondTeamStatistics.Won = secondTeamSetsWon > firstTeamSetsWon;

            return (firstTeamStatistics, secondTeamStatistics);
        }

        private static async Task<MatchTeamStatistics> GetTeamPlayerStatistics(IPage page, IBrowserContext context, string iframeClass)
        {
            var iframeElement = await page.WaitForSelectorAsync(iframeClass);
            if (iframeElement is null)
            {
                Console.WriteLine($"❌ iframe with class '{iframeClass}' not found");
                throw new InvalidOperationException($"Iframe with class '{iframeClass}' not found");
            }

            var iframeSrc = await iframeElement.GetAttributeAsync("src");
            if (string.IsNullOrEmpty(iframeSrc))
            {
                Console.WriteLine("❌ iframe src is null or empty");
                throw new InvalidOperationException("Iframe src is null or empty");
            }

            Console.WriteLine($"✅ iframe src: {iframeSrc}");
            var framePage = await context.NewPageAsync();
            await framePage.GotoAsync(iframeSrc!);

            await framePage.WaitForSelectorAsync(".team-stats-widget");

            var html = await framePage.ContentAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<string> tableMatchDivClasses = ["table", "match"];

            var tableMatchNode = doc.DocumentNode.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return tableMatchDivClasses.All(c => classes.Contains(c));
                });
            if (tableMatchNode is null)
            {
                Console.WriteLine("❌ Table match node not found");
                throw new InvalidOperationException("Table match node not found");
            }

            var allTableRows = tableMatchNode.Descendants("div")
                .Where(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("table-row") && !classes.Contains("table-header") && !classes.Contains("summary");
                }).ToList();

            var teamContainerDiv = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(div =>
               {
                   var classAttr = div.GetAttributeValue("class", "");
                   var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                   return classes.Contains("title-container");
               });

            var teamName = teamContainerDiv?.Descendants("h2")
                .FirstOrDefault()?.InnerText.Trim() ?? "Unknown Team Name";

            var matchTeamStatistics = new MatchTeamStatistics
            {
                Name = teamName,
            };

            foreach (var tableRow in allTableRows)
            {
                var playerNumber = GetPlayerNumber(tableRow);
                var playerName = GetPlayerName(tableRow);
                var pointsTotal = GetPlayerPointsTotal(tableRow);
                var serveTotal = GetPlayerServeTotal(tableRow);
                var serveErrors = GetPlayerServeErrors(tableRow);
                var serveAces = GetPlayerServeAces(tableRow);
                var receptionTotal = GetPlayerReceptionTotal(tableRow);
                var receptionErrors = GetPlayerReceptionErrors(tableRow);
                var receptionPositivePercent = GetPlayerReceptionPositivePercent(tableRow);
                var receptionPerfectPercent = GetPlayerReceptionPerfectPercent(tableRow);
                var attackTotal = GetPlayerAttackTotal(tableRow);
                var attackErrors = GetPlayerAttackErrors(tableRow);
                var attackBlocked = GetPlayerAttackBlocked(tableRow);
                var attackPoints = GetPlayerAttackPoints(tableRow);
                var attackEfficiencyPercent = GetPlayerAttackEfficiencyPercent(tableRow);
                var blocks = GetPlayerBlocks(tableRow);

                matchTeamStatistics.PlayersStatistics.Add(new PlayerMatchStatistics(
                    playerNumber,
                    playerName,
                    pointsTotal,
                    serveTotal,
                    serveErrors,
                    serveAces,
                    receptionTotal,
                    receptionErrors,
                    receptionPositivePercent,
                    receptionPerfectPercent,
                    attackTotal,
                    attackErrors,
                    attackBlocked,
                    attackPoints,
                    attackEfficiencyPercent,
                    blocks
                ));
            }

            return matchTeamStatistics;
        }      

        private static int GetPlayerNumber(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("player-number");
                })?.InnerText.Trim() ?? "-1");

        private static string GetPlayerName(HtmlNode tableRow) 
            => tableRow.Descendants("a")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("player-name");
                })?.InnerText.Trim() ?? "Unknown Player Name";

        private static int GetPlayerPointsTotal(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("sum") && classes.Contains("attack");
                })?.InnerText.Trim() ?? "-1");

        private static int GetPlayerServeTotal(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("sum") && !classes.Contains("attack");
                })?.InnerText.Trim() ?? "-1");

        private static int GetPlayerServeErrors(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("error");
                })?.InnerText.Trim() ?? "-1");

        private static int GetPlayerServeAces(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("ace");
                })?.InnerText.Trim() ?? "-1");

        private static int GetPlayerReceptionTotal(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .Where(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("sum");
                }).Skip(2).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static int GetPlayerReceptionErrors(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .Where(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("error");
                }).Skip(1).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static string GetPlayerReceptionPositivePercent(HtmlNode tableRow)
            => tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("pos-ratio");
                })?.InnerText.Trim() ?? "-1";

        private static string GetPlayerReceptionPerfectPercent(HtmlNode tableRow)
            => tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("perfect-ratio");
                })?.InnerText.Trim() ?? "-1";

        private static int GetPlayerAttackTotal(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .Where(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("sum");
                }).Skip(3).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static int GetPlayerAttackErrors(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .Where(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("error");
                }).Skip(2).FirstOrDefault()?.InnerText.Trim() ?? "-1");

        private static int GetPlayerAttackBlocked(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("block");
                })?.InnerText.Trim() ?? "-1");

        private static int GetPlayerAttackPoints(HtmlNode tableRow)
            => int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("pkt");
                })?.InnerText.Trim() ?? "-1");

        private static string GetPlayerAttackEfficiencyPercent(HtmlNode tableRow)
            => tableRow.Descendants("div")
                .Where(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("perfect-ratio");
                }).Skip(1).FirstOrDefault()?.InnerText.Trim() ?? "-1";

        private static int GetPlayerBlocks(HtmlNode tableRow) => 
            int.Parse(tableRow.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var classAttr = div.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains("pkt") && classes.Contains("block");
                })?.InnerText.Trim() ?? "-1");

        private static async Task<(int FirstTeamSetsWon, int SecondTeamSetsWon)> GetSetsWonByTeam(IPage page)
        {
            var html = await page.ContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var firstTeamSetsWon = GetSetsWon(doc, "teamAStatus");
            var secondTeamSetsWon = GetSetsWon(doc, "teamBStatus");

            return (firstTeamSetsWon, secondTeamSetsWon);
        }

        private static int GetSetsWon(HtmlDocument document, string teamStatus)
        {
            var setsWon = document.DocumentNode.Descendants("span")
                .FirstOrDefault(span =>
                {
                    var classAttr = span.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    return classes.Contains(teamStatus);
                })?.InnerText.Trim() ?? "-1";

            return int.Parse(setsWon);
        }
    }
}
