using Asp.Versioning;
using FantasyVolleyballLeague.Api;
using FantasyVolleyballLeague.Api.Common.Endpoints;
using FantasyVolleyballLeague.Api.Common.Versioning;
using FantasyVolleyballLeague.Api.Infrastructure;
using FantasyVolleyballLeague.Api.Infrastructure.Database;
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