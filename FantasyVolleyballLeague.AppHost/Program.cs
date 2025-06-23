var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", true);

var sql = builder.AddSqlServer("fantasy-volleyball-league-sql", password: sqlPassword, port: 1433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithContainerName("fantasy-volleyball-league-sql")
    .AddDatabase("FantasyVolleyballLeagueDb");

builder.AddProject<Projects.FantasyVolleyballLeague_Api>("fantasy-volleyball-league-api")
    .WithReference(sql)
    .WaitFor(sql);


await builder.Build().RunAsync();