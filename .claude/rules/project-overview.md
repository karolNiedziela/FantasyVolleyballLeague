## Project overview

### Vision

FantasyVolleyballLeague aggregates data about volleyball leagues: matches, players, upcoming fixtures,
and historical seasons. Later it becomes a fantasy game in the style of Fantasy Premier League, where
users build squads of real players and score points from their real match statistics.

Current focus: **Polish men's leagues** and the data pipeline (scraping → raw JSON → Postgres).
Women's leagues, API endpoints and the fantasy game itself are out of scope for now.

### Data sources

The Scraper scrapes each league through a **source**: the website system the data comes from, identified by its
HTML structure and URL patterns. A league selects it with `Leagues:Source` in `Scraper/appsettings.json`.
Every source implements `Scraper/Sources/ILeagueSource` and lives in `Scraper/Sources/{Country}/{Provider}/`;
`LeagueSourceResolver` picks it from all registered sources (no service locator) and the Scraper fails at
startup on an unknown source. One source can serve several leagues, and a new country or website means a new
source folder, not changes in the data processors or the raw JSON format.

**`Pls` (Poland)**: PLS is Polska Liga Siatkówki S.A., the company operating the Polish professional leagues
(TAURON Liga, TAURON Liga Kobiet, TAURON 1. Liga, TAURON 1. Liga Kobiet). All their websites run on the same
PLS CMS ("PLS S.A." in the footer, pls.pl), so they share the HTML structure, URL patterns and match ids.
Code: `Scraper/Sources/Poland/Pls/` (classes prefixed `Pls`). The domains were rebranded for the 2026/27
season, and the old domains now return 301 redirects:

| Config key | League | Current domain | Data from |
|---|---|---|---|
| `PlusLiga` | Men, division 1 ("TAURON Liga", formerly PlusLiga) | `https://www.tauronliga.pl` (was plusliga.pl) | 2000/01 |
| `PLS1Liga` | Men, division 2 ("TAURON 1. Liga", formerly PLS 1 Liga) | `https://www.tauron1liga.pl` (was pls1liga.pl) | 2018/19 |
| `TauronLigaKobiet` | Women, division 1 ("TAURON Liga Kobiet") | `https://www.tauronligakobiet.pl` (tauronliga.pl used to be the women's site) | 2008/09 |
| `Tauron1LigaKobiet` | Women, division 2 ("TAURON 1. Liga Kobiet") | `https://www.tauron1ligakobiet.pl` | 2026/27 (single season: the page has a button instead of a season dropdown) |

The women's sites link players as `/zawodnicy/id/{playerId}/...` instead of `/players/id/...`. Player ids are
unique only within one gender, so `Player` is keyed by `(GenderId, ExternalId)`.

Config keys are stable identifiers used for folder names. They stay the same when the sponsor brand
changes.

PLS URL patterns (`PlsUrls`):
- Season list: `/games.html`. The season dropdown links look like `/games/tour/{tour}/nocookies/1.html`. `tour` is the season id; 53 = 2026/27.
- Match: `/games/action/show/id/{matchId}.html`. Match ids are global across PLS sites.
- Team: `/teams/id/{clubId}/tour/{tour}.html`. The club id is stable across seasons. The site navigation also contains `/teams/id/` links, so selectors must be scoped to the match header.
- Player: `/players/id/{playerId}/tour/{tour}.html`. The player id is stable across seasons.

PLS match page eras (each has its own parser in `Scraper/Sources/Poland/Pls/Matches`; the header
`PlsMatchHeaderParser` works for all eras):
- **Archive (2000/01–2007/08)**: lineup only (set participation, no counting stats). Player links are present.
- **Table-colored (2008/09–2018/19)**: `table.table-colored`, full stats. Player links are present. The shirt number is `<span>&nbsp;1 &nbsp;</span>`, so always `HtmlEntity.DeEntitize` it.
- **Widget (2019/20+)**: stats are in volleystation iframes (`iframe.widget-team-a|b`, `widgets.volleystation.com/team-stats/{id}`), rendered client-side. **There are no player ids.** Players are resolved through the season roster by shirt number and name.

Known pitfalls:
- Name order differs between sources. Rosters and the MVP use "First Last", widget stats use "Last First". Compare names as an order-, case- and diacritics-insensitive token set.
- Dates on the page are Polish local time (`dd.MM.yyyy, HH:mm`) and are stored as UTC.

### Solution map

| Project | Role |
|---|---|
| `FantasyVolleyballLeague.Api` | ASP.NET Core minimal API (vertical slices, no endpoints yet). Holds the **domain** (`Domain/`), the EF Core `FantasyVolleyballLeagueDbContext` and the migrations (Postgres). Applies migrations on startup in Development. |
| `FantasyVolleyballLeague.Scraper` | Playwright + HtmlAgilityPack **scraper** CLI. Source-independent processors (`DataProcessors/`), shared raw models (`Models/`) and per-source scrapers (`Sources/`). Writes raw JSON to `data/raw`. |
| `FantasyVolleyballLeague.Importer` | **Importer** console app. Reads `data/raw`, upserts into Postgres using the Api's DbContext (`InternalsVisibleTo`). |
| `FantasyVolleyballLeague.AppHost` | Aspire: Postgres container, Api (healthy once migrations are applied), and explicit-start resources started from the dashboard: `scraper-matches` (current season matches), `scraper-rosters` (current season rosters) and `importer` (waits for the Api). Historical backfills run from the Scraper CLI. |
| `FantasyVolleyballLeague.ServiceDefaults` | Aspire service defaults (OpenTelemetry, health checks). |

### Data pipeline

```
tauronliga.pl / tauron1liga.pl ──(Scraper, Playwright)──► data/raw/*.json ──(Importer)──► Postgres
```

- **JSON is the raw layer and the source of truth for re-imports.** When the domain model changes,
  re-run the importer. Do not re-scrape. `data/` is gitignored.
- Layout:
  - `data/raw/matches/{LeagueKey}/{YYYY-YYYY}/{matchId}.json`
  - `data/raw/matches/{LeagueKey}/{YYYY-YYYY}/_manifest.json`
  - `data/raw/rosters/{LeagueKey}/{YYYY-YYYY}/{clubId}.json`
- Files are self-describing (`SchemaVersion: 1`): each file carries its league, season, phase, stage
  and round context. The importer never derives context from folder names.
- Matches have a `Status` of `Scheduled` or `Finished`. Seasons in progress are re-scraped, and a
  season is `Complete` only when it is not the latest season, has no scheduled matches and has no
  failures.
- The importer is idempotent. It upserts by natural keys and skips matches whose `ScrapedAtUtc` is
  not newer than the stored `Match.SourceScrapedAt`.

### Domain model

League → Season → Phase → Round (with `Stage`) → Match → SetScore / PlayerMatchStatistics.

- `Roster` is a club **in a given season** with its players. Its `ExternalId` is the club id. Matches are
  played by `HomeRoster` and `AwayRoster` (the same names are used in the raw JSON).
- `Player` is a **global person**. Its `ExternalId` is the player id. `RosterPlayer` is the season
  roster entry (shirt number, position).
- Every entity derives from `Entity`, which assigns a time-ordered UUID v7 id (`EntityId.New()`).
  `Guid.NewGuid()` is banned (`BannedSymbols.txt`, RS0030 is a build error), EF never generates keys, and
  PostgreSQL defaults ids to `uuidv7()`. `tests/FantasyVolleyballLeague.ArchitectureTests` guards these rules.
- Player resolution order: player id → roster shirt number confirmed by name → roster name → an
  unresolved player without `ExternalId`, listed in the import report.
