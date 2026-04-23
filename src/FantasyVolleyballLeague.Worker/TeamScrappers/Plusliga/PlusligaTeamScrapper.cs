using FantasyVolleyballLeague.Worker.TeamScrappers.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.TeamScrappers.Plusliga
{
    public sealed class PlusligaTeamScrapper : ITeamScrapper
    {
        private readonly PlaywrightFactory _playwrightFactory;

        public PlusligaTeamScrapper(PlaywrightFactory playwrightFactory)
        {
            _playwrightFactory = playwrightFactory;
        }

        public async Task<IEnumerable<TeamInformation>> GetTeamDataAsync(Uri pageUrl, PlaywrightSession? session = null)
        {
            await using var ownedSession = session is null
                ? await _playwrightFactory.SetupBrowserContextAsync()
                : null;
            var activeSession = session ?? ownedSession!;

            var context = activeSession.Context;
            var page = await context.NewPageAsync();

            await page.GotoAsync(pageUrl.ToString());

            var html = await page.ContentAsync();
            await page.CloseAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return await GetTeamsInformationAsync(context, doc);
        }

        private async static Task<IEnumerable<TeamInformation>> GetTeamsInformationAsync(IBrowserContext context, HtmlDocument doc)
        {
            var teams = doc.DocumentNode.Descendants("div")
               .Where(div => div.HasClass("team-box-caption"))
               .ToList();

            var teamDetailsLinks = teams.Select(team => team.Descendants("a")
                .FirstOrDefault(a => a.GetAttributeValue("href", string.Empty).Contains("/teams/"))
                ?.GetAttributeValue("href", string.Empty))
                .ToList();

            var teamInformationList = new List<TeamInformation>();

            foreach (var teamDetailsLink in teamDetailsLinks)
            {
                Console.WriteLine($"Processing {teamDetailsLink})");
                if (string.IsNullOrEmpty(teamDetailsLink))
                {
                    continue;
                }

                var teamInformation = await GetTeamPlayersAsync(context, teamDetailsLink);
                teamInformationList.Add(teamInformation);

                Console.WriteLine($"Processed {teamDetailsLink})");
            }

            return teamInformationList;
        }

        private static async Task<TeamInformation> GetTeamPlayersAsync(IBrowserContext context, string teamDetailsLinkSuffix)
        {
            var page = await context.NewPageAsync();

            await page.GotoAsync($"https://www.plusliga.pl{teamDetailsLinkSuffix}");

            var html = await page.ContentAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var players = doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("sorting-team-box", "dc-slider-item"))
                .ToList();

            var teamName = doc.DocumentNode.FindDescendantWithClass("div", "section-background")
                ?.Descendants("h1").FirstOrDefault()?.InnerText.Trim() ?? string.Empty;


            var teamInformation = new TeamInformation
            {
                Name = teamName,
                Players = await GetPlayersInformationAsync(context, players)
            };

            await page.CloseAsync();

            return teamInformation;
        }

        private static async Task<List<PlayerInformation>> GetPlayersInformationAsync(
            IBrowserContext context, 
            List<HtmlNode> players)
        {
            var playerInformationList = new List<PlayerInformation>();
            foreach (var player in players)
            {
                var playerDetailsLink = player.Descendants("a")
                    .FirstOrDefault(a => a.GetAttributeValue("href", string.Empty).Contains("/players/"))?
                    .GetAttributeValue("href", string.Empty);
                
                var playerFullName = player.Descendants("h3").FirstOrDefault()?.InnerText.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(playerDetailsLink) || string.IsNullOrEmpty(playerFullName))
                {
                    continue;
                }
                var playerInformation = await GetSinglePlayerDataAsync(context, playerFullName, playerDetailsLink);
                playerInformationList.Add(playerInformation);
            }
            return playerInformationList;
        }

        private static async Task<PlayerInformation> GetSinglePlayerDataAsync(
            IBrowserContext context, 
            string playerFullName,
            string playerDetailsLinkSuffix)
        {
            var page = await context.NewPageAsync();
            Console.WriteLine($"Processing player {playerDetailsLinkSuffix}");
            await page.GotoAsync($"https://www.plusliga.pl{playerDetailsLinkSuffix}");
            var html = await page.ContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            var numberDivs = doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("number"))
                .ToList();

            var dateOfBirth = GetNumberDivH3(numberDivs, 0) ?? string.Empty;
            var position = GetNumberDivH3(numberDivs, 1) ?? string.Empty;
            var height = ParseNumberDivInt(numberDivs, 2, "h3");
            var weight = ParseNumberDivInt(numberDivs, 3, "h3");
            var attackRange = ParseNumberDivInt(numberDivs, 4, "h3");
            var shirtNumber = ParseNumberDivInt(numberDivs, 5, "span");

            var playerInformation = new PlayerInformation(playerFullName, position, dateOfBirth, height, weight, attackRange, shirtNumber, playerDetailsLinkSuffix);

            Console.WriteLine($"Processed player {playerDetailsLinkSuffix}");

            await page.CloseAsync();

            return playerInformation;
        }

        private static string? GetNumberDivH3(List<HtmlNode> divs, int index)
            => divs.ElementAtOrDefault(index)?.Descendants("h3").FirstOrDefault()?.InnerText.Trim();

        private static int ParseNumberDivInt(List<HtmlNode> divs, int index, string childTag)
        {
            var text = divs.ElementAtOrDefault(index)?.Descendants(childTag).FirstOrDefault()?.InnerText.Trim();
            return int.TryParse(text, out var value) ? value : -1;
        }
    }
}
