using Asp.Versioning;
using FantasyVolleyballLeague.DataApi;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.ErrorHandling;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.DataApi.Common.Versioning;
using FantasyVolleyballLeague.DataApi.Infrastructure;
using FantasyVolleyballLeague.Contracts;
using FantasyVolleyballLeague.VolleyballData.Database;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<FantasyVolleyballLeagueDbContext>(
    DatabaseResource.Name,
    configureDbContextOptions: options => options.ThrowOnUnorderedPaging());
builder.Services.AddInfrastructure();
builder.Services.AddHandlers();
builder.AddServiceDefaults();
builder.Services.AddEndpoints();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions[ProblemDetailsExtensions.TraceId] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions[ProblemDetailsExtensions.Timestamp] = DateTime.UtcNow;
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

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
.AddOpenApi(options =>
{
    options.Document.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Description = "Read-only API with Polish professional volleyball league data: leagues, seasons, rosters, players and matches.";

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();
}

app.MapOpenApi()
    .WithDocumentPerVersion();
app.MapScalarApiReference();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();
app.MapEndpoints();

await app.RunAsync();