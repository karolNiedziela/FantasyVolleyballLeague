# 🏐 Fantasy Volleyball League

A volleyball data platform and, later, a fantasy game in the style of Fantasy Premier League: users build squads
of real players and score points from their real match statistics.

The current focus is the data: matches, results, player statistics, upcoming fixtures and historical seasons of
the Polish men's leagues (TAURON Liga, formerly PlusLiga, from 2000/01 and TAURON 1. Liga from 2018/19).

## ✨ Target features

### 📊 Volleyball data
- Matches, set scores and full player statistics, including historical seasons
- Upcoming fixtures
- Players tracked across seasons and clubs
- Rosters per season

### 🎮 Fantasy league
- Create your own volleyball fantasy team from real players
- Score points based on players' real match performance
- Leaderboards, transfers and lineups

## ⚙️ How it works

The data is collected from the official league websites, stored as raw files and loaded into a database. A public
read-only API serves it to third parties. The database can be rebuilt from the raw files at any time, without visiting
the websites again.

## 🚀 Getting started

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download) and
[Docker Desktop](https://www.docker.com/products/docker-desktop/).

```powershell
# 1. Start PostgreSQL and the Api (applies the database migrations); keep it running
dotnet run --project FantasyVolleyballLeague.AppHost

# 2. Install the Playwright browser used by the Scraper (once per Playwright version)
dotnet build src/FantasyVolleyballLeague.Scraper
pwsh src/FantasyVolleyballLeague.Scraper/bin/Debug/net10.0/playwright.ps1 install chromium

# 3. Scrape data: rosters first, then matches (the full history takes a few hours)
dotnet run --project src/FantasyVolleyballLeague.Scraper -- --job Rosters --from 2018
dotnet run --project src/FantasyVolleyballLeague.Scraper -- --job Matches

# 4. Import into the database (or start the "importer" resource in the Aspire dashboard)
dotnet run --project src/FantasyVolleyballLeague.Importer
```

For a quick try, scrape only the current season: `--job Matches --current true`. To keep the data fresh, start
`scraper-matches`, `scraper-rosters` and `importer` from the Aspire dashboard. See the
[Scraper](src/FantasyVolleyballLeague.Scraper/README.md) and
[Importer](src/FantasyVolleyballLeague.Importer/README.md) documentation for all options.
