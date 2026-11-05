---
name: scraper-sources
description: Workflows for the FantasyVolleyballLeague Scraper - fixing a parser after a league website changed, adding a new league or source (another country or website), and checking the quality of scraped data in data/raw. Use when scraped data is wrong or missing, a scrape fails, a new league or source is requested, or the user asks to verify scraped data.
---

# Scraper sources

Read first: `src/FantasyVolleyballLeague.Scraper/README.md` (options, data layout, sources) and
`.claude/rules/project-overview.md`. Code rules: `.claude/rules/architecture.md`.

Always present the diagnosis and the full list of files to change, and wait for the user's approval before
editing (user preference). Delegate reading raw web pages to the `html-page-analyst` agent; do not load
whole HTML pages into this conversation.

## A. A parser broke after a website change

1. **Reproduce.** Run the affected season, e.g.
   `dotnet run --project src/FantasyVolleyballLeague.Scraper -- --job Matches --league <Key> --from <year> --to <year> --force true`.
   Collect: error messages, `FailedMatchIds` in `data/raw/matches/<Key>/<season>/_manifest.json`, and the data
   quality check (section C).
2. **Analyse.** Ask `html-page-analyst` to compare the current page with the matching fixture in
   `tests/FantasyVolleyballLeague.Scraper.Tests/Fixtures/` and to locate the data the parser reads.
3. **Propose.** Diagnosis, the parser change, the fixture to update and the test to add. Wait for approval.
4. **Implement test first.** Save the new page as a fixture (keep the old one if older seasons still use the
   old format), add a failing test, then fix the parser.
5. **Verify.** `dotnet build FantasyVolleyballLeague.sln` (analyzer warnings are errors), `dotnet test`,
   re-scrape the season with `--force true`, run section C, and run the Importer.

## B. A new league or source

- **Same website system** (e.g. another PLS league): add the league to `Leagues` in the Scraper
  `appsettings.json` with the existing `Source` key, and to `Importer:Leagues` (key, name, country, gender,
  division). Verify with `--current true`.
- **New website system:**
  1. Ask `html-page-analyst` to map the website: season list, season schedule (phases, stages, rounds),
     match page (header, statistics, formats across seasons), clubs, squad and player pages, URL patterns and
     which ids are stable across seasons.
  2. Propose a design: `src/FantasyVolleyballLeague.Scraper/Sources/<Country>/<Provider>/` with a class implementing
     `ILeagueSource` (unique `Key`), the mapping to `Models/` (`MatchRecord`, `RosterRecord`, `ScheduleEntry`,
     `Season`), which fields stay `null`, and the configuration entries. A new country also needs a
     `LeagueCountry` entry in the Api domain and an EF Core migration. Wait for approval.
  3. Implement following `Sources/Poland/Pls/` as the reference: static parsers that take an `HtmlDocument`
     (testable without a browser), Playwright only for pages that need a browser, a `<Provider>Urls` class
     for URL patterns, registration `builder.Services.AddScoped<ILeagueSource, ...>()` in `Program.cs`.
  4. Tests: save real pages as fixtures (one per page type and format) and test every parser.
  5. Verify as in A.5, then scrape one past season and the current season.

The raw JSON format is shared by all sources. If a source needs a new field, change `Models/` and the
Importer `Records/` together, keep the field optional, and discuss with the user whether `SchemaVersion`
changes.

## C. Check scraped data quality

```powershell
cd data/raw/matches/<Key>/<season>
$m = Get-ChildItem *.json -Exclude _manifest.json | ForEach-Object { Get-Content $_ -Raw -Encoding utf8 | ConvertFrom-Json }
$f = $m | Where-Object Status -eq 'Finished'
$p = $f | ForEach-Object { $_.HomeRoster.Players + $_.AwayRoster.Players }
[pscustomobject]@{
  Matches = $m.Count; Finished = $f.Count
  BadSets = ($f | Where-Object { $_.Sets.Count -ne ($_.HomeRoster.SetsWon + $_.AwayRoster.SetsWon) }).Count
  NoNumber = ($p | Where-Object { $null -eq $_.Number }).Count
  NoPlayerId = ($p | Where-Object { $null -eq $_.ExternalId }).Count
  NoClubId = ($m | Where-Object { -not $_.HomeRoster.ExternalId -or -not $_.AwayRoster.ExternalId }).Count
  Levels = ($m | Group-Object StatisticsLevel | ForEach-Object { "$($_.Name)=$($_.Count)" }) -join ','
}
```

Expected for PLS: `BadSets = 0`, `NoClubId = 0`; `NoPlayerId` is the number of players in widget-era seasons
(2019/20+, resolved later through rosters by the Importer); `LineupOnly` only for 2000/01–2007/08. Also check
`FailedMatchIds` in the manifest and, after an import, `UnresolvedPlayers` in `data/import-report.json`.

## Lessons from the PLS source (apply to every source)

- Scope selectors to the main container; the same attributes can appear in tickers or navigation.
- Static HTML often contains every filtered section, so read it instead of clicking through filters.
- Decode entities (`HtmlEntity.DeEntitize`) and parse with `HtmlNodeExtensions.ParseIntOrNull`, never `int.Parse`.
- Store dates as UTC; record whether the time is confirmed.
- Keep `null` for data the source does not have; never write zeros or `-1` as placeholders.
- Take stable ids from links; names are not identifiers (order and spelling vary).
- Normalise names of phases, stages and rounds deterministically, so re-scrapes produce the same rounds.
- Be polite: keep `Playwright:MaxConcurrentTasks` low and do not re-scrape what `data/raw` already has.
