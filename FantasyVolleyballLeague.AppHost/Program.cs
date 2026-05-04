var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("fantasy-volleyball-league-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithContainerName("fantasy-volleyball-league-postgres")
    .AddDatabase("FantasyVolleyballLeagueDb");

builder.AddProject<Projects.FantasyVolleyballLeague_Api>("fantasy-volleyball-league-api")
    .WithReference(postgres)
    .WaitFor(postgres);


await builder.Build().RunAsync();