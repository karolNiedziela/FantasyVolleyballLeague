using FantasyVolleyballLeague.Worker.TeamScrappers.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.TeamScrappers.Plusliga
{
    public sealed class PlusligaTeamScrapper : ITeamScrapper
    {
        public async Task<List<TeamInformation>> GetTeamDataAsync()
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

            await page.GotoAsync("https://www.plusliga.pl/teams.html");

            var html = await page.ContentAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var seasons = doc.DocumentNode.Descendants("ul")
                .First(ul =>
                {
                    var classAttribute = ul.GetAttributeValue("class", string.Empty);
                    var classes = classAttribute.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                    return classes.Contains("dropdown-menu") && classes.Contains("dropdown-menu-select");
                })
                .Descendants("a")
                .Select(x => new SeasonInformation(
                    x.Attributes.First(x => x.Name == "href").Value,
                    x.InnerText.Trim()))
                .ToList();

            var teams = doc.DocumentNode.Descendants("div")
                .Where(div =>
                {
                    var classAttribute = div.GetAttributeValue("class", string.Empty);

                    var classes = classAttribute.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                    return classes.Contains("team-box-caption");
                }).ToList();

            var teamDetailsLinks = teams.Select(team => team.Descendants("a")
                .FirstOrDefault(a => a.GetAttributeValue("href", string.Empty).Contains("/teams/"))                
                ?.GetAttributeValue("href", string.Empty))
                .ToList();

            var teamInformationList = new List<TeamInformation>();

            foreach (var teamDetailsLink in teamDetailsLinks)
            {
                if (string.IsNullOrEmpty(teamDetailsLink))
                {
                    continue;
                }            

                var teamInformation = await GetTeamPlayersAsync(page, context, teamDetailsLink);
            }

            return teamInformationList;
        }

        private static async Task<TeamInformation> GetTeamPlayersAsync(IPage page, IBrowserContext context, string teamDetailsLinkSuffix)
        {
            page = await context.NewPageAsync();

            await page.GotoAsync($"https://www.plusliga.pl{teamDetailsLinkSuffix}");

            var html = await page.ContentAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var players = doc.DocumentNode.Descendants("div")
                .Where(div =>
                {
                    var classAttribute = div.GetAttributeValue("class", string.Empty);
                    var classes = classAttribute.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                    return classes.Contains("sorting-team-box") && classes.Contains("dc-slider-item");
                }).ToList();

            var teamName = doc.DocumentNode.Descendants("div").FirstOrDefault(div =>
            {
                var classAttribute = div.GetAttributeValue("class", string.Empty);

                var classes = classAttribute.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                return classes.Contains("section-background");
            })?.Descendants("h1").FirstOrDefault()?.InnerText.Trim() ?? string.Empty;


            var teamInformation = new TeamInformation
            {
                Name = teamName,
                Players = await GetPlayersInformationAsync(page, context, players)
            };

            return teamInformation;
        }

        private static async Task<List<PlayerInformation>> GetPlayersInformationAsync(
            IPage page, 
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
                var playerInformation = await GetSinglePlayerDataAsync(page, context, playerFullName, playerDetailsLink);
                playerInformationList.Add(playerInformation);
            }
            return playerInformationList;
        }

        private static async Task<PlayerInformation> GetSinglePlayerDataAsync(
            IPage page, 
            IBrowserContext context, 
            string playerFullName,
            string playerDetailsLinkSuffix)
        {
            page = await context.NewPageAsync();
            await page.GotoAsync($"https://www.plusliga.pl{playerDetailsLinkSuffix}");
            var html = await page.ContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            var numberDivs = doc.DocumentNode.Descendants("div")
                .Where(div => div.GetAttributeValue("class", string.Empty).Contains("number")).ToList();

            var dateOfBirth = numberDivs[0].Descendants("h3").First().InnerText.Trim();
            var position = numberDivs[1].Descendants("h3").First().InnerText.Trim();
            var height = int.Parse(numberDivs[2].Descendants("h3").First().InnerText.Trim());
            var weight = int.Parse(numberDivs[3].Descendants("h3").First().InnerText.Trim());
            var attackRange = int.Parse(numberDivs[4].Descendants("h3").First().InnerText.Trim());
            var shirtNumber = int.Parse(numberDivs[5].Descendants("span").First().InnerText.Trim());

            var playerInformation = new PlayerInformation(playerFullName, dateOfBirth, height, weight, attackRange, shirtNumber);
            
            return playerInformation;
        }
    }
}
