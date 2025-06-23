using FantasyVolleyballLeague.Worker.StatisticsExtractor;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

var serviceCollection = new ServiceCollection();

serviceCollection.AddScoped<IStatisticsScrapper, PlusligaStatisticsScrapper>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var plusligaStatisticsScrapper = serviceProvider.GetRequiredService<IStatisticsScrapper>();

await plusligaStatisticsScrapper.GetMatchStatisticsAsync();

Console.ReadKey();