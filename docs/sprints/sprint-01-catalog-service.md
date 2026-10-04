# Sprint 1 — Catalog service

**Status:** ☑ Done
**Started:** 2026-10-03
**Finished:** 2026-10-03
**Guide:** [Chapter 2](../architecture-guide/02-aspnet-core-services-catalog-orders.md) (§2, §5, §10) · **Backlog:** [Sprint 1](../planning/sprint-backlog.md#sprint-1--catalog-service)

**Goal:** Catalog returns seeded products from PostgreSQL.

## 1. Tasks

| # | Task | Owner | Status |
|---|---|---|---|
| 1 | Feature branch `feature/sprint-1-catalog` | 👤 | ☑ Done |
| 2 | Local PostgreSQL in Docker | 👤 | ☑ Done (`shopeasy-pg`, postgres:17, port 5432) |
| 3 | Install `dotnet-ef` CLI | 👤 | ☑ Done (10.0.12) |
| 4 | `Catalog.Api` project, added to the solution | 🤖 | ☑ Done (build succeeded) |
| 5 | `Product`, `CatalogDbContext`, Npgsql, package versions | 🤖 | ☑ Done (schema created as designed) |
| 6 | ServiceDefaults: ProblemDetails, health checks, OpenAPI | 🤖 | ☑ Done (tests + Scalar) |
| 7 | `GET /api/v1/products` (+ `?ids=`) | 🤖 | ☑ Done (verified in Scalar) |
| 8 | `GET /api/v1/products/{id}` → 200 / 404 | 🤖 | ☑ Done (tests) |
| 9 | `ProductDto` contract | 🤖 | ☑ Done |
| 10 | Seed data; generate the migration, update the database | 🤖 / 👤 | ☑ Done (`20261003150009_InitialCreate`, 3 products inserted) |
| 11 | Connection string in user-secrets | 👤 | ☑ Done (port 5433) |
| 12 | Integration tests (Testcontainers); run them | 🤖 / 👤 | ☑ Done (8/8 passed) |
| 13 | PR, green CI, merge | 👤 | ☑ PR #2 merged (`4378aea`); CI green (1 min, 8 tests in Docker on the runner) |

## 2. Work log

### 2026-10-03 — Sprint start

**What we did:** created this log; assigned 👤/🤖 owners to the Sprint 1 tasks in the backlog. The Sprint 0 closing doc updates go into the first Sprint 1 PR.

**Next action:** 👤 tasks 1–3.

### 2026-10-03 — Environment ready (tasks 1–3)

**What we did:** branch `feature/sprint-1-catalog` created. PostgreSQL 17 runs in container `shopeasy-pg` (port 5432, volume `shopeasy-pgdata`, user `shopeasy`). `dotnet-ef` was already installed (10.0.8) and was updated to **10.0.12**.

**What was missing / changed:** `dotnet tool install` on an existing tool *updates* it, which is fine. EF Core and ASP.NET Core package versions were aligned to **10.0.12** to match the CLI (Npgsql provider stays on its own 10.0.0 line).

### 2026-10-03 — Catalog code (tasks 4–9, seed data)

**What we did:** Claude wrote:

| File | Content |
|---|---|
| `src/Catalog/Catalog.Api/Catalog.Api.csproj` | Web project, packages without versions (central management), user-secrets id, ServiceDefaults reference |
| `Program.cs` | ServiceDefaults, `CatalogDb` connection string (fails fast if missing), DbContext, readiness check on the DB, OpenAPI in Development |
| `Products/Product.cs` | Entity |
| `Products/ProductDto.cs` | API contract, separate from the entity |
| `Products/ProductEndpoints.cs` | `GET /api/v1/products` (`?ids=` batch, max 100, sorted by name), `GET /api/v1/products/{id}` → 200/404; `AsNoTracking` |
| `Data/CatalogDbContext.cs` | Table mapping: unique SKU, `numeric(10,2)` price, `char(3)` currency |
| `Data/CatalogSeedData.cs` | 3 products with **fixed GUIDs** (Orders and tests can rely on them) |
| `appsettings*.json`, `launchSettings.json` | Port **5101**; SQL logging in Development only |
| `Catalog.Api.http` | Ready-made requests for VS Code / Visual Studio |
| ServiceDefaults | `AddProblemDetails`, `self` liveness check, `/health/live` (process only) and `/health/ready` (dependencies) |

**Decisions:** no connection string in `appsettings.json` (user-secrets locally, environment variables in containers). Liveness never checks the database.

**Next action:** 👤 user-secrets, build, migration, database update, run, try the `.http` requests.

### 2026-10-03 — Build, migration, database (tasks 4, 5, 10, 11)

**What we did:** `dotnet build` succeeded (no analyzer errors, even with warnings as errors). Created migration `InitialCreate` in `Data/Migrations`. The first `database update` failed with a password error (see Issues: Windows PostgreSQL on 5432). Moved the container to port 5433, updated the user-secret, and re-ran: EF created `catalog_db`, the `Products` table (`uuid`, `varchar(32)`, `varchar(200)`, `numeric(10,2)`, `char(3)`), the unique SKU index, and inserted the 3 seed products. Ended with **Done.**

**Note:** the update still logs one `fail: ... An error occurred using the connection to database 'catalog_db'` line at the start. **This is expected:** EF first tries to connect to `catalog_db`, finds that it doesn't exist yet, and then runs `CREATE DATABASE`. It won't appear on later runs.

**Next action:** 👤 `dotnet run`, try the `.http` requests.

### 2026-10-03 — Browser UI for testing (Scalar)

**What we did:** `http://localhost:5101/` returned a **404 ProblemDetails** response. This is correct: no endpoint exists at `/`, and the response confirms that `UseStatusCodePages` + ProblemDetails work. For testing from a UI, added **Scalar**: the `Scalar.AspNetCore` package, `MapScalarApiReference()` at `/scalar` (Development only), a redirect from `/` to `/scalar`, and the browser opens there on `dotnet run`.

**Next action:** 👤 restart the service, test all endpoints from Scalar, then the readiness test (stop/start the container).

### 2026-10-03 — Scalar verified; integration tests written (task 12)

**What we did:** Scalar opened at `/scalar`. `GET /api/v1/products` returned **200 with the 3 seeded products sorted by name** and correct prices. The first request took 2.8 s: on the first call, EF Core builds its model and the connection pool opens. Later calls are fast.

Claude wrote `tests/Catalog.Api.IntegrationTests` (xUnit v3, `WebApplicationFactory`, Testcontainers PostgreSQL 17):

| Test | Checks |
|---|---|
| `GetProductsReturnsAllSeededProductsSortedByName` | 3 seeded products, order by name |
| `GetProductsWithIdsReturnsOnlyRequestedProducts` | `?ids=` batch filter |
| `GetProductsWithTooManyIdsReturnsBadRequest` | 101 ids → 400 ProblemDetails |
| `GetProductByIdReturnsProduct` | SKU, price, currency |
| `GetUnknownProductReturnsNotFound` | 404 |
| `HealthEndpointsAreHealthyWhenDatabaseIsReachable` | `/health/live` and `/health/ready` → 200 |
| `ReadyFailsButLiveStaysHealthyWhenDatabaseIsUnreachable` | DB down → ready **503**, live **200** (automates the sprint demo) |

The container is shared by all tests (collection fixture) and is migrated once. Test packages were added to central package management. `tests/.gitkeep` was removed because the folder now has content.

**Next action:** 👤 manual readiness test; `dotnet test` (Docker must be running).

### 2026-10-03 — Test runner fix

**What we did:** `dotnet test ShopEasy.slnx` failed: *"Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK"*.

**What was missing:** xUnit v3 4.x runs on the new **Microsoft.Testing.Platform (MTP)**. With the .NET 10 SDK, `dotnet test` has to be switched to MTP mode explicitly.

**Fix:** `global.json` → `"test": { "runner": "Microsoft.Testing.Platform" }`; removed the VSTest packages (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`); new command `dotnet test --solution ShopEasy.slnx`; CI uses `--report-xunit-trx`; README updated.

### 2026-10-03 — Tests green (task 12, sprint demo)

**What we did:** after renaming the collection fixture (CA1711), `dotnet test --solution ShopEasy.slnx` → **8/8 passed** in about 1 minute (most of that time is starting the PostgreSQL container).

**Next action:** 👤 format check, commit, PR, green CI, merge (task 13).

### 2026-10-03 — PR #2 (task 13)

**What we did:** `dotnet format` flagged the constant naming (fixed in `.editorconfig`). Committed and pushed `feature/sprint-1-catalog` and opened **PR #2** (30 files, +916/−28). `CI / build-and-test` passed in 1 min: restore, format, build, and the 8 integration tests with Testcontainers on the GitHub runner.

**Next action:** 👤 merge, delete branch, `git checkout main` + `git pull`. This sprint-close log update goes into the Sprint 2 PR.

**Result:** merged into `main` as `4378aea`. The remote branch was again not deleted (same as PR #1). **Fix:** enable *Settings → General → Pull Requests → "Automatically delete head branches"* so GitHub deletes branches on merge.

## 3. Issues and fixes

| Date | Problem | Cause | Fix |
|---|---|---|---|
| 2026-10-03 | `dotnet format --verify-no-changes` → IDE1006 "Missing prefix `_`" on `private const int MaxIdsPerRequest` | Our `.editorconfig` `_camelCase` rule for private fields also matched **constants** (Sprint 0 rule was too broad). `dotnet build` doesn't run every IDE rule; `dotnet format` does | Added a `const` → PascalCase rule before the private field rule. Lesson: run `dotnet format --verify-no-changes` before every commit (CI does) |
| 2026-10-03 | Build error CA1711: type `CatalogApiCollection` must not end in "Collection" | Analyzer rule: the suffix "Collection" is reserved for types that implement collections; warnings are errors | Renamed to `CatalogApiTestGroup` |
| 2026-10-03 | `dotnet test` build error: VSTest target not supported by Microsoft.Testing.Platform on .NET 10 SDK | xUnit v3 4.x uses MTP; .NET 10 `dotnet test` defaults to VSTest mode | Opt in via `global.json` `test.runner`; drop VSTest packages; `dotnet test --solution …` |
| 2026-10-03 | `dotnet ef database update` → `28P01: password authentication failed for user "shopeasy"` | **Confirmed:** Windows service `postgresql-x64-18` (PID 6940) and Docker (PID 22520) both listen on 5432. `localhost:5432` reached the Windows PostgreSQL, which has no `shopeasy` user. The container itself worked (`psql` in the container returned PostgreSQL 17.11) | Keep the Windows service (may be used elsewhere); map the container to host port **5433** (`-p 5433:5432`); connection string `Port=5433`. **Rule for all later sprints:** local ShopEasy PostgreSQL = port 5433 |

## 4. Decisions made in this sprint

| Decision | Why |
|---|---|
| Catalog is a single project | Simple CRUD; layers would be ceremony (Chapter 2, §4) |
| Fixed GUIDs for seed products | Stable ids for Orders, tests, and `.http` files |
| Liveness = process only; readiness = database | A DB outage must not restart pods (Chapter 6, §5) |
| No connection string in committed config | Secrets never in Git; 12-factor config |
| EF/ASP.NET packages pinned to 10.0.12 | Match the `dotnet-ef` CLI version |
| Tests run on **Microsoft.Testing.Platform** (not VSTest) | New .NET 10 test experience; required by xUnit v3 4.x; faster, no adapter packages |
| Local PostgreSQL container on host port **5433** | Port 5432 is taken by a Windows PostgreSQL 18 service |
| ~~OpenAPI JSON only~~ → **Scalar UI** (`Scalar.AspNetCore` 2.17.13) at `/scalar`, Development only; `/` redirects there | Testing from a browser UI is easier while learning. .NET 10 no longer ships Swagger UI, and Scalar is the common replacement. Not exposed in Production |

## 5. What I learned

- _(your own notes; suggestions below)_
- Port conflicts are easy to miss: `netstat -ano | findstr :5432` + `Get-Service *postgres*` found a Windows PostgreSQL hiding behind `localhost`.
- EF Core migrations: `migrations add` creates code, `database update` creates the DB, tables, and seed data (`HasData`).
- Liveness vs readiness in practice: DB down → ready 503, live 200.
- .NET 10 testing: xUnit v3 runs on Microsoft.Testing.Platform, opted in through `global.json`.
- Testcontainers gives real PostgreSQL in tests, locally and in CI, with no setup.
- Warnings as errors + analyzers (CA1711, IDE1006) catch naming problems early; `dotnet format --verify-no-changes` before each commit.

## 6. Sprint demo

- [x] `GET /api/v1/products` returns the 3 seeded products (Scalar + test)
- [x] `/health/ready` fails when PostgreSQL is stopped and recovers when started (automated: `ReadyFailsButLiveStaysHealthyWhenDatabaseIsUnreachable`; manual stop/start check optional)
