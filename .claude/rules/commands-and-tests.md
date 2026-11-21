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

### Query plans on the dev database

The dev Postgres runs in the `fantasy-volleyball-league-postgres` container (database `FantasyVolleyballLeagueDb`). Use the
container's own password, so no secret is printed, and only run `SELECT` and `EXPLAIN` there:

```bash
docker exec -i fantasy-volleyball-league-postgres sh -c \
  'export PGPASSWORD="$POSTGRES_PASSWORD"; psql -U "$POSTGRES_USER" -d FantasyVolleyballLeagueDb' < plan.sql
```

`plan.sql` holds `EXPLAIN (ANALYZE, BUFFERS, COSTS OFF) <query>;`. Test on the largest data (PlusLiga matches, about 4.6k, and the
largest season). Dev statistics can be stale, so a `Seq Scan` on a small table is expected. Do not run `ANALYZE`, DDL or writes
there without asking. The `ef-core-queries` skill describes how to read the plan.

## Tests

Run all with `dotnet test --solution FantasyVolleyballLeague.sln`, one with `dotnet test --project tests/<Project>`.
`DataApi.IntegrationTests` needs Docker running. `ArchitectureTests` also fail when a model change has no migration.
