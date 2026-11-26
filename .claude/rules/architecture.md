## System overview

```
League websites ──(Scraper)──► data/raw/*.json ──(Importer)──► Postgres ◄──(EF Core)── DataApi ──► API clients
                                                                  ▲
                               AppHost (Aspire) starts Postgres and the DataApi; Scraper and Importer on demand
```

The Scraper and the Importer share nothing but the raw JSON files and the position names in `Shared`, and only the Importer and the DataApi know the database.

### Solution map

| Project | Role |
|---|---|
| `FantasyVolleyballLeague.VolleyballData` | The volleyball **data model**: the domain (`Domain/`), the EF Core `FantasyVolleyballLeagueDbContext` and the migrations (Postgres) in `Database/`. Shared by the DataApi and the Importer (`InternalsVisibleTo`). |
| `FantasyVolleyballLeague.DataApi` | Public read-only ASP.NET Core minimal API for third parties (vertical slices, URL versioning). Applies migrations on startup in Development. |
| `FantasyVolleyballLeague.Scraper` | Playwright + HtmlAgilityPack **scraper** CLI. Source-independent processors (`DataProcessors/`), shared raw models (`Models/`) and per-source scrapers (`Sources/`). Writes raw JSON to `data/raw`. |
| `FantasyVolleyballLeague.Importer` | **Importer** console app. Reads `data/raw`, upserts into Postgres using the VolleyballData DbContext (`InternalsVisibleTo`). |
| `FantasyVolleyballLeague.AppHost` | Aspire: Postgres container, DataApi (healthy once migrations are applied), and explicit-start resources started from the dashboard: `scraper-matches` (current season matches), `scraper-rosters` (current season rosters) and `importer` (waits for the DataApi). Historical backfills run from the Scraper CLI. |
| `FantasyVolleyballLeague.ServiceDefaults` | Aspire service defaults (OpenTelemetry, health checks). |
| `FantasyVolleyballLeague.Shared` | Small dependency-free types and constants shared by the AppHost, the DataApi, the Importer, the Scraper, `VolleyballData` and the tests: `Constants/` (`PhaseNames`, `PositionNames`: the names in the raw JSON and in the database), `Persistence/` (`DatabaseResource`: database name and the pinned Postgres image), `Types/` (`Entity`, `EntityId`) and `Text/` (`NameNormalizer`). No packages. |

Project references:
- `DataApi` → `VolleyballData`, `Shared`, `ServiceDefaults`.
- `Importer` → `VolleyballData`, `Shared`.
- `AppHost` → `DataApi`, `Scraper`, `Importer`, `Shared`.
- `Scraper` → `Shared`; it references no other project.
- `VolleyballData` → `Shared`; it references no web project.

### Importer layout

`ImportRunner` runs the `IImportStep`s ordered by `Order` (`RostersImportStep`, `MatchesImportStep`, then
`SeasonLeagueNamesImportStep`, which sets the league name of each season, and `ClubGroupingImportStep`, which groups the rosters
into clubs) and writes an
`ImportReport` (unresolved players, file errors, warnings, counters) next to the data. `Records/*File` are the DTOs
that mirror the raw JSON and the manual data files, `ManualData/*.json` is the data entered by hand (in git, copied to
the output: `league-names.json`, `clubs.json`, `club-splits.json`), `Import/` holds the pure helpers (`RosterMatching`, `PlayerNameKey`,
`CareerMembership`, `SeasonNameIndex`, `StatisticsParsing`, `LeagueNaming`, `ClubKey`, `ClubGrouping`, `RosterStaffChanges`) and the readers
`RawDataReader` (raw JSON; tested on read-only files in `Fixtures/RawData`) and `ManualDataReader` (reads and validates
the manual files; `Fixtures/ManualData`)
that `Importer.Tests` cover, and `Steps/` holds the steps and the per-match context.

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

- `FantasyVolleyballLeague.Shared`: a small library of public, dependency-free building blocks used by several projects,
  one folder per kind: `Constants/` (`PhaseNames`, `PositionNames`), `Persistence/` (`DatabaseResource`), `Types/`
  (`Entity`, `EntityId`) and `Text/` (`NameNormalizer`). Pure code only: no NuGet packages and no reference to another
  project, so the Importer and the AppHost do not pull in ASP.NET Core (the reason it is not part of `ServiceDefaults`).
  Something belongs here only when two projects need it; code used by one project stays in that project. Its namespaces
  contain `Shared`, a Visual Basic keyword, so CA1716 is switched off in `Shared` and `Shared.UnitTests` (the solution is C# only).
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

- One use case = one folder. Do not merge multiple use cases into one file. Each slice has a `<UseCaseName>.http` file
  next to it for manual requests.
- Each use case has its own `IEndpoint` class, not one class per resource. What the use cases of one feature share
  (route prefix, OpenAPI tag) lives in `Features/<Feature>/<Feature>Routes.cs`, for example `LeaguesRoutes`. Policies
  for the whole API (authorization, rate limiting) go on the `v1` group in `MapEndpoints`, not on each endpoint.
- Handlers must not call other handlers.
- Nested routes are navigation aliases of a top-level list, at most one level deep (`/leagues/{id}/seasons` aliases
  `/seasons?leagueId=`). An alias is its own use case in the feature of the returned resource
  (`Features/Seasons/GetLeagueSeasons/`), returns the same DTO with the same sorting, paging and filters, and returns
  404 when the parent does not exist (the top-level filter returns an empty list). The list and its aliases share the
  projection and filters through `IQueryable` extensions of the feature, not through each other's handlers.
  Exception: `/players/{id}/rosters` (the career of a player) is not an alias of `/rosters?playerId=`. A roster player
  is a resource of its own (shirt number, position, `memberFrom`/`memberTo`, which `RosterResponse` does not carry), so
  it has its own DTO in `Features/Players/GetPlayerRosters/`. It keeps the rest of the alias rules: paging, sorting
  and 404 when the player does not exist.
- Detail responses carry the summaries of their parents and the navigation the client cannot compute
  (`currentSeason`, `previousSeason`/`nextSeason`, `previousRound`/`nextRound`) as typed fields. No `links` to
  sub-collections: `links` exist only in the paging envelope.
- Features read from the DbContext directly (through `IDbContext`) — no generic repository pattern.

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
Range and allow-list checks stay in the filters. Unparseable parameters (`?offset=abc`) are handled by the framework
and `GlobalExceptionHandler`.

Lookups (gender, country, position, match status, statistics level) are exposed by their `Code`: a stable `snake_case`
value (`women`, `outside_hitter`, `lineup_only`; countries use lowercase ISO 3166-1 alpha-2, `pl`), never by id or
`Name`. Responses carry the plain code (`"gender": "women"`); labels and translations belong to the clients. A lookup
filter is a `string?` query parameter registered with `.WithCodeFilter(parameter, description, codes)`, which answers
an unknown or wrongly cased code with 400 and lists the codes as an OpenAPI `enum`. The handler maps the code to the id
with `FromCode` and filters by the foreign key. Codes are a public contract: add new ones, never rename or remove one
(`LookupCodeTests` pins them). `Name` stays for the Importer, which matches raw JSON values by it.

Paging: a list endpoint returns `PagedResults.Ok(page)`, with `links` in the body and no `Link` header. This is the one
place where this file overrides the REST skill's `{ data, pagination, links }` envelope. Paging is offset-based (`offset`,
`limit`), not keyset, on purpose: lists hold at most a few thousand rows, and clients need random page access. Revisit it for
a list that grows far beyond that.

OpenAPI: `.WithPaging()` and `.WithSorting<,>()` describe `offset`, `limit` and `sort`. Describe other query parameters
with `.WithParameterDescription(name, text)`. Do not use `[Description]` on the parameters of an `[AsParameters]` type:
it throws in Microsoft.AspNetCore.OpenApi 10.0.14.

### REST conventions

Follow the `aspnetcore-rest-api` skill (resource naming, status codes, Problem Details,
pagination, URL versioning, OpenAPI). Where it conflicts with this file, this file wins:
- One `IEndpoint` class and one handler per use case, not a static `Map<Resource>` class
  and not a service layer behind an interface.
- Handlers read the database through `IDbContext` directly, with no repository layer.


### Namespaces

Root namespaces: `FantasyVolleyballLeague.DataApi` and `FantasyVolleyballLeague.VolleyballData`.
Follow folder structure: `FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosters`,
`FantasyVolleyballLeague.VolleyballData.Domain`, etc.

### Dependency injection

Use explicit constructor injection with `private readonly` fields. Do not use primary constructors.

Every injected collaborator is an interface, registered in DI, with the interface next to its implementation: handlers
are injected into endpoints as `IQueryHandler<TQuery, TResponse>`, handlers read the database through `IDbContext`
(`Set<T>()`; writes will use `IUnitOfWork`), and a collaborator such as `IStandingsCalculator` is an interface too.
Exceptions: framework types (`ILogger<T>`, `IOptions<T>`, `CancellationToken`), records and DTOs, and static pure
functions that need no injection.

Complex logic goes into its own class behind such an interface. It takes plain records and returns plain records (not
entities) and does not touch the database, so it is unit tested directly. Queries and the flow of a handler are tested
against real Postgres. Mocking `IDbContext` does not work with EF Core (`ToListAsync` and `CountAsync` need a real
query provider), so handler tests use the real context, not a substitute.

## Sorting and collation

Postgres sorts text with the database collation (`en_US.utf8`), which sorts Polish letters wrongly
(`Łańcut` before `Lublin`, `Śląsk` before `Suwałki`). Name columns that are sorted for users use an ICU collation.

- **Today (Poland only):** `League.Name`, `Roster.Name`, `Club.Name`, `Player.FullName` and `StaffMember.FullName` are
  `UseCollation("pl-x-icu")` (their `...EntityTypeConfiguration` classes). Give the same
  collation to every other sorted name column when its slice is built. Do not use `EF.Functions.Collate` in queries for
  these columns.
- **When the first league from another country is added, replace this.** No single collation is right for all
  languages (Turkish `I`/`İ`, Czech `ch` after `h`, Polish `Ł`/`Ś`), and a collation belongs to a column or an
  expression, never to a row. Planned design:
  - `LeagueCountry` gets a collation name (Poland `pl-x-icu`, Turkey `tr-x-icu`, Italy `it-x-icu`).
  - Remove the column collations with a migration (they would force one language on all rows).
  - Sorting by name for lists spanning countries groups by country first (`ORDER BY CountryId`), then sorts each
    group with its own collation. `EF.Functions.Collate` needs a constant, so generate one
    `CASE WHEN CountryId = n THEN Collate(Name, "<collation>") END` ordering per country from `LeagueCountry.All`.
  - `SortMap` allows one expression per sort key today. A key must be able to produce several orderings.
  - `Player` and `StaffMember` have no country, so decide the rule for their names (for example use the country of the league being
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
  root `global.json`; commands in `commands-and-tests.md`). Assertions: **Shouldly** (`actual.ShouldBe(expected)`),
  not xunit `Assert`.
- Shouldly is a global using in every test project.
- Test methods are named `Subject_Should_Outcome`, for example `ChangeDateOfPayments_Should_Change_DateOfPayment` or
  `HandleAsync_Should_FilterByGender`. CA1707 (underscores) is switched off for `tests/` in `tests/.editorconfig`
  for this reason. Tests written before this convention keep their old names until they are touched.
- The DataApi has two test projects:
  - `DataApi.UnitTests`: pure logic, no database and no Docker (sort parser, filters, link builder, argument checks).
  - `DataApi.IntegrationTests`: real Postgres and the real HTTP pipeline. **Docker must be running.**
    - `DataApiFactory` (a `WebApplicationFactory<Program>`) starts one Postgres container for the whole run (xunit
      assembly fixture), pinned to `DatabaseResource.PostgresImageTag` (the same image as the Aspire dev container),
      applies the migrations and passes the connection string with `UseSetting`. The app runs in the `Testing`
      environment. Do not remove or re-register services.
    - Tests derive from `IntegrationTestBase`: the database is reset with Respawn before each test (lookup tables with
      seed data are kept). Tests run sequentially, because they share the database.
- `Shared.UnitTests`: pure unit tests of `Shared` (`Types/`, `Text/`), mirroring its folders.
- In every test project, shared test helpers live in `Fixtures/` (e.g. `DataApiFactory`, `IntegrationTestBase`,
  `ArchitectureFixture`) and test data builders in `Seeds/` (e.g. `LeagueSeeds`). Neither name exists in `src`, because the
  other test folders mirror the `src` folders (tests of `Program.cs` sit in the project root).
    - Never use EF InMemory or SQLite: collation, `LIKE`, trigram search and null ordering differ from Postgres.
    - Per slice, mirror `Features/<Feature>/<UseCase>/` under `Features/` in the test project: handler tests
      (`new XxxHandler(dbContext)`) are the bulk, a few endpoint tests per slice cover the HTTP contract. Fake only
      external systems.
- `ArchitectureTests` use **ArchUnitNET** (`TngTech.ArchUnitNET.xUnitV3`): the assemblies are loaded once in
  `Fixtures/ArchitectureFixture` (`Model`), and a rule is `Types().That()...Should()...Check(Model)`. Test only rules
  the compiler cannot enforce (inside one assembly): a rule between projects is already enforced by `ProjectReference`.
  A rule needs positive results by default, so an empty match fails instead of passing silently
  (`WithoutRequiringPositiveResults()` only where the match set can legitimately be empty). `ResideInNamespace` is an
  exact match; use `ResideInNamespaceMatching` for a regex. Check that a new rule can fail by breaking it once.
  Behaviour (for example UUID v7 ids) belongs in the unit tests, not here.
- Expression trees cannot hold tuple literals or `is { }` patterns, so for those use `foreach` + `ShouldBe`, or `.Any(...).ShouldBeTrue()`, instead of `ShouldContain(predicate)`.
