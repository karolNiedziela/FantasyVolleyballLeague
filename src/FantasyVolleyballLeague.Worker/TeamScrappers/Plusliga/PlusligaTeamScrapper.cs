using FantasyVolleyballLeague.Worker.TeamScrappers.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Worker.TeamScrappers.Plusliga
{
    public sealed class PlusligaTeamScrapper : ITeamScrapper
    {
        private const int MaxConcurrentTeams = 2;
        private const int MaxConcurrentPlayers = 3;

        private readonly PlaywrightFactory _playwrightFactory;

        public PlusligaTeamScrapper(PlaywrightFactory playwrightFactory)
        {
            _playwrightFactory = playwrightFactory;
        }

        public async Task<IEnumerable<TeamRoster>> GetTeamRosterAsync(Uri pageUrl, string baseUrl, PlaywrightSession? session = null)
        {
            await using var ownedSession = session is null
                ? await _playwrightFactory.SetupBrowserContextAsync()
                : null;
            var activeSession = session ?? ownedSession!;

            var context = activeSession.Context;
            var page = await context.NewPageAsync();

            await page.GotoAsync(pageUrl.ToString());

            await PlaywrightUtilities.TryClickDenyCookiesAsync(activeSession);

            var html = await page.ContentAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return await GetRostersAsync(context, doc, baseUrl);
        }

        private static async Task<IEnumerable<TeamRoster>> GetRostersAsync(IBrowserContext context, HtmlDocument doc, string baseUrl)
        {
            var validTeamLinks = doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("team-box-caption"))
                .Select(team => team.Descendants("a")
                    .FirstOrDefault(a => a.GetAttributeValue("href", string.Empty).Contains("/teams/"))
                    ?.GetAttributeValue("href", string.Empty))
                .Where(link => !string.IsNullOrEmpty(link))
                .ToList();

            using var semaphore = new SemaphoreSlim(MaxConcurrentTeams);
            var tasks = validTeamLinks.Select(async link =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Console.WriteLine($"Processing {link}");
                    var roster = await GetRosterAsync(context, link!, baseUrl);
                    Console.WriteLine($"Processed {link}");
                    return roster;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return [.. await Task.WhenAll(tasks)];
        }

        private static async Task<TeamRoster> GetRosterAsync(IBrowserContext context, string teamDetailsLinkSuffix, string baseUrl)
        {
            var page = await context.NewPageAsync();

            await page.GotoAsync($"{baseUrl}{teamDetailsLinkSuffix}");

            var html = await page.ContentAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var players = doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("sorting-team-box", "dc-slider-item"))
                .ToList();

            var teamName = doc.DocumentNode.FindDescendantWithClass("div", "section-background")
                ?.Descendants("h1").FirstOrDefault()?.InnerText.Trim() ?? string.Empty;

            var roster = new TeamRoster
            {
                Name = teamName,
                Players = await GetPlayersAsync(context, players, baseUrl)
            };

            await page.CloseAsync();

            return roster;
        }


        private static async Task<List<PlayerProfile>> GetPlayersAsync(
            IBrowserContext context,
            List<HtmlNode> players,
            string baseUrl)
        {
            var validPlayers = players
                .Select(player => (
                    Link: player.Descendants("a")
                        .FirstOrDefault(a => a.GetAttributeValue("href", string.Empty).Contains("/players/"))?
                        .GetAttributeValue("href", string.Empty),
                    FullName: player.Descendants("h3").FirstOrDefault()?.InnerText.Trim() ?? string.Empty))
                .Where(p => !string.IsNullOrEmpty(p.Link) && !string.IsNullOrEmpty(p.FullName))
                .ToList();

            using var semaphore = new SemaphoreSlim(MaxConcurrentPlayers);
            var tasks = validPlayers.Select(async p =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetPlayerAsync(context, p.FullName, p.Link!, baseUrl);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return [.. await Task.WhenAll(tasks)];
        }

        private static async Task<PlayerProfile> GetPlayerAsync(
            IBrowserContext context,
            string playerFullName,
            string playerDetailsLinkSuffix,
            string baseUrl)
        {
            var page = await context.NewPageAsync();
            Console.WriteLine($"Processing player {playerDetailsLinkSuffix}");
            await page.GotoAsync($"{baseUrl}{playerDetailsLinkSuffix}");
            var html = await page.ContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var numberDivs = doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("number"))
                .ToList();

            var dateOfBirth = GetNumberDivH3(numberDivs, 0) ?? string.Empty;
            var position = PositionMapper.Map(GetNumberDivH3(numberDivs, 1) ?? string.Empty);
            var height = ParseNumberDivInt(numberDivs, 2, "h3");
            var weight = ParseNumberDivInt(numberDivs, 3, "h3");
            var attackReach = ParseNumberDivInt(numberDivs, 4, "h3");
            var shirtNumber = ParseNumberDivInt(numberDivs, 5, "span");

            var profile = new PlayerProfile(playerFullName, position, dateOfBirth, height, weight, attackReach, shirtNumber, playerDetailsLinkSuffix);

            Console.WriteLine($"Processed player {playerDetailsLinkSuffix}");

            await page.CloseAsync();

            return profile;
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
