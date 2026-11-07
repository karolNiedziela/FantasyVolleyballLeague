using System.Globalization;
using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    internal static class PlsRosterParser
    {
        /// <summary>
        /// Clubs of a season from the teams page: <c>div.team-box-caption a[href="/teams/id/{clubId}/..."]</c>.
        /// </summary>
        public static IReadOnlyList<ClubLink> ParseClubs(HtmlDocument document)
        {
            return document.DocumentNode.FindDescendantsWithClass("div", "team-box-caption")
                .Select(caption => caption.Descendants("a")
                    .Select(a => (Id: PlsExternalIds.FromTeamUrl(a.GetAttributeValue("href", string.Empty)), Name: a.CleanText()))
                    .FirstOrDefault(link => link.Id is not null && link.Name.Length > 0))
                .Where(link => link.Id is not null)
                .Select(link => new ClubLink(link.Id!.Value, link.Name))
                .DistinctBy(club => club.ExternalId)
                .ToList();
        }

        /// <summary>
        /// Team page: every player is a <c>div.sorting-team-box.dc-slider-item</c> with a profile link,
        /// <c>h2</c> = shirt number, <c>h3</c> = "First Last", <c>h4</c> = position (Polish).
        /// </summary>
        public static PlsTeamPage ParseTeamPage(HtmlDocument document)
        {
            // The page has several section titles; the generic one ("Kluby") is an h1.title.
            var name = document.DocumentNode.FindDescendantsWithClass("div", "section-title")
                .SelectMany(title => title.Elements("h1"))
                .FirstOrDefault(h1 => !h1.HasClass("title"))
                ?.CleanText();

            var players = document.DocumentNode.FindDescendantsWithClass("div", "sorting-team-box", "dc-slider-item")
                .Select(ParseRosterEntry)
                .OfType<PlsRosterEntry>()
                .DistinctBy(player => player.ExternalId)
                .ToList();

            return new PlsTeamPage(string.IsNullOrEmpty(name) ? null : name, players);
        }

        /// <summary>
        /// Player page: <c>div.number</c> blocks identified by their label, e.g.
        /// <c>&lt;div class="number"&gt;Wzrost: &lt;span&gt;&lt;h3&gt;195&lt;/h3&gt;&lt;/span&gt;&lt;/div&gt;</c>.
        /// </summary>
        public static PlsPlayerDetails ParsePlayerProfile(HtmlDocument document)
        {
            var values = document.DocumentNode.FindDescendantsWithClass("div", "number")
                .Select(div => (Label: div.CleanText(), Value: div.Element("span")?.CleanText() ?? string.Empty))
                .ToList();

            string? GetValue(string label)
            {
                var value = values.FirstOrDefault(v => v.Label.StartsWith(label, StringComparison.OrdinalIgnoreCase)).Value;
                return string.IsNullOrEmpty(value) ? null : value;
            }

            var position = GetValue("Pozycja");

            return new PlsPlayerDetails(
                ParseDate(GetValue("Data urodzenia")),
                position is null ? null : PlsPositionMapper.Map(position),
                HtmlNodeExtensions.ParseIntOrNull(GetValue("Wzrost")),
                HtmlNodeExtensions.ParseIntOrNull(GetValue("Waga")),
                HtmlNodeExtensions.ParseIntOrNull(GetValue("Zasięg z wyskoku do ataku")),
                HtmlNodeExtensions.ParseIntOrNull(GetValue("Numer")));
        }

        /// <summary>
        /// Career table of the player page (<c>h2</c> "KARIERA ZAWODOWA W LIDZE" followed by a table). Every row is
        /// <c>from | to | position | number | team</c>, where the team is a link <c>/teams/id/{clubId}/tour/{tour}.html</c>
        /// that also tells which season the stint belongs to. Rows that cannot be read are skipped.
        /// </summary>
        public static IReadOnlyList<PlsCareerRow> ParseCareer(HtmlDocument document)
        {
            var title = document.DocumentNode.Descendants("h2")
                .FirstOrDefault(h2 => h2.CleanText().StartsWith("KARIERA ZAWODOWA", StringComparison.OrdinalIgnoreCase));
            var table = title?.ParentNode?.Descendants("table").FirstOrDefault();
            if (table is null)
            {
                return [];
            }

            return table.Descendants("tr")
                .Select(ParseCareerRow)
                .OfType<PlsCareerRow>()
                .ToList();
        }

        private static PlsCareerRow? ParseCareerRow(HtmlNode row)
        {
            var cells = row.Elements("td").ToArray();
            if (cells.Length < 5)
            {
                return null;
            }

            var teamUrl = cells[4].Descendants("a").FirstOrDefault()?.GetAttributeValue("href", string.Empty);
            var clubId = PlsExternalIds.FromTeamUrl(teamUrl);
            var tour = PlsExternalIds.FromTourUrl(teamUrl);
            var from = ParseDate(cells[0].CleanText());
            var to = ParseDate(cells[1].CleanText());
            if (clubId is null || tour is null || from is null || to is null)
            {
                return null;
            }

            var position = cells[2].CleanText();

            return new PlsCareerRow(
                clubId.Value,
                tour.Value,
                from.Value,
                to.Value,
                HtmlNodeExtensions.ParseIntOrNull(cells[3].CleanText()),
                string.IsNullOrEmpty(position) ? null : PlsPositionMapper.Map(position));
        }

        private static PlsRosterEntry? ParseRosterEntry(HtmlNode box)
        {
            var link = box.Descendants("a")
                .FirstOrDefault(a => PlsExternalIds.FromPlayerUrl(a.GetAttributeValue("href", string.Empty)) is not null);
            var fullName = box.Descendants("h3").FirstOrDefault()?.CleanText();
            if (link is null || string.IsNullOrEmpty(fullName))
            {
                return null;
            }

            var href = link.GetAttributeValue("href", string.Empty);
            var position = box.Descendants("h4").FirstOrDefault()?.CleanText();

            return new PlsRosterEntry(
                PlsExternalIds.FromPlayerUrl(href)!.Value,
                fullName,
                HtmlNodeExtensions.ParseIntOrNull(box.Descendants("h2").FirstOrDefault()?.InnerText),
                string.IsNullOrEmpty(position) ? null : PlsPositionMapper.Map(position),
                href);
        }

        private static DateOnly? ParseDate(string? text)
        {
            return DateOnly.TryParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : null;
        }
    }
}
