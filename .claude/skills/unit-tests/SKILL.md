---
name: unit-tests
description: Writing, fixing and reviewing .NET unit tests with xUnit, Shouldly and NSubstitute. Use when the user asks to add or change unit tests, to cover a class with tests, or to review existing unit tests. Not for integration tests (database, HTTP pipeline, containers).
model: sonnet
allowed-tools: Read, Grep, Glob, Bash(dotnet test:*), Bash(dotnet build:*), PowerShell(dotnet test:*), PowerShell(dotnet build:*)
---

# Unit tests (xUnit + Shouldly + NSubstitute)

The project's own conventions win over this skill: `CLAUDE.md`, `.claude/rules/`, `.editorconfig`, analyzers and the
style of the existing tests. Use the rules below where the project says nothing.

**Litmus test:** if the internals of the class are rewritten and its behaviour stays the same, the tests stay green.

## Workflow

Pick the path that matches the request: write, fix or review.

**Running tests:** use the command the project documents (README, `CLAUDE.md`, build scripts). Otherwise run
`dotnet test <test project path>`; on .NET 10 with Microsoft.Testing.Platform the form is
`dotnet test --project <test project path>`. Use `--filter` to run a single test while working on it.

### Write new tests

1. Read the code under test, the project's test conventions and the existing tests next to it. Reuse their helpers,
   builders and folder layout.
2. List the behaviours to cover: the main path, each rule or failure path, edge cases (empty, null, limits).
   Show the list and the target test file, and wait for approval before writing.
3. Write the tests: one behaviour each, `[Theory]` for variations.
4. Run the test project and fix until green with no analyzer warnings.
5. Report what could not be unit tested and why (needs the database, needs a seam).

### Fix a failing or flaky test

1. Run the failing test on its own and read the failure message.
2. Decide whether the test or the code under test is wrong. If the code is wrong, report it and do not change the
   test to make it pass.
3. Change only the failing test, and a shared helper only when the helper is the cause. Apply the rules below to the
   lines you touch; do not restyle the rest of the file.
4. For a flaky test, look first for time, randomness, culture, shared state or test ordering (sections 5 and 9).
   Never fix it with a delay or a retry.
5. Run the whole test project and confirm it is green.

### Review existing tests

1. Read the tests and the code they cover.
2. Check them against the checklist (section 11) and the anti-patterns (section 12).
3. Report the findings, most serious first: file, test, problem, fix. Do not change files unless asked.

## 1. What a unit test is

**Fast** (milliseconds), **isolated** (no database, network, HTTP pipeline or writable file system; read-only fixture
files are fine; no shared state), **repeatable** (same result on every run, machine and time zone) and
**self-checking** (passes or fails with no manual inspection). It verifies one behaviour of code we own.

- If the code needs a database or the HTTP pipeline to be tested, it is not a unit test: say so instead of faking
  the database (no EF InMemory, no SQLite, no substituted `DbContext`).
- If logic worth testing is buried in a class that needs infrastructure, extract it into its own class that takes
  and returns plain values or records, and test that class.

## 2. Names and layout

- The test file mirrors the source path (`src/App/Orders/Pricing.cs` → `tests/App.UnitTests/Orders/PricingTests.cs`);
  the namespace follows the folder. Class name: `<ClassUnderTest>Tests`, `sealed`.
- Test method names state behaviour, not implementation. Use the project's convention; if it has none, use
  `Subject_Should_Outcome`, with `_When_Condition` when the scenario matters:
  - `Apply_Should_RejectANegativeQuantity`
  - `TryHandleAsync_Should_HideTheDetail_When_TheEnvironmentIsProduction`
- Never `Test1`, `WorksCorrectly`, `HappyPath`, `Should_CallRepository`.

## 3. Arrange, Act, Assert

- **Never write `// Arrange`, `// Act` or `// Assert` comments.** The three blocks are separated by one blank line
  only.
- Exactly **one Act** per test. Two actions mean two tests (or a `[Theory]`).
- Put the Act on its own line and keep its result in a variable when Arrange or Assert are non-trivial.
- No logic in tests: no `if`, `switch`, loops, `try/catch` or string building to compute the expected value. Write
  the expected value literally. Repeated cases become a `[Theory]`.

```csharp
[Fact]
public void Apply_Should_RejectANegativeQuantity()
{
    var pricing = new Pricing();

    var exception = Should.Throw<ArgumentOutOfRangeException>(() => pricing.Apply(quantity: -1));

    exception.ParamName.ShouldBe("quantity");
}
```

## 4. One behaviour per test

- Several assertions are fine when they describe **one** behaviour (status code and body of one response).
- Unrelated checks go to separate tests, so a failure names the rule that broke ("assertion roulette").
- When several properties of one result must all be reported on failure, use `ShouldSatisfyAllConditions`:

```csharp
order.ShouldSatisfyAllConditions(
    () => order.Status.ShouldBe(OrderStatus.Confirmed),
    () => order.Total.ShouldBe(52.50m));
```

## 5. xUnit

- `[Fact]` for one case; `[Theory]` + `[InlineData]` for several inputs of one behaviour. Use `TheoryData<...>` with
  `[MemberData(nameof(...))]` when the data is not a compile-time constant (dates, records, collections).
- xUnit v3: pass `TestContext.Current.CancellationToken` to every async call that takes a token.
- Async tests are `async Task` and `await` the work. Never `Thread.Sleep` or fixed `Task.Delay` to wait. If there is
  nothing to await, the code needs a seam (or, as a last resort, poll a condition with a timeout).
- Setup:
  - Prefer private helper methods (`CreateSut()`, `Invoke(...)`) that each test calls, so the test shows what it
    depends on.
  - Use the constructor only for setup that **every** test in the class needs; a test overrides a default in its
    own Arrange. xUnit creates a new instance per test, so instance fields are fresh for each test.
  - No `static` mutable state. `static readonly` is fine for immutable values.
- No `[Collection]`, class or assembly fixtures: they share expensive resources, and a unit test has none.

## 6. Shouldly

Never use xunit `Assert.*`.

| Need | Idiom |
|---|---|
| Equality | `actual.ShouldBe(expected)` |
| Collection, in order | `keys.ShouldBe(["limit", "offset"])` |
| Collection, any order | `keys.ShouldBe(["limit", "offset"], ignoreOrder: true)` |
| Exactly one element | `items.ShouldHaveSingleItem()` (returns the item, keep chaining) |
| Type check + cast | `var problem = result.ShouldBeOfType<ValidationProblem>();` |
| Same instance | `result.ShouldBeSameAs(expected)` |
| Null / empty | `ShouldBeNull()`, `ShouldNotBeNull()`, `ShouldBeEmpty()`, `ShouldNotBeNullOrWhiteSpace()` |
| Bool | `handled.ShouldBeTrue()` |
| Text | `ShouldContain("between 1 and")`, `ShouldStartWith(...)`, `ShouldBe(..., StringCompareShould.IgnoreCase)` |
| Exception | `var ex = Should.Throw<T>(() => ...);` / `await Should.ThrowAsync<T>(() => ...)` |
| No exception | `Should.NotThrow(() => ...)` |
| Several conditions | `ShouldSatisfyAllConditions(...)` |

- Assert the exception type and the part of the message that matters, not the whole sentence.
- `ShouldContain(predicate)` takes an expression tree, which cannot hold tuple literals or `is { }` patterns. Use
  `.Any(...).ShouldBeTrue()`, or `ShouldHaveSingleItem()` and assert on the item.
- When the same multi-step assertion repeats across tests (e.g. on a `Result<T>`), add a small extension
  (`ShouldBeSuccess()`) in the test project's shared helpers, so the failure message shows the actual error.

## 7. NSubstitute

Substitute **only interfaces at a boundary of the code under test**: external systems (HTTP clients, e-mail,
message buses, file system), framework services that need a host (`IHostEnvironment`, `IProblemDetailsService`),
time and randomness.

Do **not** substitute:
- domain entities, value objects, records, DTOs: create real instances;
- pure static helpers, mappers, validators and other cheap deterministic collaborators: use the real ones;
- the database context or repositories over it in code whose job is the query: that is an integration test;
- `ILogger<T>`: use `NullLogger<T>.Instance`.

```csharp
private readonly IHostEnvironment _environment = Substitute.For<IHostEnvironment>();

_environment.EnvironmentName.Returns(Environments.Production);
client.GetAsync(id, Arg.Any<CancellationToken>()).Returns(response);

await emailSender.Received(1).SendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
```

- Assert the **observable outcome** (returned value, state, response), not which methods were called. `Received`
  and `DidNotReceive` are for boundary side effects that are the behaviour itself (an e-mail is sent, a message is
  published), never for internal steps (`Save` then `Flush`).
- `Arg.Any<T>()` for arguments the test does not care about; `Arg.Is<T>(x => ...)` sparingly. If matching gets
  complicated, capture the argument (`Arg.Do<T>(x => captured = x)` or `ReceivedCalls()`) and assert on it.
- A substitute that only supplies values is a **stub**; one the test asserts against is a **mock**. Name fields after
  their role (`_environment`, `_emailSender`), not `mockX`.
- More than two or three substitutes for one class is a design smell (too many dependencies). Say so instead of
  adding more.
- Only interfaces and virtual members can be substituted. A non-virtual member silently keeps its real behaviour.

## 8. Test data

- Use the minimal data that proves the behaviour. Do not set properties the test does not read.
- No magic numbers or strings: give values a name that explains them (`private const int MaxQuantity = 100;`,
  `Limits.Max + 1`).
- Build objects with a small private factory when only one test class needs it, or a builder with defaults and
  `WithX(...)` methods in the test project's shared folder when several do. Tests state only the values they care
  about; a constructor change then breaks one helper, not every test.
- No random generated data (it breaks repeatability) and no real personal data (national ids, e-mail addresses,
  phone numbers).

## 9. Determinism

- No `DateTime.Now`/`UtcNow`, `DateTimeOffset.Now`, `Random` or environment reads in the code under test. Inject
  `TimeProvider` and use `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) with `SetUtcNow(...)` in
  tests; add the package the first time it is needed.
- Ids: tests that compare ids use fixed values, never freshly generated ones.
- Dates are literal (`new DateOnly(2025, 1, 30)`), instants are UTC or carry an explicit offset.
- Culture and time zone: code that parses or formats passes a culture explicitly, and the test covers it. A test
  must pass on a machine with any regional settings.

## 10. What not to test

- Private methods: test them through the public member that uses them. If one is complex enough to need its own
  tests, extract a class with a public contract.
- Do not reach into private members with reflection, and do not add `InternalsVisibleTo` only to test
  implementation details. (A project that exposes internal types to tests on purpose is fine.)
- Framework and library behaviour (JSON serializer, EF Core change tracking, routing): test our own rules, not that
  the library works.
- Records and DTOs without logic, trivial getters, generated code.
- Coverage percentage is not a goal. A test must protect a rule someone could break.

## 11. Checklist before handing tests over

- [ ] File and namespace mirror the source; class is `sealed`; names follow the project convention.
- [ ] AAA separated by blank lines, one Act, no logic, no AAA comments.
- [ ] One behaviour per test; repeated cases are a `[Theory]`.
- [ ] Shouldly only; exceptions checked with `Should.Throw(Async)`.
- [ ] Substitutes only for boundaries; outcome asserted, not interactions (unless the interaction is the behaviour).
- [ ] No time, randomness, culture or shared static state leaks into the result.
- [ ] Comments only where something is non-obvious.
- [ ] `dotnet test` passes with no analyzer warnings; a test written for a bug fails before the fix.

## 12. Reviewing tests: anti-patterns to flag

- Private members tested through reflection, or a private helper asserted to be called.
- Over-mocking: domain objects, records or pure helpers substituted.
- `Received(...)` on internal steps instead of asserting the outcome.
- Giant setup that hides the values that matter (use a helper or builder).
- `if`, loops, `switch` or string building in a test.
- `Thread.Sleep` or fixed delays.
- Shared mutable state; tests that depend on execution order.
- Time, randomness or culture leaking into the result.
- Several behaviours in one test, or several Acts.
- Magic literals; more input data than the behaviour needs.
- Tests of framework or library behaviour instead of our own rules.
- `// Arrange` / `// Act` / `// Assert` comments; `Assert.*` instead of Shouldly; names that do not follow the
  convention.
