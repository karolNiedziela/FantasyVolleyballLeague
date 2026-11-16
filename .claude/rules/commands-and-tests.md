## Commands

Prerequisites: .NET 10 SDK and Docker Desktop. All commands run from the repository root (PowerShell).

```powershell
# Postgres + DataApi (applies migrations); explicit-start resources are started from the dashboard
dotnet run --project FantasyVolleyballLeague.AppHost

# Playwright browser for the Scraper (once per Playwright version)
dotnet build src/FantasyVolleyballLeague.Scraper
pwsh src/FantasyVolleyballLeague.Scraper/bin/Debug/net10.0/playwright.ps1 install chromium

# Scraper: rosters first (widget-era players are resolved through them), then matches
dotnet run --project src/FantasyVolleyballLeague.Scraper -- --job Rosters --from 2018
dotnet run --project src/FantasyVolleyballLeague.Scraper -- --job Matches --current true

# Importer (idempotent)
dotnet run --project src/FantasyVolleyballLeague.Importer

# Add an EF migration
dotnet ef migrations add <Name> --project src/FantasyVolleyballLeague.VolleyballData --startup-project src/FantasyVolleyballLeague.DataApi
```

Scraper options: `--job Matches|Rosters`, `--league <key>`, `--from <year>`, `--to <year>`, `--current true`,
`--force true`, `--max-attempts <n>`. Booleans take an explicit value. A season is identified by its start year.
Full details are in the Scraper and Importer READMEs.

### Local setup

- The Importer run from the command line needs the gitignored `src/FantasyVolleyballLeague.Importer/appsettings.local.json`
  with `ConnectionStrings:FantasyVolleyballLeagueDb`. The Postgres password is in the AppHost user secrets
  (`dotnet user-secrets list --project FantasyVolleyballLeague.AppHost`, key
  `Parameters:fantasy-volleyball-league-postgres-password`). Started from Aspire, it gets the connection string itself.
- The `.http` files of the DataApi use REST Client variables from `src/FantasyVolleyballLeague.DataApi/http-client.env.json`
  (`DataApi_HostAddress`, `ApiVersion`).

## Tests

Run all with `dotnet test --solution FantasyVolleyballLeague.sln`, one with `dotnet test --project tests/<Project>`.
`DataApi.IntegrationTests` needs Docker running. `ArchitectureTests` also fail when a model change has no migration.
