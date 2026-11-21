---
name: "ef-core-queries"
description: "Use when writing, reviewing or optimizing EF Core data access: read queries (filters, projections, paging, sorting, N+1), write paths (SaveChanges, batching, tracking, bulk updates, concurrency), indexes, migrations, and checking a query plan."
allowed-tools: "Read, Grep, Glob, Bash(dotnet build:*), Bash(dotnet test:*), PowerShell(dotnet build:*), PowerShell(dotnet test:*)"
---

# EF Core queries

Rules for reading and writing data with EF Core. They are adapted from the `dotnet-data:optimizing-ef-core-queries` plugin
skill and apply to any EF Core project. A project's own rules (`CLAUDE.md`, `.claude/rules/`) win over this skill: if a
rule does not fit the project, say so and follow the project.

## Workflow

1. Find the boundary: the `DbContext`, the query or the repository that runs it, and who calls it (a request handler, a
   background job, an import).
2. Trace the query to the SQL it produces. Count the statements and the rows each returns, and count the transactions and
   `SaveChanges` calls on the calling path. A helper that returns a list or `IEnumerable<T>` has already left the database,
   so operators after it run in memory.
3. Prove behaviour against the real database engine with a test. EF reports an untranslatable expression only when the query
   runs, and an in-memory provider or SQLite differs from the real engine in collation, `LIKE` and null ordering.
4. For a slow or non-trivial query, look at the plan (section 6). Add an index only when a plan or a measurement shows the need.
5. Separate what was demonstrated from what is an assumption. A source review does not prove a speedup; say what was not measured.

## 1. Read path

- Project straight to the type the caller needs with `Select`. Do not load whole entities to read a few columns, and do not
  return entities from an API. A scalar or DTO projection tracks nothing, so `AsNoTracking` adds nothing there. A DTO that
  holds an entity is still tracked.
- Keep filters, ordering, projection and aggregates on `IQueryable` until the final `ToListAsync`, `FirstOrDefaultAsync`,
  `AnyAsync` or `CountAsync`. Do not call `AsEnumerable`, `ToList` or LINQ-to-objects in the middle. Use `AnyAsync` for
  existence and `CountAsync` only when the number is needed.
- Compose query pieces as `IQueryable` extension methods named after what they do (`WhereActive`, `ProjectToSummary`), so
  several queries share a filter or a projection without calling each other.
- Order every paged or listed result by a unique key (add the primary key as the last `ThenBy`). A second `OrderBy` replaces
  the first; use `ThenBy` for a secondary key. `Skip`/`Take` without a unique order is not deterministic.
- Paging: offset (`Skip`/`Take`) is simple and fine for small, bounded lists, but deep pages scan and discard the skipped rows
  and shift under concurrent inserts. Prefer keyset (seek) paging for large or fast-growing data: order by a unique key and
  filter on the last seen value; for several sort columns the filter must combine them (`a < x || (a == x && id < y)`).
- N+1: the same `SELECT` repeated per row means a navigation or query inside a loop. Load related data in a fixed number
  of queries: project aggregates with `Select`, use `Include`, or load each child collection once and group in memory
  (`ToLookup`). Do not enable lazy loading.
- Several sibling collection `Include`s multiply rows into a cross product, and one collection can repeat a wide parent
  column many times. Prefer a narrow projection; otherwise consider `AsSplitQuery`, knowing it adds statements and is not one
  consistent snapshot. Do not split just because there are several `Include`s.
- Tracking: a read-only entity graph can use `AsNoTracking`; use `AsNoTrackingWithIdentityResolution` when shared entities
  must resolve to one instance. Keep tracking when the entities will be edited.
- Run one operation at a time on a `DbContext`. Overlapping calls (`Task.WhenAll` on one context) are not supported; use a
  context per concurrent operation, from a factory.

## 2. Write path

- One `SaveChanges` per unit of work. Stage the changes, then save once. A load-mutate loop followed by one save is not a
  round trip per row. Check for a `SaveChanges` hidden behind a repository, an audit hook or a progress callback inside a loop.
- Large imports: save in bounded batches and clear the change tracker after each, so it does not grow. With automatic change
  detection, `ChangeTracker.Entries()`, `HasChanges` and `SaveChanges` scan everything tracked, so do not inspect it per
  row; keep your own counters. `Add` and `AddRange` do not call `DetectChanges`, so swapping one for the other does not
  help. Rotate or dispose the context only when the batch owns it.
- Bulk changes: `ExecuteUpdateAsync` and `ExecuteDeleteAsync` change rows in one statement without loading them. Use them
  for large, simple changes that need no per-entity logic. They do not update tracked entities, do not check concurrency
  tokens on their own, run outside `SaveChanges` (so they are neither batched with it nor atomic with it), and skip
  `SaveChanges` hooks and client-side cascades. Do not mix them with tracked edits of the same properties.
- Idempotency: a retried or re-run write must not duplicate data. Upsert by a natural key and back it with a unique index;
  an existence check followed by an insert is not race-safe. Chunking alone is not idempotency.
- Concurrency: use a concurrency token (row version) for data several writers edit, and handle
  `DbUpdateConcurrencyException`. Know what a retrying execution strategy re-runs, and wrap a multi-step unit of work in
  `strategy.ExecuteAsync`.
- Transactions: know whether an enclosing transaction exists before assuming that several saves are atomic. One final commit
  does not undo earlier saves.
- A row that cannot be processed in a batch job is recorded and skipped; it does not abort the whole batch.
- Generated keys are available only after the save, so read them from the entities you added, not by assuming an order.

## 3. Predicates, search and indexes

- Compare a bare column, or a half-open range (`d >= start && d < start.AddYears(1)`, not `d.Year == y`). A function,
  arithmetic or a type conversion on an indexed column prevents an index seek.
- `Contains` and other unanchored substring matches cannot use an ordinary B-tree index. A prefix match can. For substring
  search use a provider feature built for it, such as a trigram index on PostgreSQL, and keep the same normalization in the
  query as in the stored column.
- Case-insensitive matching: use a normalized column or a column collation, not a function wrapped around the column.
  A query-specific collation can defeat an index built with another collation.
- Missing index versus missing evidence: a foreign key normally has an index on its columns unless another index or the
  primary key covers them. Look at the migrations and the database before claiming a gap. Do not pad a review with generic
  index advice.
- A new index goes on an evidenced query: equality columns first, then the range or sort column. An extra index does not fix
  an incompatible predicate, and every index slows writes.
- Hot query shapes can use `EF.CompileQuery`, which skips only the translation lookup and does not make the database work
  cheaper. Do not use it for occasional queries. Leave parameterization (`EF.Parameter`, `EF.Constant`) at the defaults
  unless a measured plan problem calls for it.

## 4. Migrations

- Create the migration with the project's `dotnet ef migrations add` command and read it before anything uses it.
- A new non-null column on a table that has rows needs a real default or a backfill before any foreign key. The default EF
  generates (`0` for an int) breaks a foreign key to a lookup table.
- Do not run `dotnet ef database update` (or anything else that writes to the database) without the user's approval. Show the
  migration and let the user apply it. Check whether the app applies migrations on start, and say so if it does.
- When EF does not own the schema, give the index or change to whoever manages the database.

## 5. Raw SQL

- Use `FromSql`, `FromSqlInterpolated` and `ExecuteSql` so values are sent as parameters. Never build SQL by concatenating
  user input.

## 6. Checking a query plan

- See the SQL EF generates by tagging the query (`.TagWith("name")`) and logging `Microsoft.EntityFrameworkCore.Database.Command`
  at `Information`, or `LogTo`. Compare the statement count and rows before and after a change.
- Run the plan on the database engine the project uses (`EXPLAIN (ANALYZE, BUFFERS)` on PostgreSQL, the actual execution plan
  on SQL Server) with realistic data, not an empty table. Run only reads and `EXPLAIN` against a shared or dev database, and
  never put a password on the command line or in the output.
- Read the plan for the scan type, rows removed by a filter, estimated versus actual rows, sorts, and buffers.
  A sequential scan on a small table is the right plan, not a finding. Stale statistics make estimates unreliable.
- Hand-written SQL is only equivalent to what EF generates; say so when you use it instead of the logged statement.
- Report what was measured and what was not.

## Before finishing, check

- [ ] Reads project to a DTO, with filters, ordering and paging in SQL; no entity leaves the data layer
- [ ] A fixed number of queries; no query inside a loop
- [ ] Ordering ends in a unique key
- [ ] Writes: one save per unit of work or per bounded batch; idempotent; concurrency and transactions considered
- [ ] Predicates compare bare columns, or use a purpose-built search index
- [ ] A test ran the query on the real engine
- [ ] A plan was checked for anything non-trivial, and no index was added without evidence
- [ ] A migration, if any, was read and not applied without approval
- [ ] `dotnet build` clean, `dotnet test` green
