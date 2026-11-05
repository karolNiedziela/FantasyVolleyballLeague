using System.Globalization;
using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;
using static FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches.PlsScheduleConstants;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    /// <summary>
    /// Builds the season schedule from the static HTML of the season games page.
    /// The page always renders every phase, stage ("round" filter) and round ("term" filter) section,
    /// hidden with CSS, so no filter clicking is needed:
    /// <c>section[data-phase] &gt; section[data-round] (h3) &gt; section[data-term] (h4) &gt; [data-game-id]</c>.
    /// </summary>
    internal static class PlsSeasonScheduleParser
    {
        public static IReadOnlyList<ScheduleEntry> Parse(HtmlDocument document)
        {
            var sections = document.DocumentNode.Descendants("section")
                .Where(s => s.HasClass(CssClasses.FilterableContent) && !s.HasClass(CssClasses.AjaxSyncedGames))
                .ToList();

            var entries = new List<ScheduleEntry>();

            var phases = GetPhases(document);
            for (var phaseIndex = 0; phaseIndex < phases.Count; phaseIndex++)
            {
                var (phaseCode, phaseName) = phases[phaseIndex];
                var phase = new NamedOrder(phaseName, phaseIndex + 1);

                var stageSections = sections
                    .Where(s => s.GetAttributeValue(Attributes.Phase, "") == phaseCode
                        && s.Attributes[Attributes.Round] is not null
                        && s.Attributes[Attributes.Term] is null)
                    .OrderBy(s => GetIntAttribute(s, Attributes.Round))
                    .ToList();

                foreach (var stageSection in stageSections)
                {
                    var stageNumber = GetIntAttribute(stageSection, Attributes.Round);
                    var stage = stageSections.Count > 1
                        ? new NamedOrder(ExtractStageName(stageSection, stageNumber), stageNumber)
                        : new NamedOrder(string.Empty, 1);

                    var termSections = sections
                        .Where(s => s.GetAttributeValue(Attributes.Phase, "") == phaseCode
                            && GetIntAttribute(s, Attributes.Round) == stageNumber
                            && s.Attributes[Attributes.Term] is not null)
                        .OrderBy(s => GetIntAttribute(s, Attributes.Term))
                        .ToList();

                    foreach (var termSection in termSections)
                    {
                        var termNumber = GetIntAttribute(termSection, Attributes.Term);
                        var roundName = ExtractTermName(termSection)
                            ?? (stage.Name.Length > 0 ? stage.Name : phase.Name);
                        var round = new NamedOrder(roundName, termNumber);

                        entries.AddRange(GetMatchIds(termSection).Select(matchId => new ScheduleEntry(matchId, phase, stage, round)));
                    }
                }
            }

            return [.. entries.DistinctBy(entry => entry.MatchId)];
        }

        /// <summary>
        /// Normalises stage and round headings so that names match the legacy folder names,
        /// e.g. "Runda I: Ćwierćfinał (do 2 zwycięstw)" → "I Ćwierćfinał", "Para nr: 1" → "Para 1", "Kolejka: 1" → "Kolejka 1".
        /// </summary>
        public static string NormalizeHeading(string text)
        {
            var cleaned = HtmlNodeExtensions.CleanText(text);

            if (cleaned.StartsWith(Polish.RoundPrefix, StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned[Polish.RoundPrefix.Length..];
            }

            var parenIndex = cleaned.IndexOf('(', StringComparison.Ordinal);
            if (parenIndex > 0)
            {
                cleaned = cleaned[..parenIndex];
            }

            var words = cleaned.Replace(":", " ", StringComparison.Ordinal)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !w.Equals(Polish.NumberAbbreviation, StringComparison.OrdinalIgnoreCase));

            return string.Join(' ', words);
        }

        private static List<(string Code, string Name)> GetPhases(HtmlDocument document)
        {
            var phaseList = document.DocumentNode.Descendants("div")
                .FirstOrDefault(div => div.GetAttributeValue(Attributes.FilterListType, "") == FilterListTypes.Phase);

            return phaseList?.Descendants("button")
                .Select(b => (Code: b.GetAttributeValue(Attributes.FilterValue, ""), Name: b.CleanText()))
                .Where(p => p.Code.Length > 0
                    && !p.Code.Equals(Polish.AllFilterValue, StringComparison.OrdinalIgnoreCase)
                    && !p.Name.Contains(Polish.AllFilter, StringComparison.OrdinalIgnoreCase))
                .Select(p => (p.Code, PlsPhaseNameMapper.Map(p.Name)))
                .ToList() ?? [];
        }

        private static string ExtractStageName(HtmlNode stageSection, int stageNumber)
        {
            var heading = stageSection.Element("header")?.Element("h3")
                ?? stageSection.Descendants("h3").FirstOrDefault();
            var name = heading is null ? string.Empty : NormalizeHeading(heading.InnerText);

            return name.Length > 0 ? name : stageNumber.ToString(CultureInfo.InvariantCulture);
        }

        private static string? ExtractTermName(HtmlNode termSection)
        {
            var heading = termSection.Element("header")?.Element("h4")
                ?? termSection.Descendants("h4").FirstOrDefault();
            if (heading is null)
            {
                return null;
            }

            var name = NormalizeHeading(heading.InnerText);
            return name.Length > 0 ? name : null;
        }

        private static IEnumerable<int> GetMatchIds(HtmlNode termSection)
        {
            return termSection.Descendants()
                .Select(n => HtmlNodeExtensions.ParseIntOrNull(n.Attributes[Attributes.GameId]?.Value))
                .OfType<int>()
                .Distinct();
        }

        private static int GetIntAttribute(HtmlNode node, string attributeName)
        {
            return HtmlNodeExtensions.ParseIntOrNull(node.Attributes[attributeName]?.Value) ?? 0;
        }
    }
}
