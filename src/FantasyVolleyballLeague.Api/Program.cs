using FantasyVolleyballLeague.Api;
using FantasyVolleyballLeague.Api.Common.Endpoints;
using FantasyVolleyballLeague.Api.Infrastructure;
using FantasyVolleyballLeague.Api.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<FantasyVolleyballLeagueDbContext>("FantasyVolleyballLeagueDb");
builder.Services.AddInfrastructure();
builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddEndpoints();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapEndpoints();

await app.RunAsync();
