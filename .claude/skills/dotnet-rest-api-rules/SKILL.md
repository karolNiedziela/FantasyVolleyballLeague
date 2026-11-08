---
name: "dotnet-rest-api-rules"
description: "Use when designing, implementing or reviewing ASP.NET Core REST/HTTP APIs: resource naming, status codes, Problem Details errors, pagination, URL versioning, OpenAPI."
---

# .NET REST API rules

Pragmatic rules for ASP.NET Core HTTP APIs. They reflect what Microsoft's official dotnet-aspnetcore skills, Milan Jovanovic, Mukesh Murugan and SSW agree on. Treat them as defaults: if one does not fit the project, say so, choose an alternative, and apply that alternative everywhere.

## Workflow

1. Inspect `Program.cs` and existing endpoints. Continue in the project's current style (controllers or minimal APIs). In a new project use minimal APIs. Never mix both styles.
2. List the resources, their relationships and operations before writing any endpoint.
3. Implement the endpoints using the rules below.
4. Add OpenAPI metadata and a `.http` file with one request per endpoint, including error cases.
5. Verify: `dotnet build` with zero warnings, run the app, check status codes and the OpenAPI document.

## Rules

### 1. Be consistent
Whatever convention the API uses (plural nouns, one error format, one status code for validation), use it on every endpoint. When in doubt, copy the nearest existing endpoint.

### 2. Resource naming and URLs
- Plural nouns, no verbs: `/api/v1/invoices`, never `/getInvoice`.
- Lowercase paths; kebab-case for multi-word segments.
- At most one level of nesting. Two levels is a red flag: expose a top-level resource and filter by the parent id instead (`/entries?habitId=...`, not `/users/{id}/habits/{id}/entries`).
- JSON property names are camelCase (the .NET default).

### 3. HTTP methods
- GET reads, POST creates, PUT replaces, DELETE removes. PUT and DELETE must be idempotent.
- PATCH is optional. For partial updates prefer a dedicated sub-resource endpoint (for example `POST /invoices/{id}/cancel`) unless the project already uses PATCH.
- For POST requests that clients may retry with side effects (payments, invoices), accept an `Idempotency-Key` header.

### 4. Status codes
| Case | Code |
|---|---|
| Read OK / update OK | 200 |
| Created | 201 with `Location` header pointing to the new resource |
| Accepted for async work | 202 |
| Deleted / no body | 204 |
| Validation error and business-rule failure | 400 with Problem Details |
| Not authenticated / not allowed | 401 / 403 |
| Not found | 404 |
| Conflict (unique constraint, concurrency) | 409 |
| Rate limit exceeded | 429 with `Retry-After` |

ASP.NET Core returns 200 by default, so set codes deliberately. Do not return 500 yourself.

### 5. Errors
- One error format everywhere: Problem Details (RFC 9457, which replaces RFC 7807), media type `application/problem+json`.
- Register `AddProblemDetails()`, `UseExceptionHandler()` and `UseStatusCodePages()`. Use an `IExceptionHandler` for custom exception-to-status mapping, and log the exception before returning true.
- Never return stack traces or `exception.Message`. Add a trace id for support.
- Field-level validation errors go in an `errors` extension on the Problem Details body.

### 6. Collections: envelope and pagination
- Never return a bare array. Return an object so the API can grow without breaking clients: `{ "data": [...], "pagination": {...}, "links": {...} }`.
- Every collection endpoint is paginated, with a default and maximum page size.
- Offset pagination (`page`, `pageSize`) is fine for small data. Prefer cursor (keyset) pagination for large or fast-growing data; the cursor is an opaque string.
- Include `first`/`next` links when helpful; build them with `LinkGenerator`.

### 7. Versioning
- Use URL versioning with the `Asp.Versioning.Http` (and `.Mvc` / `.Mvc.ApiExplorer` for controllers) packages: `/api/v1/...`.
- Adding a field is not a breaking change; removing or renaming one is.
- Mark old versions deprecated and announce a retirement date before removing them.

### 8. Types and serialization
- Separate request and response types as `sealed record`s. Never expose EF Core entities.
- Naming: `CreateInvoiceRequest`, `InvoiceResponse`.
- Dates are `DateTimeOffset`. Enums serialize as strings (`JsonStringEnumConverter`).
- Add XML `<summary>` comments to API types so they flow into OpenAPI.

### 9. Endpoint implementation
- Every endpoint accepts a `CancellationToken` and passes it to all async calls.
- Minimal APIs: use `TypedResults` with explicit `Results<T1, T2>` return types; group endpoints per resource in a static class with a `Map<Resource>` method.
- Minimal APIs on .NET 10+: call `AddValidation()` so data annotations are enforced.
- Keep handlers thin: put logic in a service behind an interface.

### 10. OpenAPI and documentation
- .NET 9+: built-in `AddOpenApi()` and `MapOpenApi()`. Do not add Swashbuckle to .NET 9+ projects.
- Add summary, description and `Produces` metadata (or typed results) to every endpoint so all status codes are documented.

### 11. Security and protection
- HTTPS only. Every endpoint is authenticated unless explicitly public; use role or policy authorization.
- Use the built-in rate limiter (`AddRateLimiter` plus `UseRateLimiter`, available since ASP.NET Core 7) on public endpoints. Its default rejection code is 503, so set `RejectionStatusCode = StatusCodes.Status429TooManyRequests` explicitly.
- Never put secrets, tokens or personal data (PESEL, NIP) into URLs or logs.

## Before finishing, check
- [ ] Plural nouns, max one level of nesting, camelCase properties
- [ ] Correct status codes, 201 has `Location`
- [ ] All errors are Problem Details; no stack traces
- [ ] Collections are enveloped and paginated
- [ ] URL version in the route
- [ ] DTOs are sealed records; no entities exposed
- [ ] `CancellationToken` everywhere
- [ ] OpenAPI generated; `.http` file exists
- [ ] `dotnet build` clean
