## Backend

### Project structure

The backend is split by product area, not by technical layer. Do not add Domain/Infrastructure class libraries.

- `FantasyVolleyballLeague.VolleyballData`: the volleyball data model shared by every consumer
  (the DataApi, the Importer and a future Fantasy game). It depends on no web project.

      src/FantasyVolleyballLeague.VolleyballData/
      ├── Domain/          # Entity classes
      └── Database/        # DbContext, EF configurations, DI wiring, migrations

- `FantasyVolleyballLeague.DataApi`: the public, read-only, versioned API for third parties.

      src/FantasyVolleyballLeague.DataApi/
      ├── Common/          # Shared plumbing (endpoint scanning, versioning, base types)
      ├── Features/        # Vertical slices — one folder per feature
      └── Infrastructure/  # DI wiring

- `FantasyVolleyballLeague.Contracts`: a tiny library of compile-time constants shared by the AppHost, the apps and the
  integration tests (`DatabaseResource`: database name and Postgres image). No logic and no NuGet packages, so the
  Importer and the AppHost do not pull in ASP.NET Core (the reason it is not part of `ServiceDefaults`). It is not a
  place for general shared code.
- A future Fantasy game gets its own projects (domain and API) that reference `VolleyballData`.
  Never reference `DataApi` from another project.
- `VolleyballData` types are `internal` and exposed through `InternalsVisibleTo`
  (DataApi, Importer, ArchitectureTests, DataApi.UnitTests, DataApi.IntegrationTests). EF commands: `--project src/FantasyVolleyballLeague.VolleyballData
  --startup-project src/FantasyVolleyballLeague.DataApi`.

### Vertical slice architecture

Each feature owns everything it needs. Organise by feature, not by technical layer.

    Features/<FeatureName>/<UseCaseName>/
    ├── <UseCaseName>Query.cs          # or <UseCaseName>Command.cs
    ├── <UseCaseName>QueryHandler.cs   # or <UseCaseName>CommandHandler.cs
    ├── <UseCaseName>Response.cs       # or <UseCaseName>Request.cs
    └── <UseCaseName>Endpoint.cs

- One use case = one folder. Do not merge multiple use cases into one file.
- Each use case has its own `IEndpoint` class, not one class per resource. What the use cases of one feature share
  (route prefix, OpenAPI tag) lives in `Features/<Feature>/<Feature>Routes.cs`, for example `LeaguesRoutes`. Policies
  for the whole API (authorization, rate limiting) go on the `v1` group in `MapEndpoints`, not on each endpoint.
- Handlers must not call other handlers.
- Features read from DbContext directly — no generic repository pattern.

### Endpoints

Use minimal APIs. Every endpoint is a class that implements `IEndpoint`:

```csharp
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
```

Endpoints are auto-registered via `AddEndpoints()` / `MapEndpoints()` in `Program.cs`.
Do not use controllers.

Validation of `sort`, `offset` and `limit` runs in endpoint filters that exist only when the endpoint asks for them.
An endpoint whose query implements `ISortableQuery<>` must call `.WithSorting<TQuery, TEntity>()`, and one whose query
implements `IPagedQuery` must call `.WithPaging()`. Without them a bad `sort` or `limit` gives a 500 instead of a 400
(and no upper limit on `limit`). No architecture test enforces this, so check it in every list endpoint.

Paging: a list endpoint returns `PagedResults.Ok(page)`. The response body is `{ items, offset, limit, totalCount, links }`
and `links` (`self`, `first`, `prev`, `next`, `last`, absolute URLs) is filled by `PagedOk` from the request URL, because a
handler does not know the URL. `self` always has the effective `offset` and `limit`. `prev` and `next` are `null` on the first
and last page. There is no `Link` header: the links are in the body (also readable from a browser without CORS setup).
This is the one place where this file overrides the REST skill's `{ data, pagination, links }` envelope.

OpenAPI: `.WithPaging()` and `.WithSorting<,>()` describe `offset`, `limit` and `sort` (the sort fields come from the
`SortMap`). Describe other query parameters of an endpoint with `.WithParameterDescription(name, text)`. Do not use
`[Description]` on the parameters of an `[AsParameters]` type: it throws in Microsoft.AspNetCore.OpenApi 10.0.14.

### REST conventions

Follow the `dotnet-rest-api-rules` skill (resource naming, status codes, Problem Details,
pagination, URL versioning, OpenAPI). Where it conflicts with this file, this file wins:
- One `IEndpoint` class and one handler per use case, not a static `Map<Resource>` class
  and not a service layer behind an interface.
- Handlers read `DbContext` directly.


### Namespaces

Root namespaces: `FantasyVolleyballLeague.DataApi` and `FantasyVolleyballLeague.VolleyballData`.
Follow folder structure: `FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosters`,
`FantasyVolleyballLeague.VolleyballData.Domain`, etc.

### Dependency injection

Use explicit constructor injection with `private readonly` fields. Do not use primary constructors.

## Sorting and collation

Postgres sorts text with the database collation (`en_US.utf8`), which sorts Polish letters wrongly
(`Łańcut` before `Lublin`, `Śląsk` before `Suwałki`). Name columns that are sorted for users use an ICU collation.

- **Today (Poland only):** `League.Name` is `UseCollation("pl-x-icu")` (`LeagueEntityTypeConfiguration`). Give the
  same collation to every other sorted name column when its slice is built (`Roster.Name`, `Player.FullName`).
- **When the first league from another country is added, replace this.** No single collation is right for all
  languages (Turkish `I`/`İ`, Czech `ch` after `h`, Polish `Ł`/`Ś`), and a collation belongs to a column or an
  expression, never to a row. Planned design:
  - `LeagueCountry` gets a collation name (Poland `pl-x-icu`, Turkey `tr-x-icu`, Italy `it-x-icu`).
  - Remove the column collations with a migration (they would force one language on all rows).
  - Sorting by name for lists spanning countries groups by country first (`ORDER BY CountryId`), then sorts each
    group with its own collation. `EF.Functions.Collate` needs a constant, so generate one
    `CASE WHEN CountryId = n THEN Collate(Name, "<collation>") END` ordering per country from `LeagueCountry.All`.
  - `SortMap` allows one expression per sort key today. A key must be able to produce several orderings.
  - `Player` has no country, so decide the rule for player names (for example use the country of the league being
    queried, or a neutral collation such as `und-x-icu`).
  - Name search (`?q=`) is unaffected: it matches on the normalized name column, not on a collation.
- ICU collations have a version. When the Postgres image (`DatabaseResource.PostgresImageTag`) is upgraded, check
  indexes on collated columns (`REINDEX`) and the collation version warning.

## Packages

All NuGet package versions are managed centrally in `Directory.Packages.props`.
- Never set a version directly in a `.csproj` file.
- Always add new packages to `Directory.Packages.props` first, then reference them without a version in the project file.

## Build

Defined in `Directory.Build.props` and applies to all projects:
- Target framework: `net10.0`
- Nullable reference types: enabled
- Implicit usings: enabled
- Analysis level: latest, all rules enabled
- Code analysis warnings are treated as errors — do not suppress without a reason
- `SonarAnalyzer.CSharp` is included in every project as an analyser

## Code style

Enforced via `.editorconfig`:
- Indentation: 4 spaces
- Namespaces: block-scoped (`namespace Foo { }`)
- `using` directives: outside the namespace
- `var`: use for non-built-in types when type is not apparent
- Primary constructors: disabled — use explicit constructor + `private readonly` fields
- Braces: always required, even for single-line blocks
- Expression-bodied members: allowed for accessors and properties, not for constructors or methods
- `readonly` fields: preferred where possible

## Tests

- Test framework: **xunit v3** (`xunit.v3`) in every test project, run by Microsoft.Testing.Platform (opted in with the
  root `global.json`). Run with `dotnet test --solution FantasyVolleyballLeague.sln` or
  `dotnet test --project tests/<Project>`. Assertions: **Shouldly** (`actual.ShouldBe(expected)`), not xunit `Assert`.
- Shouldly is a global using in every test project.
- The DataApi has two test projects:
  - `DataApi.UnitTests`: pure logic, no database and no Docker (sort parser, filters, link builder, argument checks).
  - `DataApi.IntegrationTests`: real Postgres and the real HTTP pipeline. **Docker must be running.**
    - `DataApiFactory` (a `WebApplicationFactory<Program>`) starts one Postgres container for the whole run (xunit
      assembly fixture), pinned to `DatabaseResource.PostgresImageTag` (the same image as the Aspire dev container),
      applies the migrations and passes the connection string with `UseSetting`. The app runs in the `Testing`
      environment. Do not remove or re-register services.
    - Tests derive from `IntegrationTestBase`: the database is reset with Respawn before each test (lookup tables with
      seed data are kept). Tests run sequentially, because they share the database.
- In every test project, shared test helpers live in `Fixtures/` (e.g. `DataApiFactory`, `IntegrationTestBase`,
  `ProjectTypes`) and test data builders in `Seeds/` (e.g. `LeagueSeeds`). Neither name exists in `src`, because the
  other test folders mirror the `src` folders (tests of `Program.cs` sit in the project root).
    - Never use EF InMemory or SQLite: collation, `LIKE`, trigram search and null ordering differ from Postgres.
    - Per slice, mirror `Features/<Feature>/<UseCase>/` under `Features/` in the test project: handler tests
      (`new XxxHandler(dbContext)`) are the bulk, a few endpoint tests per slice cover the HTTP contract. Fake only
      external systems.
- Expression trees cannot hold tuple literals or `is { }` patterns, so for those use `foreach` + `ShouldBe`, or `.Any(...).ShouldBeTrue()`, instead of `ShouldContain(predicate)`.
