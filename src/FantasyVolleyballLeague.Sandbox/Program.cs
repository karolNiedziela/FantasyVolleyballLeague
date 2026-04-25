using CsvHelper;
using FantasyVolleyballLeague.Worker;
using FantasyVolleyballLeague.Worker.DataProcessors.Statistics;
using FantasyVolleyballLeague.Worker.DataProcessors.Teams;
using FantasyVolleyballLeague.Worker.Services;
using FantasyVolleyballLeague.Worker.StatisticsScrapper;
using FantasyVolleyballLeague.Worker.StatisticsScrappers;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga;
using FantasyVolleyballLeague.Worker.TeamScrappers;
using FantasyVolleyballLeague.Worker.TeamScrappers.Models;
using FantasyVolleyballLeague.Worker.TeamScrappers.Plusliga;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

var config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

serviceCollection.AddOptions<PlaywrightOptions>()
    .Configure(options => config.GetSection(PlaywrightOptions.SectionName).Bind(options));
serviceCollection.AddScoped<PlaywrightFactory>();

var leagues = config.GetSection(LeagueOptions.SectionName).Get<List<LeagueOptions>>() ?? [];
serviceCollection.AddSingleton<IReadOnlyList<LeagueOptions>>(leagues);

serviceCollection.AddScoped<IMatchStatisticsScrapper, PlusligaMatchStatisticsScrapper>();
serviceCollection.AddScoped<ISeasonMatchScrapper, PlusligaSeasonMatchScrapper>();
serviceCollection.AddScoped<ITeamScrapper, PlusligaTeamScrapper>();
serviceCollection.AddScoped<ISeasonScrapper, SeasonScrapper>();
serviceCollection.AddScoped<ITeamDataProcessor, TeamDataProcessor>();
serviceCollection.AddScoped<IStatisticsDataProcessor, StatisticsDataProcessor>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var plusligaSeasonMatchScrapper = serviceProvider.GetRequiredService<ISeasonMatchScrapper>();
var seasonScrapper = serviceProvider.GetRequiredService<ISeasonScrapper>();
var teamDataProcessor = serviceProvider.GetRequiredService<ITeamDataProcessor>();
var statisticsDataProcessor = serviceProvider.GetRequiredService<IStatisticsDataProcessor>();

await teamDataProcessor.AcquireAndSaveAsync();
//await statisticsDataProcessor.AcquireAndSaveAsync();

Console.ReadKey();