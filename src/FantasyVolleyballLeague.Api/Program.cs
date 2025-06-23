using FantasyVolleyballLeague.Api;
using FantasyVolleyballLeague.Api.Database;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
  .SetBasePath(builder.Environment.ContentRootPath)
  .AddJsonFile("appsettings.json")
  .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
  .AddUserSecrets<Program>()
  .AddEnvironmentVariables()
  .Build();

builder.AddSqlServerDbContext<FantasyVolleyballLeagueDbContext>("FantasyVolleyballLeagueDb");

builder.Services.AddDatabase(configuration);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

app.MapGet("/weatherforecast", () =>
{
#pragma warning disable CA5394 // Do not use insecure randomness
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
#pragma warning restore CA5394 // Do not use insecure randomness
    return forecast;
})
.WithName("GetWeatherForecast");

await app.RunAsync();

#pragma warning disable S3903 // Types should be defined in named namespaces
sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
#pragma warning restore S3903 // Types should be defined in named namespaces
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
