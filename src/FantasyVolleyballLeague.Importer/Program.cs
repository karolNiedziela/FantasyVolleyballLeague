using FantasyVolleyballLeague.Api.Infrastructure.Database;
using FantasyVolleyballLeague.Importer;
using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Steps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

// Local overrides (connection string outside Aspire). Environment variables are re-added afterwards,
// so a connection string injected by the Aspire AppHost still wins.
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

builder.AddNpgsqlDbContext<FantasyVolleyballLeagueDbContext>("FantasyVolleyballLeagueDb");

builder.Services.Configure<ImporterOptions>(builder.Configuration.GetSection(ImporterOptions.SectionName));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));

builder.Services.AddSingleton<ImportReport>();
builder.Services.AddSingleton<RawDataReader>();
builder.Services.AddScoped<ImportCatalog>();
builder.Services.AddTransient<IImportStep, RostersImportStep>();
builder.Services.AddTransient<IImportStep, MatchesImportStep>();
builder.Services.AddTransient<ImportRunner>();

using var host = builder.Build();

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

using var scope = host.Services.CreateScope();

var runner = scope.ServiceProvider.GetRequiredService<ImportRunner>();

await runner.RunAsync(cancellation.Token);
