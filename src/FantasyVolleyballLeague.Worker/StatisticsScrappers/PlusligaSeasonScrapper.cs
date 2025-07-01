using FantasyVolleyballLeague.Worker.StatisticsScrapper;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers
{
    public sealed class PlusligaSeasonScrapper : ISeasonScrapper
    {
        private readonly IMatchStatisticsScrapper matchStatisticsScrapper;

        public PlusligaSeasonScrapper(IMatchStatisticsScrapper matchStatisticsScrapper)
        {
            this.matchStatisticsScrapper = matchStatisticsScrapper;
        }

        public async Task<Dictionary<int, List<SeasonMatchWeekGameStatistics>>> GetSeasonStatisticsAsync()
        {
            var url = "https://www.plusliga.pl/games.html";
            var web = new HtmlWeb();
            var doc = web.Load(new Uri(url));

            var parentSectionWithMainPhase = GetMainPhaseParentSection(doc);
            var firstRoundSection = GetFirstRoundSection(parentSectionWithMainPhase);
            var firstRoundMatchWeeks = GetFirstRoundMatchWeeks(firstRoundSection);

            var seasonMatchWeekStatistics = new Dictionary<int, List<SeasonMatchWeekGameStatistics>>();
            var currentMatchWeek = 1;

            foreach (var matchWeek in firstRoundMatchWeeks)
            {
                seasonMatchWeekStatistics[currentMatchWeek] = [];

                var matches = matchWeek.Descendants("section")
                    .Where(section =>
                    {
                        var classAttr = section.GetAttributeValue("class", "");
                        var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                        return classes.Contains("ajax-synced-games");
                    }).ToList();

                var matchIds = matches.Select(game => game.GetAttributeValue("data-game-id", string.Empty)).ToList();

                foreach (var matchId in matchIds)
                {
                    if (!int.TryParse(matchId, out var parsedMatchId))
                    {
                        Console.WriteLine($"Invalid Match ID: {matchId}");
                        continue;
                    }

                    var (firstTeamStatistics, secondTeamStatistics) = await matchStatisticsScrapper.GetMatchStatisticsAsync(parsedMatchId);

                    seasonMatchWeekStatistics[currentMatchWeek].Add(new SeasonMatchWeekGameStatistics(
                        firstTeamStatistics,
                        secondTeamStatistics,
                        parsedMatchId
                    ));
                }

                currentMatchWeek++;
            }

            return seasonMatchWeekStatistics;
        }

        private static HtmlNode? GetMainPhaseParentSection(HtmlDocument doc)
            => doc.DocumentNode.Descendants("section")
                .FirstOrDefault(section =>
                {
                    var classAttr = section.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    var dataPhaseAttribute = section.Attributes
                       .FirstOrDefault(attr => attr.Name.Equals("data-phase", StringComparison.Ordinal));

                    return classes.Contains("filterable-content") &&
                           dataPhaseAttribute != null &&
                           dataPhaseAttribute.Value.Equals("RS", StringComparison.Ordinal);
                });

        private static HtmlNode? GetFirstRoundSection(HtmlNode parentSectionWithMainPhase)
            => parentSectionWithMainPhase?.Descendants("section")
                .FirstOrDefault(section =>
                {
                    var classAttr = section.GetAttributeValue("class", "");
                    var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    var dataRoundAttribute = section.Attributes
                        .FirstOrDefault(attr => attr.Name.Equals("data-round", StringComparison.Ordinal));

                    return classes.Contains("filterable-content") &&
                           dataRoundAttribute != null;
                });

        private static List<HtmlNode>? GetFirstRoundMatchWeeks(HtmlNode firstRoundSection)
            => firstRoundSection?.Descendants("section")
                 .Where(section =>
                 {
                     var classAttr = section.GetAttributeValue("class", "");
                     var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                     var dataTermAttribute = section.Attributes
                        .FirstOrDefault(attr => attr.Name.Equals("data-term", StringComparison.Ordinal));

                     return classes.Contains("filterable-content") &&
                            dataTermAttribute != null;
                 }).ToList();

    }
}
