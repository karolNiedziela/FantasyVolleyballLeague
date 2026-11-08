using Asp.Versioning;
using FantasyVolleyballLeague.DataApi;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Versioning;
using FantasyVolleyballLeague.DataApi.Infrastructure;
using FantasyVolleyballLeague.VolleyballData.Database;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<FantasyVolleyballLeagueDbContext>("FantasyVolleyballLeagueDb");
builder.Services.AddInfrastructure();
builder.AddServiceDefaults();
builder.Services.AddEndpoints();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = ApiVersions.V1;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
})
.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();
}

app.MapOpenApi()
    .WithDocumentPerVersion();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.MapEndpoints();

await app.RunAsync();