# FantasyVolleyballLeague

@rules/project-overview.md

@rules/architecture.md

@rules/commands-and-tests.md

## Claude tooling

- Skills (`.claude/skills`): `scraper-sources` (fix a parser after a website change, add a league or source, verify
  `data/raw`), `dotnet-rest-api-rules` (REST conventions),
  `commit-message` (Conventional Commits), `unit-tests` (writing, fixing and reviewing unit tests: xUnit, Shouldly,
  NSubstitute), `integration-tests` (tests against real Postgres and the HTTP pipeline: WebApplicationFactory,
  Testcontainers, Respawn).
- Tests: choose the skill by what the behaviour needs, not by the project: `unit-tests` by default,
  `integration-tests` only for what needs Postgres or the HTTP pipeline. Test each rule at one level.
- Agent `html-page-analyst`: read-only analysis of league web pages (selectors, URL patterns); use it instead of reading
  large HTML in the main conversation.
