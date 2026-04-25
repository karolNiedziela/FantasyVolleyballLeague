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

        public async Task<IEnumerable<SeasonInformation>> GetSeasons(LeagueOptions leagueOptions, PlaywrightSession? session = null)
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

            var seasonInformationList = new List<SeasonInformation>();
            foreach (var season in GetSeasonBrowserData(doc))
            {
                var seasonYears = season.Name.Split(' ')[1];
                var parts = seasonYears.Split('/');
                var startYear = int.Parse(parts[0]);
                var endYear= int.Parse(parts[1]);
                var url = new Uri(leagueOptions.Url + season.Href);

                seasonInformationList.Add(new SeasonInformation(season.Name, startYear, endYear, url));
            }

            return seasonInformationList;
        }

        private static List<SeasonBrowserData> GetSeasonBrowserData(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants("ul")
                .FirstOrDefault(ul => ul.HasClass("dropdown-menu", "dropdown-menu-select"))
                ?.Descendants("a")
                .Select(x => new SeasonBrowserData(
                    x.Attributes.First(a => a.Name == "href").Value,
                    x.InnerText.Trim()))
                .ToList() ?? [];
        }
    }
}
