using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Tests.Fixtures
{
    /// <summary>
    /// Real pages of tauronliga.pl saved on 2026-10-18 (see <c>.claude/rules/project-overview.md</c> for the eras).
    /// </summary>
    internal static class Fixture
    {
        public static HtmlDocument Load(string fileName)
        {
            var document = new HtmlDocument();
            document.LoadHtml(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName)));
            return document;
        }
    }
}
