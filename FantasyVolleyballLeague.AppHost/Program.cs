var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("fantasy-volleyball-league-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(5432)
    .WithContainerName("fantasy-volleyball-league-postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithLifetime(ContainerLifetime.Persistent))
    .AddDatabase("FantasyVolleyballLeagueDb");

var api = builder.AddProject<Projects.FantasyVolleyballLeague_Api>("fantasy-volleyball-league-api")
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health");

// Data pipeline, started on demand from the dashboard. Full history backfills run from the CLI
builder.AddProject<Projects.FantasyVolleyballLeague_Scraper>("scraper-matches")
    .WithArgs("--job", "Matches", "--current", "true")
    .WithExplicitStart();

builder.AddProject<Projects.FantasyVolleyballLeague_Scraper>("scraper-rosters")
    .WithArgs("--job", "Rosters", "--current", "true")
    .WithExplicitStart();

builder.AddProject<Projects.FantasyVolleyballLeague_Importer>("importer")
    .WithReference(postgres)
    .WaitFor(api)
    .WithExplicitStart();

await builder.Build().RunAsync();
