using FantasyVolleyballLeague.Worker.StatisticsScrapper;
using FantasyVolleyballLeague.Worker.StatisticsScrappers;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga;
using FantasyVolleyballLeague.Worker.TeamScrappers;
using FantasyVolleyballLeague.Worker.TeamScrappers.Plusliga;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

serviceCollection.AddScoped<IMatchStatisticsScrapper, PlusligaMatchStatisticsScrapper>();
serviceCollection.AddScoped<ISeasonScrapper, PlusligaSeasonScrapper>();
serviceCollection.AddScoped<ITeamScrapper, PlusligaTeamScrapper>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var plusligaMatchStatisticsScrapper = serviceProvider.GetRequiredService<IMatchStatisticsScrapper>();
var plusligaSeasonScrapper = serviceProvider.GetRequiredService<ISeasonScrapper>();
var plusligaTeamScrapper = serviceProvider.GetRequiredService<ITeamScrapper>();

//await plusligaSeasonScrapper.GetSeasonStatisticsAsync();

await plusligaTeamScrapper.GetTeamDataAsync();


Console.ReadKey();