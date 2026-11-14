using FantasyVolleyballLeague.Contracts;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage(DatabaseResource.PostgresImage, DatabaseResource.PostgresImageTag)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("fantasy-volleyball-league-postgres-data")
    .WithHostPort(5432)
    .WithContainerName("fantasy-volleyball-league-postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithLifetime(ContainerLifetime.Persistent))
    .AddDatabase(DatabaseResource.Name);

var dataApi = builder.AddProject<Projects.FantasyVolleyballLeague_DataApi>("data-api")
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

// Waits for the DataApi, not only Postgres: the DataApi applies the EF migrations on startup.
builder.AddProject<Projects.FantasyVolleyballLeague_Importer>("importer")
    .WithReference(postgres)
    .WaitFor(dataApi)
    .WithExplicitStart();

await builder.Build().RunAsync();
