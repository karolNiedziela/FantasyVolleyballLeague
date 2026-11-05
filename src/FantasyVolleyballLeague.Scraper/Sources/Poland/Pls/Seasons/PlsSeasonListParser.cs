using System.Globalization;
using System.Text.RegularExpressions;
using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Seasons
{
    internal static partial class PlsSeasonListParser
    {
        /// <summary>
        /// Parses the season dropdown (<c>ul.dropdown-menu-select</c>), e.g.
        /// <c>&lt;a href="/games/tour/53/nocookies/1.html"&gt;Sezon 2026/2027&lt;/a&gt;</c>.
        /// </summary>
        public static IReadOnlyList<Season> Parse(HtmlDocument doc, string baseUrl)
        {
            var links = doc.DocumentNode.Descendants("ul")
                .FirstOrDefault(ul => ul.HasClass("dropdown-menu", "dropdown-menu-select"))
                ?.Descendants("a")
                .Select(a => new PlsSeasonLink(a.GetAttributeValue("href", string.Empty), a.CleanText()))
                .Where(link => !string.IsNullOrEmpty(link.Href))
                .ToList() ?? [];

            var seasons = new List<Season>();
            foreach (var link in links)
            {
                var match = SeasonYearsRegex().Match(link.Name);
                if (!match.Success)
                {
                    continue;
                }

                var startYear = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var endYear = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                var url = new Uri(new Uri(baseUrl), link.Href);

                seasons.Add(new Season(link.Name, startYear, endYear, PlsExternalIds.FromTourUrl(link.Href), url));
            }

            return [.. seasons.OrderByDescending(s => s.StartYear)];
        }

        [GeneratedRegex(@"(\d{4})\s*/\s*(\d{4})")]
        private static partial Regex SeasonYearsRegex();
    }
}
