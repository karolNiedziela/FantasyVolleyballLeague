using FantasyVolleyballLeague.Scraper;
using FantasyVolleyballLeague.Scraper.DataProcessors.Statistics;
using FantasyVolleyballLeague.Scraper.DataProcessors.Rosters;
using FantasyVolleyballLeague.Scraper.Sources;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters;
using FantasyVolleyballLeague.Scraper.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

// Usage: --job Matches|Rosters [--league PlusLiga] [--from 2000] [--to 2018] [--current true] [--force true]
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory,
});
builder.Configuration.AddCommandLine(args, JobOptions.SwitchMappings);

builder.Services.Configure<PlaywrightOptions>(builder.Configuration.GetSection(PlaywrightOptions.SectionName));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
builder.Services.Configure<JobOptions>(builder.Configuration.GetSection(JobOptions.SectionName));

var leagues = builder.Configuration.GetSection(LeagueOptions.SectionName).Get<List<LeagueOptions>>() ?? [];
builder.Services.AddSingleton<IReadOnlyList<LeagueOptions>>(leagues);

builder.Services.AddSingleton<PlaywrightFactory>();
builder.Services.AddSingleton<RawDataStore>();
builder.Services.AddScoped<LeagueSourceResolver>();

// Poland: leagues run by PLS (Polska Liga Siatkówki S.A.).
builder.Services.AddSingleton<PlsWidgetLoader>();
builder.Services.AddScoped<PlsMatchScrapper>();
builder.Services.AddScoped<PlsRosterScrapper>();
builder.Services.AddScoped<ILeagueSource, PlsLeagueSource>();

builder.Services.AddScoped<IStatisticsDataProcessor, StatisticsDataProcessor>();
builder.Services.AddScoped<IRosterDataProcessor, RosterDataProcessor>();

using var host = builder.Build();

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

var jobOptions = host.Services.GetRequiredService<IOptions<JobOptions>>().Value;
var store = host.Services.GetRequiredService<RawDataStore>();
Console.WriteLine($"Job: {jobOptions.Kind}, data root: {store.Root}");

using var scope = host.Services.CreateScope();

// Fail fast on a misconfigured league source instead of in the middle of a scrape.
var sourceResolver = scope.ServiceProvider.GetRequiredService<LeagueSourceResolver>();
foreach (var league in leagues)
{
    sourceResolver.Resolve(league);
}

switch (jobOptions.Kind)
{
    case JobKind.Matches:
        await scope.ServiceProvider.GetRequiredService<IStatisticsDataProcessor>().AcquireAndSaveAsync(cancellation.Token);
        break;
    case JobKind.Rosters:
        await scope.ServiceProvider.GetRequiredService<IRosterDataProcessor>().AcquireAndSaveAsync(cancellation.Token);
        break;
    default:
        throw new InvalidOperationException($"Unknown job '{jobOptions.Kind}'.");
}
