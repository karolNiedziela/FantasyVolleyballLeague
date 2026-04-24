using FantasyVolleyballLeague.Worker.StatisticsScrapper;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;
using static FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga.PlusligaSeasonMatchScrapperConstants;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga
{
    public sealed class PlusligaSeasonMatchScrapper : ISeasonMatchScrapper
    {
        private const int MaxConcurrentMatches = 3;

        private readonly IMatchStatisticsScrapper _matchStatisticsScrapper;

        public PlusligaSeasonMatchScrapper(IMatchStatisticsScrapper matchStatisticsScrapper)
        {
            _matchStatisticsScrapper = matchStatisticsScrapper;
        }

        public async Task<IReadOnlyList<SeasonPhaseStatistics>> GetAllPhasesMatchStatisticsAsync(
            IPage page, PlaywrightSession session)
        {
            var html = await page.ContentAsync();
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var phaseButtons = GetPhaseButtons(document).ToArray();
            if (phaseButtons.Length == 0)
            {
                Console.WriteLine("No phase buttons found in document.");
                return [];
            }

            await PlaywrightUtilities.TryClickDenyCookiesAsync(session);

            var result = new List<SeasonPhaseStatistics>();

            foreach (var button in phaseButtons)
            {
                var phaseName = button.InnerText.Trim();
                var dataPhase = button.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.FilterValue, "");

                Console.WriteLine($"Processing phase: {phaseName}");

                try
                {
                    await page.Locator($"button[{PlusligaSeasonMatchScrapperConstants.Attributes.FilterValue}='{dataPhase}']").ClickAsync();
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"Button with filter '{dataPhase}' was not found in time. Skipping phase.");
                    continue;
                }

                var updatedHtml = await page.ContentAsync();
                var updatedDocument = new HtmlDocument();
                updatedDocument.LoadHtml(updatedHtml);

                var termFilterType = GetVisibleFilterListType(updatedDocument, dataPhase);
                if (termFilterType is null)
                {
                    Console.WriteLine($"No visible filter list type found for phase {phaseName}. Skipping.");
                    continue;
                }

                List<SeasonPhaseStage> stages;

                if (termFilterType == PlusligaSeasonMatchScrapperConstants.FilterListTypes.Round)
                {
                    stages = await ScrapeRoundThenTermsAsync(page, session, updatedDocument, dataPhase);
                }
                else
                {
                    var stageName = GetVisibleStageName(updatedDocument, dataPhase);
                    var rounds = await ScrapeTermRoundsAsync(page, session, updatedDocument, termFilterType, dataPhase);
                    stages = rounds.Count > 0 ? [new SeasonPhaseStage(1, stageName, rounds)] : [];
                }

                if (stages.Count > 0)
                {
                    result.Add(new SeasonPhaseStatistics(PhaseNameMapper.Map(phaseName), stages));
                }
            }

            return result;
        }

        private async Task<List<SeasonPhaseStage>> ScrapeRoundThenTermsAsync(
            IPage page, PlaywrightSession session, HtmlDocument document, string dataPhase)
        {
            var stages = new List<SeasonPhaseStage>();
            var roundValues = GetFilterValues(document, PlusligaSeasonMatchScrapperConstants.FilterListTypes.Round, dataPhase);

            foreach (var roundValue in roundValues)
            {
                await page.Locator(
                    $"div[{PlusligaSeasonMatchScrapperConstants.Attributes.FilterListType}='{PlusligaSeasonMatchScrapperConstants.FilterListTypes.Round}']" +
                    $"[{PlusligaSeasonMatchScrapperConstants.Attributes.Phase}='{dataPhase}'] " +
                    $"button[{PlusligaSeasonMatchScrapperConstants.Attributes.FilterValue}='{roundValue}']").ClickAsync();
                var roundHtml = await page.ContentAsync();
                var roundDocument = new HtmlDocument();
                roundDocument.LoadHtml(roundHtml);

                var stageName = GetVisibleStageName(roundDocument, dataPhase);
                var termRounds = await ScrapeTermRoundsAsync(page, session, roundDocument, PlusligaSeasonMatchScrapperConstants.FilterListTypes.Term, dataPhase);
                if (termRounds.Count == 0)
                {
                    termRounds = await ScrapeWithoutTermsAsync(page, session, roundDocument, dataPhase);
                }

                if (termRounds.Count > 0)
                {
                    stages.Add(new SeasonPhaseStage(roundValue, stageName, termRounds));
                }
            }

            return stages;
        }

        private async Task<List<SeasonPhaseRound>> ScrapeTermRoundsAsync(
            IPage page, PlaywrightSession session, HtmlDocument document, string filterListType, string dataPhase)
        {
            var rounds = new List<SeasonPhaseRound>();
            var filterValues = GetFilterValues(document, filterListType, dataPhase);
         
            foreach (var filterValue in filterValues)
            {
                await page.Locator(
                    $"div[{PlusligaSeasonMatchScrapperConstants.Attributes.FilterListType}='{filterListType}']" +
                    $"[{PlusligaSeasonMatchScrapperConstants.Attributes.Phase}='{dataPhase}'] " +
                    $"button[{PlusligaSeasonMatchScrapperConstants.Attributes.FilterValue}='{filterValue}']").ClickAsync();
                var termHtml = await page.ContentAsync();
                var termDocument = new HtmlDocument();
                termDocument.LoadHtml(termHtml);

                var visibleTermSection = GetVisibleTermSection(termDocument);
                var termName = visibleTermSection is not null ? ExtractTermName(visibleTermSection) : null;

                var matchIds = GetMatchIds(termDocument);
                if (matchIds.Count == 0)
                {
                    Console.WriteLine($"No matches found for term {filterValue} in phase {dataPhase}. Skipping term.");
                    continue;
                }

                var matches = await ProcessMatchIdsAsync(session, matchIds);
                Console.WriteLine($"Term {filterValue}: {matches.Count} matches scraped.");
                if (matches.Count > 0)
                {
                    rounds.Add(new SeasonPhaseRound(filterValue, termName, matches));
                }
            }

            return rounds;
        }

        private async Task<List<SeasonPhaseRound>> ScrapeWithoutTermsAsync(IPage page, PlaywrightSession session, HtmlDocument document, string dataPhase)
        {
            var rounds = new List<SeasonPhaseRound>();

            var matchIds = GetMatchIds(document);
            if (matchIds.Count == 0)
            {
                Console.WriteLine($"No matches found for in phase {dataPhase}. Skipping term.");
                return rounds;
            }

            var matches = await ProcessMatchIdsAsync(session, matchIds);
            Console.WriteLine($"{matches.Count} matches scraped.");

            if (matches.Count > 0)
            {
                rounds.Add(new SeasonPhaseRound(1, dataPhase, matches));
            }

            return rounds;
        }

        private static IEnumerable<HtmlNode> GetPhaseButtons(HtmlDocument document)
        {
            var buttons = document.DocumentNode.Descendants("div")
                .FirstOrDefault(div =>
                {
                    var attr = div.Attributes
                        .FirstOrDefault(a => a.Name.Equals(PlusligaSeasonMatchScrapperConstants.Attributes.FilterListType, StringComparison.OrdinalIgnoreCase));
                    return attr != null && attr.Value.Equals(PlusligaSeasonMatchScrapperConstants.FilterListTypes.Phase, StringComparison.Ordinal);
                })?.Descendants("button");

            return (buttons ?? []).Where(b =>
                !b.InnerText.Contains(PlusligaSeasonMatchScrapperConstants.Polish.AllFilter, StringComparison.OrdinalIgnoreCase) &&
                !b.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.FilterValue, "").Equals(PlusligaSeasonMatchScrapperConstants.Polish.AllFilterValue, StringComparison.OrdinalIgnoreCase));
        }

        private static string? GetVisibleFilterListType(HtmlDocument document, string phase)
        {
            return document.DocumentNode.Descendants("div")
                .Where(div =>
                    div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Phase, "") == phase &&
                    div.Attributes[PlusligaSeasonMatchScrapperConstants.Attributes.FilterListType] != null &&
                    div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.FilterListType, "") != PlusligaSeasonMatchScrapperConstants.FilterListTypes.Phase &&
                    !div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNone, StringComparison.OrdinalIgnoreCase) &&
                    !div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNoneWithSpace, StringComparison.OrdinalIgnoreCase))
                .Select(div => div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.FilterListType, ""))
                .FirstOrDefault();
        }

        private static List<int> GetFilterValues(HtmlDocument document, string filterListType, string phase)
        {
            var container = document.DocumentNode.Descendants("div")
                .FirstOrDefault(div =>
                    div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.FilterListType, "") == filterListType &&
                    div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Phase, "") == phase &&
                    !div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNone, StringComparison.OrdinalIgnoreCase) &&
                    !div.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNoneWithSpace, StringComparison.OrdinalIgnoreCase));

            if (container is null)
            {
                return [];
            }

            return container.Descendants("button")
                .Select(b => b.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.FilterValue, ""))
                .Where(v => int.TryParse(v, out _))
                .Select(int.Parse)
                .ToList();
        }

        private static string ExtractStageName(HtmlNode stageSection)
        {
            var h3 = stageSection.Descendants("h3").FirstOrDefault();
            if (h3 is null)
            {
                return stageSection.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Round, PlusligaSeasonMatchScrapperConstants.DefaultStageName);
            }

            var text = h3.InnerText.Trim();
            if (text.StartsWith(PlusligaSeasonMatchScrapperConstants.Polish.RoundPrefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text[PlusligaSeasonMatchScrapperConstants.Polish.RoundPrefix.Length..].Trim();
            }

            var parenIndex = text.IndexOf('(', StringComparison.Ordinal);
            if (parenIndex > 0)
            {
                text = text[..parenIndex].Trim();
            }

            return string.IsNullOrWhiteSpace(text)
                ? stageSection.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Round, PlusligaSeasonMatchScrapperConstants.DefaultStageName)
                : text;
        }

        private static string? ExtractTermName(HtmlNode termSection)
        {
            var h4 = termSection.Descendants("h4").FirstOrDefault();
            if (h4 is null)
            {
                return null;
            }

            var text = h4.InnerText.Trim();
            text = text.Replace(PlusligaSeasonMatchScrapperConstants.Polish.TermNumberPrefix, "", StringComparison.OrdinalIgnoreCase)
                       .Replace(PlusligaSeasonMatchScrapperConstants.Polish.TermNumberPrefixShort, "", StringComparison.OrdinalIgnoreCase)
                       .Trim();

            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private async Task<List<MatchRecord>> ProcessMatchIdsAsync(
            PlaywrightSession session, List<int> matchIds)
        {
            if (matchIds.Count == 0)
            {
                return [];
            }

            using var semaphore = new SemaphoreSlim(MaxConcurrentMatches);

            var tasks = matchIds.Select(async matchId =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await _matchStatisticsScrapper.GetMatchStatisticsAsync(matchId, session);
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    Console.WriteLine($"Failed to scrape match {matchId}: {ex.Message}. Skipping.");
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            return [.. results.OfType<MatchRecord>()];
        }

        private static List<int> GetMatchIds(HtmlDocument document)
        {
            var visibleTermSection = document.DocumentNode.Descendants("section")
                .FirstOrDefault(section =>
                    section.HasClass(PlusligaSeasonMatchScrapperConstants.CssClasses.FilterableContent) &&
                    section.Attributes[PlusligaSeasonMatchScrapperConstants.Attributes.Term] != null &&
                    !section.HasClass(PlusligaSeasonMatchScrapperConstants.CssClasses.AjaxSyncedGames) &&
                    !section.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNone, StringComparison.OrdinalIgnoreCase) &&
                    !section.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNoneWithSpace, StringComparison.OrdinalIgnoreCase));

            if (visibleTermSection is null)
            {
                return [];
            }

            return visibleTermSection.Descendants("section")
               .Where(section => section.HasClass(PlusligaSeasonMatchScrapperConstants.CssClasses.AjaxSyncedGames))
               .Select(game => game.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.GameId, string.Empty))
               .Select(int.Parse)
               .ToList();
        }

        private static string GetVisibleStageName(HtmlDocument document, string phase)
        {
            var stageSection = document.DocumentNode.Descendants("section")
                .FirstOrDefault(s =>
                    s.HasClass(PlusligaSeasonMatchScrapperConstants.CssClasses.FilterableContent) &&
                    s.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Phase, "") == phase &&
                    s.Attributes[PlusligaSeasonMatchScrapperConstants.Attributes.Round] != null &&
                    s.Attributes[PlusligaSeasonMatchScrapperConstants.Attributes.Term] == null &&
                    !s.HasClass(PlusligaSeasonMatchScrapperConstants.CssClasses.AjaxSyncedGames) &&
                    !s.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNone, StringComparison.OrdinalIgnoreCase) &&
                    !s.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNoneWithSpace, StringComparison.OrdinalIgnoreCase));

            return stageSection is not null ? ExtractStageName(stageSection) : phase;
        }

        private static HtmlNode? GetVisibleTermSection(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("section")
                .FirstOrDefault(s =>
                    s.HasClass(PlusligaSeasonMatchScrapperConstants.CssClasses.FilterableContent) &&
                    s.Attributes[PlusligaSeasonMatchScrapperConstants.Attributes.Term] != null &&
                    !s.HasClass(PlusligaSeasonMatchScrapperConstants.CssClasses.AjaxSyncedGames) &&
                    !s.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNone, StringComparison.OrdinalIgnoreCase) &&
                    !s.GetAttributeValue(PlusligaSeasonMatchScrapperConstants.Attributes.Style, "").Contains(PlusligaSeasonMatchScrapperConstants.DisplayStyles.DisplayNoneWithSpace, StringComparison.OrdinalIgnoreCase));
        }
    }
}
