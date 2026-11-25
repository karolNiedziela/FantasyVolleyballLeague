using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Tests.Fixtures
{
    /// <summary>
    /// Real pages of the PLS websites (see <c>.claude/rules/project-overview.md</c> for the eras), minimized with
    /// <c>tools/fixtures/Minimize-PlsFixtures.ps1</c> to what the parsers read. The repository is public, so a new page
    /// is always minimized before it is committed (PLS terms of use, copyrighted news, personal data).
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
