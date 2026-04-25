using FantasyVolleyballLeague.Worker.TeamScrappers.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Worker.Services
{
    public sealed class SeasonScrapper : ISeasonScrapper
    {
        private readonly PlaywrightFactory _playwrightFactory;

        public SeasonScrapper(PlaywrightFactory playwrightFactory)
        {
            _playwrightFactory = playwrightFactory;
        }

        public async Task<IEnumerable<Season>> GetSeasonsAsync(LeagueOptions leagueOptions, PlaywrightSession? session = null)
        {
            await using var ownedSession = session is null
                ? await _playwrightFactory.SetupBrowserContextAsync()
                : null;
            var activeSession = session ?? ownedSession!;

            var page = await activeSession.Context.NewPageAsync();
            await page.GotoAsync(new Uri(leagueOptions.GamesUrl).AbsoluteUri);

            var html = await page.ContentAsync();
            await page.CloseAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var seasons = new List<Season>();
            foreach (var season in GetSeasonLinks(doc))
            {
                var seasonYears = season.Name.Split(' ')[1];
                var parts = seasonYears.Split('/');
                var startYear = int.Parse(parts[0]);
                var endYear = int.Parse(parts[1]);
                var url = new Uri(leagueOptions.Url + season.Href);

                seasons.Add(new Season(season.Name, startYear, endYear, url));
            }

            return seasons;
        }

        public async Task<IEnumerable<Season>> GetTeamSeasonsAsync(LeagueOptions leagueOptions, PlaywrightSession? session = null)
        {
            await using var ownedSession = session is null
                ? await _playwrightFactory.SetupBrowserContextAsync()
                : null;
            var activeSession = session ?? ownedSession!;

            var page = await activeSession.Context.NewPageAsync();
            await page.GotoAsync(new Uri(leagueOptions.TeamsUrl).AbsoluteUri);

            var html = await page.ContentAsync();
            await page.CloseAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var seasons = new List<Season>();
            foreach (var season in GetSeasonLinks(doc))
            {
                var seasonYears = season.Name.Split(' ')[1];
                var parts = seasonYears.Split('/');
                var startYear = int.Parse(parts[0]);
                var endYear = int.Parse(parts[1]);
                var url = new Uri(leagueOptions.Url + season.Href);

                seasons.Add(new Season(season.Name, startYear, endYear, url));
            }

            return seasons;
        }

        private static List<SeasonLink> GetSeasonLinks(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants("ul")
                .FirstOrDefault(ul => ul.HasClass("dropdown-menu", "dropdown-menu-select"))
                ?.Descendants("a")
                .Select(x => new SeasonLink(
                    x.Attributes.First(a => a.Name == "href").Value,
                    x.InnerText.Trim()))
                .ToList() ?? [];
        }
    }
}
