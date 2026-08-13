# Thinkbridge Backend Assignment — Shruti Sahrawat

Days 1–3. All code in this repository. **139 tests passing** across three test projects, green on GitHub Actions.

| Suite | Tests | Runtime |
|---|---|---|
| `OrderRefactor.Tests` | 21 | ~2s |
| `Quotes.Tests.Unit` | 95 | ~0.3s |
| `Quotes.Tests.Integration` | 23 | ~14s (real SQL Server 2022 container) |

Two applications: **QuotesApi** (minimal API, DDD aggregate) and **OrderRefactor** (layered refactor, JWT auth, Entra ID).

---

## Day 1 — Foundations

**Piece 1 — Hello in two languages**
[`hello-cs/Program.cs`](hello-cs/Program.cs) · [`hello-ts/hello.ts`](hello-ts/hello.ts)
C# needs a `.csproj` and an SDK before it will run anything; Node 24 executes TypeScript directly with no build step and no config file. The contrast is the point — one runtime asks you to declare structure up front, the other asks for nothing.

**Piece 2 — Minimal ASP.NET Core API**
[`QuotesApi/Extensions/EndpointExtensions.cs`](QuotesApi/Extensions/EndpointExtensions.cs) · [`QuotesApi/Repositories/QuoteRepository.cs`](QuotesApi/Repositories/QuoteRepository.cs)
Four endpoints on `/api/quotes` — paged list, create, get by id, delete. EF Core + SQLite with migrations applied at startup, scoped `IQuoteRepository` via DI, `ValidationProblemDetails` on invalid input, `CancellationToken` flowing into every EF query, structured logging via `ILogger<T>`, and `ProblemDetails` from [exception middleware](QuotesApi/Middleware/ExceptionHandlingMiddleware.cs). `Program.cs` stays under 120 lines by splitting into `AddInfrastructure()` and `MapQuoteEndpoints()` extension methods.

**Piece 3 — Refactor a god-method controller**
Before: [`OrderRefactor/Original/OrderController.cs`](OrderRefactor/Original/OrderController.cs) — the ~250-line original, saved unmodified.
Prompt that generated it: [`OrderRefactor/Original/PROMPT.md`](OrderRefactor/Original/PROMPT.md)
Analysis: [`OrderRefactor/REFACTOR_NOTES.md`](OrderRefactor/REFACTOR_NOTES.md) — 10+ distinct smells, each with its consequence and intended fix, written before touching a line of code.
After: [`Controllers/OrderController.cs`](OrderRefactor/Controllers/OrderController.cs) → [`Services/OrderService.cs`](OrderRefactor/Services/OrderService.cs) → [`Repositories/`](OrderRefactor/Repositories) — split into layers wired by DI, async end-to-end with cancellation, typed return shapes, and empty catches replaced with narrow handlers that log and rethrow.

**Piece 4 — Real AI-assisted work**
[`AI_REFLECTION.md`](AI_REFLECTION.md) — where Claude Code helped, where it over-engineered and I pushed back, and where Copilot suggested something subtly wrong.

**Piece 5 — Build a real aggregate**
[`QuotesApi/Domain/Collection.cs`](QuotesApi/Domain/Collection.cs) · [`QuotesApi/Domain/CollectionItem.cs`](QuotesApi/Domain/CollectionItem.cs)
An aggregate root that enforces its own invariants: name 3–80 characters, maximum 50 items, no duplicate quote IDs, positive quote ID required. `CollectionItem` is an immutable value object mapped as an EF owned type. Every mutation goes through `AddItem`/`RemoveItem`, which throw rather than letting callers touch the collection directly — so the aggregate is consistent after every operation, not merely by convention. Endpoints: [`CollectionsController.cs`](QuotesApi/Controllers/CollectionsController.cs).

---

## Day 2 — Architecture and Authentication

**Piece 1 — Dependency injection at depth** *(partial — see note)*
[`QuotesApi/Services/IClock.cs`](QuotesApi/Services/IClock.cs) · [`SystemClock.cs`](QuotesApi/Services/SystemClock.cs), registered as a singleton in [`Program.cs`](QuotesApi/Program.cs). Repositories and `DbContext` scoped; `DiscountCalculator` transient.
**Known gap:** `IClock` is registered and covered by fake-clock tests against the `Quote.Create(author, text, clock)` overload, but `EndpointExtensions` still calls the two-argument overload, so the clock is never consulted on the live request path. `CollectionItem` and `AuthController` also still call `DateTime.UtcNow` directly. The abstraction exists and is testable; it is not yet threaded through production.

**Piece 2 — async/await with cancellation through layers**
`CancellationToken` is the last parameter on every I/O method and flows controller → service → repository → EF. See [`IOrderRepository.cs`](OrderRefactor/Repositories/IOrderRepository.cs) and [`QuoteRepository.cs`](QuotesApi/Repositories/QuoteRepository.cs). Cancellation is tested, not assumed: [`CollectionsControllerCancellationTests.cs`](OrderRefactor.Tests/CollectionsControllerCancellationTests.cs).

**Piece 3 — Test the domain layer**
[`OrderRefactor.Tests/CollectionDomainTests.cs`](OrderRefactor.Tests/CollectionDomainTests.cs), extended considerably in [`Quotes.Tests.Unit/CollectionTests.cs`](Quotes.Tests.Unit/CollectionTests.cs) — every `Collection` invariant including the 49th/50th/51st item boundary. Pure and fast: no DbContext, no fixtures, no setup methods.

**Piece 4 — AI-assisted refactor: anemic to rich**
[`QuotesApi/Models/Quote.cs`](QuotesApi/Models/Quote.cs) — private setters, private constructor, a static `Quote.Create` factory validating author (1–200 chars) and text (1–1000 chars) with trimming, and `SoftDelete()` instead of a publicly mutable flag. Rationale and the bug the anemic version would have shipped: [`WHY.md`](WHY.md). Tests: [`Quotes.Tests.Unit/QuoteTests.cs`](Quotes.Tests.Unit/QuoteTests.cs).

**Piece 5 — JWT auth with my own issuer**
[`OrderRefactor/Controllers/AuthController.cs`](OrderRefactor/Controllers/AuthController.cs) — `POST /api/auth/login` returns `access_token`, `refresh_token`, and `expires_in`. HS256, signed with a 256-bit key read from `IConfiguration`, never hardcoded.

**Piece 6 — Refresh tokens with rotation and reuse detection**
Same file. Refresh tokens are stored hashed, never in plaintext ([`Models/RefreshToken.cs`](OrderRefactor/Models/RefreshToken.cs): `TokenHash`, `UserId`, `ExpiresAt`, `RevokedAt`, `ReplacedByToken`). Every refresh rotates the pair and marks the old token replaced. **Presenting an already-rotated token revokes the entire family for that user** and forces re-authentication — so a leaked token cannot be used twice, and the theft is detected rather than silently exploited.
Proven end-to-end in [`RefreshTokenTests.cs`](OrderRefactor.Tests/RefreshTokenTests.cs): log in, refresh once, replay the spent token → 401, then confirm the legitimate user's current token is dead too.

---

## Day 3 — Enterprise Auth and Testing

**Wire Entra ID as the identity provider** *(config complete, live token untested — see note)*
[`OrderRefactor/Program.cs`](OrderRefactor/Program.cs) registers two bearer schemes behind a policy scheme:
Request with Bearer token
↓
PolicyScheme reads the issuer claim (reads only — no validation yet)
↓
iss == "OrderRefactorIssuer"?
├─ yes → InternalJwt symmetric key, my own tokens
└─ no → EntraJwt Microsoft's public keys, fetched from Authority
↓
[Authorize] resolves as normal — controllers unchanged
Entra configuration (`TenantId`, `ClientId`, `Audience`) lives in `appsettings.json`; these are public identifiers, not secrets. Authority is `https://login.microsoftonline.com/{tenant}/v2.0`. No client secret is needed anywhere — an API that only validates tokens uses Microsoft's published signing keys.
**Known gap:** the application is registered in Entra and the code path is in place, but I could not obtain a real Entra access token to verify end-to-end. The institutional tenant rejected the `access_as_user` scope grant (`AADSTS65005`). The internal JWT path is verified working; the Entra branch is unverified against a live token.

**Authorization policies and claims**
`AdminOnly` (claim-based) and `CanEditOwnOrders` (custom assertion) defined in [`Program.cs`](OrderRefactor/Program.cs), applied at [`OrderController.CreateOrder`](OrderRefactor/Controllers/OrderController.cs). Authentication answers *who you are*; policies answer *what you may do*. Roles are claims that change; policies encode rules that don't.

**Lock down the API end-to-end**
[`OrderControllerTests.cs`](OrderRefactor.Tests/OrderControllerTests.cs) + [`RefreshTokenTests.cs`](OrderRefactor.Tests/RefreshTokenTests.cs) — 21 tests: anonymous → 401, authenticated but wrong policy → 403, correct policy → 201, expired token → 401, malformed token → 401, revoked refresh chain → 401.

**The testing pyramid in real terms**
Reflected in the actual shape of the suites: 95 unit tests at ~3ms each, 44 integration tests at ~200ms–600ms, no end-to-end layer. The lesson that stuck is that the pyramid is about *time*, not test count — 23 integration tests consume more wall-clock than 95 unit tests, so the ratio that matters is the one on the stopwatch.

**xUnit with FluentAssertions**
[`Quotes.Tests.Unit/`](Quotes.Tests.Unit) — 95 tests in ~0.3s. One test class per production class, `Method_StateUnderTest_ExpectedBehavior` naming, explicit AAA in every test, no `SetUp` hiding arrangement, `[Theory]`/`[InlineData]` for boundaries. NSubstitute for `IOrderRepository`, `IConfiguration`, and `ILogger<T>`.

**Integration tests with WebApplicationFactory**
[`Quotes.Tests.Integration/`](Quotes.Tests.Integration) — 23 tests booting the real application: real middleware pipeline, real DI graph, real EF. A fresh database and `HttpClient` per test, no shared state between tests. `ProblemDetails` and `ValidationProblemDetails` response shapes are asserted, not just status codes.

**Real SQL Server in CI with Testcontainers**
[`MsSqlContainerFixture.cs`](Quotes.Tests.Integration/MsSqlContainerFixture.cs) — one SQL Server 2022 container per assembly run via `IAsyncLifetime` + `ICollectionFixture`, with each test getting its own database on that shared container. The suite goes 2s → 14s: the honest cost of testing against a real engine.
The SQLite migrations could not be replayed against SQL Server. They bake in literal `TEXT` column types and a `Sqlite:Autoincrement` annotation that SQL Server silently ignores, producing a table with no `IDENTITY` — so every insert fails. The fix is not translation but a separate SQL-Server-native migration set inside the test project ([`Migrations/SqlServer/`](Quotes.Tests.Integration/Migrations/SqlServer)), wired via `MigrationsAssembly`. Zero production changes; the SQLite app still runs unmodified.

---

## Concept cards

Three cards were conceptual rather than build tasks. Where each one landed in the code:

**Day 1 — Tools check.** .NET SDK 10.0.302, Node 24 (runs `hello.ts` natively, no `tsc` step), Git, VS Code with C# Dev Kit, Copilot, Claude Code — the last used for the Day 1 refactor, the Day 2 rich-model rewrite, and three Day 3 test projects.

**Day 2 — Entity, value object, aggregate root.** Demonstrated in [`QuotesApi/Domain/`](QuotesApi/Domain): `Collection` is the aggregate root and the consistency boundary; `CollectionItem` is an immutable value object mapped as an EF owned type; `ICollectionRepository` is one repository per root rather than per entity; and all mutation goes through the root, which throws on invariant violation instead of letting callers reach inside.

**Day 2 — JWT, OAuth2, OIDC.** Applied in [`AuthController.cs`](OrderRefactor/Controllers/AuthController.cs) — self-issued JWTs, 15-minute access tokens, 7-day single-use rotating refresh tokens, which is exactly the shape the card prescribes for an API like this — and in [`Program.cs`](OrderRefactor/Program.cs), where a policy scheme routes between my own issuer and an OIDC provider (Entra ID) on the issuer claim.

---

## Two bugs these tests caught

**A startup bug that would have broken any clean deployment.** All 23 integration tests failed on their first run — inside `Program.cs`, not in test code. `Quote.IsDeleted` existed on the model but had never been captured in a migration, so `Database.Migrate()` threw `PendingModelChangesWarning` against any fresh database. My local `quotes.db` predated the drift, so it had never surfaced in development. A clean clone would not have booted. Fixed in [`20260812113000_AddQuoteIsDeleted.cs`](QuotesApi/Migrations/20260812113000_AddQuoteIsDeleted.cs).

**A regression I introduced myself, caught within the hour.** Adding `[Authorize(Policy = "AdminOnly")]` to `CreateOrder` immediately broke an existing Day 2 test that posted without a token — it started returning 401 before reaching the logic under test. The suite caught it the same hour I wrote it. I fixed the test, not the policy.

---

## Running it

```bash
dotnet test OrderRefactor.Tests        # 21
dotnet test Quotes.Tests.Unit          # 95
dotnet test Quotes.Tests.Integration   # 23 — requires Docker
```

[`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs all three projects as separate jobs on GitHub Actions. **All three pass**, including the integration job, which starts a real SQL Server 2022 container on the runner via Testcontainers. [Latest run](../../actions).
