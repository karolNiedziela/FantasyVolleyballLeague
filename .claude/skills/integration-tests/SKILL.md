---
name: integration-tests
description: Writing, fixing and reviewing .NET integration tests with xUnit, Shouldly, WebApplicationFactory and Testcontainers. Use when the user asks to add, change or review tests that need a real database, the ASP.NET Core HTTP pipeline or containers, or to change production code that already has integration tests. Not for unit tests.
model: sonnet
allowed-tools: Read, Grep, Glob, Bash(dotnet test:*), Bash(dotnet build:*), Bash(docker info:*), Bash(docker ps:*), PowerShell(dotnet test:*), PowerShell(dotnet build:*), PowerShell(docker info:*), PowerShell(docker ps:*)
---

# Integration tests (xUnit + Shouldly + WebApplicationFactory + Testcontainers)

The project's own conventions win over this skill: `CLAUDE.md`, `.claude/rules/`, `.editorconfig`, analyzers and the
style of the existing tests. Rules shared with unit tests (Arrange-Act-Assert, names, Shouldly, test data,
determinism) come from the `unit-tests` skill; this skill adds only what is different for integration tests.

**Litmus test:** the test would catch a bug that only shows up against the real database, configuration or HTTP
pipeline. If it would fail only because a rule in plain C# broke, it belongs in a unit test.

## Workflow

Pick the path that matches the request: write, fix, change the implementation or review.

**Running tests:** use the command the project documents (README, `CLAUDE.md`, build scripts). Otherwise run
`dotnet test <test project path>`; on .NET 10 with Microsoft.Testing.Platform the form is
`dotnet test --project <test project path>`. Use `--filter` to run a single test while working on it.
Docker must be running. If Testcontainers cannot reach Docker, say so and stop; do not fall back to an in-memory
database. Integration tests live in their own project (e.g. `App.IntegrationTests`), so unit tests run without Docker.

### Write new tests

1. Read the code under test and the existing integration test infrastructure: the `WebApplicationFactory`, container
   fixtures, reset, test authentication, builders. Reuse them; never add a second factory or a second container for
   the same database.
2. For each behaviour, choose the level (section 2). Show the list of behaviours, their level and the target test
   file, and wait for approval before writing.
3. If the infrastructure is missing, propose it as a separate step (factory, container fixture, reset) and wait for
   approval before writing tests on top of it.
4. Write the tests: one behaviour each, `[Theory]` for variations.
5. Run the new test class on its own and then with the whole test project. A test that passes only on its own leaks
   or depends on state.
6. Report what was not covered and why (belongs in a unit test, needs an external system we do not own).

### Fix a failing or flaky test

1. Run the failing test on its own and read the failure message. First tell an infrastructure failure (Docker,
   container start-up, migrations, connection string) from an assertion failure.
2. Decide whether the test or the code under test is wrong. If the code is wrong, report it and do not change the
   test to make it pass.
3. Change only the failing test, and shared infrastructure only when it is the cause. Do not restyle the rest of
   the file.
4. For a flaky test, look first for data shared between tests, a missing reset, parallel tests writing the same
   rows, background work that is not awaited (hosted services, outbox, message consumers) and time. Never fix it
   with a fixed delay or a retry; wait for an observable condition with a timeout.
5. Run the whole test project and confirm it is green.

### Change the implementation of covered code

Use when production code with existing behaviour changes (refactoring, a new feature in an existing endpoint or
query, a bug fix). The tests that passed before the change are the safety net: a test that fails after it is a
regression until shown otherwise, not an outdated test. This is the same path as in the `unit-tests` skill, with
the additions below for infrastructure.

1. Before the change, check that Docker is running (`docker info`), then run the integration tests of the affected
   area and note the result. If Docker is not available, say that the baseline could not be checked and ask before
   changing code without it; never skip this step silently. If anything is already red, stop and report it; tell an
   infrastructure failure from an assertion failure first.
2. Before the change, check that the tests cover the current behaviour of what you are about to change: the HTTP
   contract the client sees (status codes, Problem Details, JSON shape) and the stored state. For each gap, plan a
   characterization test at the level section 2 gives: it describes what the code does now, not what it should do.
   Show the gaps and the planned tests and wait for approval, then write them and run them on the old code until
   green. Cover only the behaviour the change touches. If the current behaviour looks like a bug, do not lock it in
   silently: report it and ask.
3. Make the change, then run the same tests with Docker running, first the area, then the whole test project. They
   must pass without being edited.
4. If a test fails, assume a regression and fix the code first. Change an existing test only when:
   - the behaviour change is intended (the user asked for it), or
   - the test was bound to the implementation (e.g. it compared the full `detail` sentence, read state through the
     application's own `DbContext`, or replaced one of our own services, sections 3 and 7); rewrite it to assert the
     contract instead.

   Report every existing test you changed: which test, what changed and why.
5. Add tests for the new behaviour separately (a new field in the response, the effect of a new dependency), following
   steps 2-5 of "Write new tests".

### Review existing tests

1. Read the tests, the code they cover and the shared infrastructure.
2. Check them against the checklist (section 8) and the anti-patterns (section 9).
3. Report the findings, most serious first: file, test, problem, fix. Do not change files unless asked.

## 1. What an integration test is

It verifies that our code works with the infrastructure it owns: the database, the HTTP pipeline (routing, model
binding, validation, authentication and authorization, filters, serialization), configuration and dependency
injection. Tests stay **narrow**: they exercise our service, and systems we do not own are replaced at the network
boundary.

| Real | Replaced | Never |
|---|---|---|
| Our database: the production engine and major version, in a container | External HTTP APIs we do not own (WireMock.Net) | EF Core InMemory provider |
| The application's DI container, middleware and configuration | The message broker, through the bus library's test harness, unless the broker integration itself is under test | SQLite in place of the production engine |
| EF Core migrations, applied to the test database | The identity provider, through a test authentication handler | A shared developer or test-environment database |
| Redis or other infrastructure the service owns, in a container | Time (`FakeTimeProvider`) | Real external services or production data |

- The database engine depends on the project (SQL Server or PostgreSQL): read it from the `DbContext` registration
  and use the matching Testcontainers module and image version.
- Keep the suite small. A behaviour that a unit test can prove is unit tested; integration tests cover what only
  real infrastructure can prove.
- Each test creates the data it needs and does not depend on the order of tests or on data left by another test.
- Containers start once per test assembly, never once per test or per class.

## 2. Through HTTP or against the database

| Behaviour | Level |
|---|---|
| Status codes, Problem Details, validation errors, headers | HTTP |
| Authentication and authorization (401, 403, policies, ownership of a resource) | HTTP |
| Routing, model binding, JSON contract (names, enums, dates, nulls) | HTTP |
| A use case end to end, including what it stores | HTTP |
| Query semantics: filtering, sorting, paging, projections | Database |
| Constraints: unique indexes, foreign keys, concurrency tokens | Database |
| Engine-specific behaviour: collation and case sensitivity, JSON columns, `EF.Functions` | Database |
| Transactions and code that manages its own transaction | Database |

- Default to HTTP for anything a client can reach through an endpoint: test the contract the client sees.
- Go to the database level when the rule lives in a query and proving it through HTTP would need many requests or
  much data, or when there is no endpoint.
- Test each rule at one level, not both.
- After an HTTP call, check stored state through the API (a following `GET`) where one exists; otherwise read it
  through a new DI scope and a new `DbContext`, never through the context the application used.
- If the project has an Aspire AppHost and the goal is to check several services working together, use
  `DistributedApplicationTestingBuilder` (`Aspire.Hosting.Testing`) with the resources the AppHost defines. It runs
  the application out of process and cannot replace services in dependency injection, so it does not replace the
  tests above.

## 3. Hosting the application: WebApplicationFactory

- Reference `Microsoft.AspNetCore.Mvc.Testing`. On .NET 10 and later the generated `Program` class is public and
  needs nothing; on earlier versions add `public partial class Program;` at the end of `Program.cs`.
- One factory for the application, in the test project's shared infrastructure folder. Tests receive it through the
  assembly fixture (section 4); they never create a `WebApplicationFactory` themselves.
- Exception: a single test that needs a different configuration (another environment, a low rate limit) derives a
  host from the shared factory with `factory.WithWebHostBuilder(...)` and disposes it at the end of the test. Each
  call starts a new host, so keep these tests rare.
- Point the application at the test database by overriding the connection string
  (`builder.UseSetting("ConnectionStrings:<Name>", connectionString)`). Do not replace the `DbContext`
  registration: the container runs the same engine as production, so only the address changes.
- Use `ConfigureTestServices` only for what section 1 lists as replaced: base addresses of external APIs,
  authentication, `TimeProvider`, the bus test harness. Do not replace our own services, repositories or handlers:
  then it is no longer an integration test of them.
- Set the environment explicitly (`builder.UseEnvironment(...)`) to the one the project uses for tests, so the
  result does not depend on the machine.
- Hosted services that are not under test and would touch the database or the network on start-up are removed in
  `ConfigureTestServices` by their implementation type, not with `RemoveAll<IHostedService>()`, which also removes
  the framework's own. A test of a hosted service starts it on purpose.
- Create clients with `factory.CreateClient()`. Pass `TestContext.Current.CancellationToken` to every call.
  Set `AllowAutoRedirect = false` in `WebApplicationFactoryClientOptions` when a test checks a redirect.

## 4. The database: Testcontainers

- Use the Testcontainers module for the engine the project uses (`Testcontainers.MsSql`, `Testcontainers.PostgreSql`)
  and pin the image to the production major version. Never use the `latest` tag.
- One fixture per test assembly owns the containers, the application host, WireMock and the reset tool, registered
  with xUnit v3 `[assembly: AssemblyFixture(typeof(...))]`. The simplest form is the factory itself:
  `sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime`. `InitializeAsync` starts the
  containers, applies the migrations and creates the `Respawner`; `DisposeAsync` disposes the host first, then the
  containers. (The `unit-tests` rule against fixtures does not apply here: sharing an expensive resource is exactly
  their purpose.) `DisposeAsync` overrides the factory's own and calls `base.DisposeAsync()` before disposing the
  containers.
- Build the schema with `Database.MigrateAsync()`, not `EnsureCreated()`, so the migrations themselves are tested.
  If the `DbContext` uses a retrying execution strategy, run it through `Database.CreateExecutionStrategy()`.
- Reset before each test: the fixture exposes one `ResetAsync()` that resets the database with Respawn and every
  replaced dependency (WireMock, Redis, `FakeTimeProvider`, section 5). A shared base class for the test classes
  (e.g. `IntegrationTestBase`) implements `IAsyncLifetime` and calls it in `InitializeAsync`, so no test class can
  forget it (xUnit creates a new instance for every test).
- Respawn: ignore `__EFMigrationsHistory` and reference data that migrations insert. Pass an open `DbConnection`;
  for PostgreSQL also set `DbAdapter = DbAdapter.Postgres`.
- Each test inserts the data it needs after the reset, through the API where an endpoint exists, otherwise through a
  builder that writes with a new `DbContext`.
- Parallelism: tests that share one database must not run at the same time. Turn parallelization off for the
  integration test assembly: `[assembly: Parallelization(Mode = ParallelMode.None)]` on xUnit v3 4.0 and later,
  `[assembly: CollectionBehavior(DisableTestParallelization = true)]` on earlier versions. Unit tests live in their
  own project and keep running in parallel.
- A new `DbContext` (for database-level tests, seeding and reading back) comes from a new DI scope of the factory
  (`factory.Services.CreateScope()`), so it has exactly the application's options: provider settings, naming,
  interceptors, retry strategy. Do not build `DbContextOptions` by hand in tests. The base class can offer helpers
  (`SeedAsync(...)`, `ExecuteDbContextAsync(...)`) that create the scope.

## 5. Dependencies outside the database

The rule from section 1: what our service owns runs for real in a container; what we do not own is replaced at
the network boundary. Each replacement is registered once, in the shared factory, and reset before every test.

### External HTTP APIs: WireMock.Net

- Start one `WireMockServer` in the assembly fixture and point the typed or named `HttpClient` at its `Url` through
  configuration (`UseSetting("<Service>:BaseAddress", server.Url)`). The application's own `HttpClient`
  registration, handlers and resilience policies stay real; that is what the test covers.
- Reset the mappings and the request log before each test (`server.Reset()`), as part of the fixture's reset.
- Each test sets up only the responses it needs:

```csharp
_fixture.ExternalApi
    .Given(Request.Create().WithPath("/rates/EUR").UsingGet())
    .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new { rate = 4.27m }));
```

- Cover the failure paths the application handles: error status codes, timeouts (`WithDelay` longer than the
  configured timeout), malformed bodies. These are the cases a unit test with a substitute cannot prove.
- Assert the outgoing request (`server.LogEntries`) only when sending it is the behaviour (e.g. a notification is
  sent with the right body), as with `Received` in unit tests.

### Messaging

- Use the bus library's test support: for MassTransit, `AddMassTransitTestHarness()` in `ConfigureTestServices`
  and `ITestHarness` from `factory.Services`; for another library, its own test support under the same rules.
  A real broker in a container (`Testcontainers.RabbitMq`, ...) only when the broker configuration itself is under
  test (topology, routing, retries).
- The harness collects messages for the whole run. Assert on the message that belongs to this test, by an id the
  test created, never just on the message type:

```csharp
(await harness.Published.Any<OrderSubmitted>(x => x.Context.Message.OrderId == orderId)).ShouldBeTrue();
```

- The harness waits for messages with a timeout; never add a fixed delay before asserting.

### Redis and other infrastructure we own

- A container from the matching Testcontainers module (`Testcontainers.Redis`, ...), started in the assembly
  fixture and pointed at through configuration, like the database.
- Clear it before each test, as part of the fixture's reset, so a cached value from one test cannot satisfy another.

### Time

- Replace `TimeProvider` with a `FakeTimeProvider` in `ConfigureTestServices` and expose it from the fixture. Each
  test sets the instant it needs with `SetUtcNow(...)`; the fixture's reset sets it back to a fixed default.

## 6. Authentication and authorization

- Replace the identity provider with a test authentication handler, registered in the shared factory as the
  default scheme. It builds the user from request headers, so each test chooses its user without shared state:

```csharp
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-User";
    public const string RolesHeader = "X-Test-Roles";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var roles = Request.Headers[RolesHeader]
            .SelectMany(value => value!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
```

```csharp
services.AddAuthentication(TestAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
```

- A request without the header is anonymous. Add a small helper in the shared infrastructure
  (`client.AsUser("user-1", "Admin")` or a request builder) so tests state the user and nothing else. User ids are
  fixed literals, never real people's data.
- If authorization policies name a scheme explicitly (`AuthenticationSchemes = "Bearer"`), the default scheme does
  not apply to them; make those policies use the test scheme in the factory, otherwise every test gets 401.
- What to test: 401 for an anonymous request, 403 for a user without the role or policy, and access to another
  user's resource (the most common real bug). The authorization rules are ours; the token validation is the
  framework's, so do not test JWT signing or the identity provider.
- Keep the real authentication only when the project has its own token logic (custom claims transformation, custom
  validation); then issue tokens signed with a test key instead of replacing the handler.

## 7. Asserting HTTP responses

- Assert the status code first, through a shared helper that puts the response body into the failure message
  (`await response.ShouldHaveStatusCode(HttpStatusCode.Created)`); a bare `StatusCode.ShouldBe(...)` hides why a
  request failed.
- Write URLs literally in the test (`"/api/v1/orders"`). The route is part of the contract, so a test that breaks
  when it changes is correct; this is the exception to the "no magic strings" rule.
- Read bodies with `ReadFromJsonAsync<T>` using the application's `JsonSerializerOptions` (exposed by the fixture),
  so string enums and dates deserialize the way clients see them. Use the API's response records, not EF entities.
- Check the JSON contract itself (camelCase names, enums as strings, the collection envelope) once per response type
  by reading raw JSON (`JsonNode`), not in every test.

| Response | Assert |
|---|---|
| 201 Created | `Location` header is set; then `GET` it and check the stored values (section 2) |
| 204 No Content | empty body; a following `GET` returns 404 after a delete |
| 400 validation | `application/problem+json`; read as `HttpValidationProblemDetails`; the keys in `Errors` name the invalid fields |
| 400 business rule, 404, 409 | `application/problem+json`; status and `type` or `title`, not the full `detail` sentence |
| 409 from a database constraint | send the conflicting request twice; the second gets 409, not 500 |
| Unhandled exception | not here: triggering one would mean replacing our own services. Prove the exception handler (no message or stack trace in production) with a unit test |
| Collection | the project's collection envelope (items, paging metadata, links); default page size applies; a page size above the maximum is clamped or rejected as the project decided |
| 429 | only in a test that configures a low limit on purpose, through `WithWebHostBuilder` (section 3); the shared factory sets limits high enough not to interfere |

## 8. Checklist before handing tests over

- [ ] Each behaviour needs real infrastructure to prove it; anything a unit test can prove is unit tested.
- [ ] The level (HTTP or database) follows section 2; each rule is tested at one level only.
- [ ] The shared factory and assembly fixture are reused; one container per engine, image pinned.
- [ ] The database and every replaced dependency are reset before each test; each test creates its own data.
- [ ] The test passes on its own and in the full run; parallelization is off for the assembly.
- [ ] Only systems we do not own are replaced, at the network boundary; our services run for real.
- [ ] Stored state is checked through the API first, otherwise through a new `DbContext` from a new DI scope.
- [ ] Responses are checked for status code and body or Problem Details, using the application's JSON options.
- [ ] No fixed delays; asynchronous work is awaited through a condition with a timeout.
- [ ] `TestContext.Current.CancellationToken` is passed to every call that takes a token.
- [ ] Arrange-Act-Assert, names and Shouldly follow the `unit-tests` skill.
- [ ] Docker is running and the test project is green with no analyzer warnings.
- [ ] After an implementation change: the baseline was green with Docker running, existing tests pass unedited, and
      every edited existing test is reported with a reason.

## 9. Reviewing tests: anti-patterns to flag

- EF Core InMemory, SQLite in place of the production engine, or a shared developer or test-environment database.
- Our own services, repositories or handlers replaced in `ConfigureTestServices`.
- A new `WebApplicationFactory` or container per test or per class; a second factory for the same application
  (a one-off `WithWebHostBuilder` for a test that needs different configuration is fine, section 3).
- `DbContextOptions` built by hand in tests instead of taking the `DbContext` from a DI scope.
- Tests that depend on execution order or on data created by another test; no reset between tests.
- Stored state read through the `DbContext` the application used, or checked in the database when the API exposes it.
- Only the status code checked where the body is the contract; whole error sentences compared.
- `Task.Delay` or `Thread.Sleep` to wait for background work or messages.
- Real external services or the internet reached from a test.
- Messages asserted by type only, so a message from another test can satisfy the assertion.
- Hard-coded connection strings, ports or `latest` image tags instead of the container's values and a pinned image.
- Authentication switched off (`AllowAnonymous`, removed `[Authorize]`) instead of a test handler, so 401 and 403
  cannot be tested.
- An integration test that repeats a rule already proved by a unit test.
- Everything listed in section 12 of the `unit-tests` skill.
