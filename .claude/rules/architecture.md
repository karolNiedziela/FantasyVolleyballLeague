## Backend

### Project structure

Single API project (`FantasyVolleyballLeague.Api`). Do not split into separate Domain/Infrastructure
class libraries. All code lives inside:

    src/FantasyVolleyballLeague.Api/
    ├── Common/          # Shared plumbing (endpoint scanning, base types)
    ├── Domain/          # Shared entity classes (EF Core models)
    ├── Features/        # Vertical slices — one folder per feature
    └── Infrastructure/  # DbContext, EF configurations, DI wiring

### Vertical slice architecture

Each feature owns everything it needs. Organise by feature, not by technical layer.

    Features/<FeatureName>/<UseCaseName>/
    ├── <UseCaseName>Query.cs      # or Command.cs
    ├── <UseCaseName>Handler.cs
    ├── <UseCaseName>Response.cs   # or Request.cs
    └── <UseCaseName>Endpoint.cs

- One use case = one folder. Do not merge multiple use cases into one file.
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

### REST conventions

Follow the `dotnet-rest-api-rules` skill (resource naming, status codes, Problem Details,
pagination, URL versioning, OpenAPI). Where it conflicts with this file, this file wins:
- One `IEndpoint` class and one handler per use case, not a static `Map<Resource>` class
  and not a service layer behind an interface.
- Handlers read `DbContext` directly.


### Namespaces

Root namespace: `FantasyVolleyballLeague.Api`.
Follow folder structure: `FantasyVolleyballLeague.Api.Features.Rosters.GetRosters`, etc.

### Dependency injection

Use explicit constructor injection with `private readonly` fields. Do not use primary constructors.

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
