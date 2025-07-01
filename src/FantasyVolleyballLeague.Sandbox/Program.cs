using FantasyVolleyballLeague.Worker.StatisticsScrapper;
using FantasyVolleyballLeague.Worker.StatisticsScrappers;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;



var serviceCollection = new ServiceCollection();

serviceCollection.AddScoped<IMatchStatisticsScrapper, PlusligaMatchStatisticsScrapper>();
serviceCollection.AddScoped<ISeasonScrapper, PlusligaSeasonScrapper>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var plusligaMatchStatisticsScrapper = serviceProvider.GetRequiredService<IMatchStatisticsScrapper>();
var plusligaSeasonScrapper = serviceProvider.GetRequiredService<ISeasonScrapper>();

await plusligaSeasonScrapper.GetSeasonStatisticsAsync();

//await GetMatchWeeks(plusligaMatchStatisticsScrapper);

//await plusligaMatchStatisticsScrapper.GetMatchStatisticsAsync();

Console.ReadKey();

static async Task GetMatchWeeks(IMatchStatisticsScrapper matchStatisticsScrapper)
{
    var url = "https://www.plusliga.pl/games.html";
    var web = new HtmlWeb();
    var doc = web.Load(new Uri(url));

    var parentSectionWithMainPhase = doc.DocumentNode.Descendants("section")
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

    var firstRoundSection = parentSectionWithMainPhase?.Descendants("section")
        .FirstOrDefault(section =>
        {
            var classAttr = section.GetAttributeValue("class", "");
            var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            var dataRoundAttribute = section.Attributes
                .FirstOrDefault(attr => attr.Name.Equals("data-round", StringComparison.Ordinal));

            return classes.Contains("filterable-content") &&
                   dataRoundAttribute != null;
        });

    var firstRoundMatchWeeks =  firstRoundSection?.Descendants("section")
         .Where(section =>
         {
             var classAttr = section.GetAttributeValue("class", "");
             var classes = classAttr.Split([' '], StringSplitOptions.RemoveEmptyEntries);

             var dataTermAttribute = section.Attributes
                .FirstOrDefault(attr => attr.Name.Equals("data-term", StringComparison.Ordinal));

             return classes.Contains("filterable-content") &&
                    dataTermAttribute != null;
         }).ToList();

    var matchWeeks = Enumerable.Range(1, firstRoundMatchWeeks!.Count);

    foreach (var matchWeek in firstRoundMatchWeeks)
    {
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
            if (int.TryParse(matchId, out var id))
            {
                await matchStatisticsScrapper.GetMatchStatisticsAsync(id);
            }
            else
            {
                Console.WriteLine($"Invalid Match ID: {matchId}");
            }
        }
    }
}