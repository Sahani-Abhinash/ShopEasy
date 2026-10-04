# Sprint 2 — Orders service

**Status:** ☑ Done
**Started:** 2026-10-03
**Finished:** 2026-10-04
**Guide:** [Chapter 2](../architecture-guide/02-aspnet-core-services-catalog-orders.md) (§3, §6–§9, §12) · **Backlog:** [Sprint 2](../planning/sprint-backlog.md#sprint-2--orders-service)

**Goal:** Orders accepts an order with authoritative prices and safe retries.

## 1. Tasks

| # | Task | Owner | Status |
|---|---|---|---|
| 1 | Switch to `main`, branch `feature/sprint-2-orders`; auto-delete merged branches | 👤 | ◐ Branch created from `4378aea`; auto-delete setting still to do |
| 2 | Four projects with correct references | 🤖 | ☑ Done (tests pass) |
| 3 | `Order`, `OrderItem`, `OrderStatus`, `DomainException` | 🤖 | ☑ Done (tests pass) |
| 4 | Domain rules | 🤖 | ☑ Done (tests pass) |
| 5 | Domain unit tests | 🤖 | ☑ Done (tests pass) |
| 6 | `ICatalogClient` + `CatalogHttpClient` with resilience | 🤖 | ☑ Done (tests pass) |
| 7 | `PlaceOrderHandler` | 🤖 | ☑ Done (tests pass) |
| 8 | EF mapping (owned items, unique (CustomerId, IdempotencyKey), xmin) | 🤖 | ☑ Done (tests pass) |
| 9 | `POST /api/v1/orders` → 202 + Location | 🤖 | ☑ Done (tests pass) |
| 10 | `GET /api/v1/orders/{id}` (own orders only) | 🤖 | ☑ Done (tests pass) |
| 11 | 422 unknown product, 503 Catalog down | 🤖 | ☑ Done (tests pass) |
| 12 | User-secrets, migration, database update | 👤 | ☑ Done (`20261004102012_InitialCreate`, `orders_db` on port 5433) |
| 13 | Integration tests; run them | 🤖 / 👤 | ☑ Done (domain 10/10, integration 9/9 passed) |
| 14 | Manual test with Catalog + Orders running | 👤 | ☑ Done (202, same id on retry, Catalog prices, 404 for other customer, 503 with Catalog down) |
| 15 | PR, green CI, merge | 👤 | ☑ PR #3 merged (`0f1d605`); CI green |

## 2. Work log

### 2026-10-03 — Sprint start and code (tasks 2–11, 13)

**What we did:** set up the Sprint 2 log and the task owners. Claude wrote the Orders service in four layers:

| Layer | Files | Responsibility |
|---|---|---|
| **Domain** | `Order`, `OrderItem`, `OrderStatus`, `DomainException` | Rules: ≥1 item, quantity 1–100, no duplicate products, non-negative price, 3-letter currency, status `Pending`; `Total` computed; UUID v7 ids |
| **Application** | `PlaceOrderHandler`, `PlaceOrderCommand/Line/Result`, `IOrderRepository`, `ICatalogClient` | Use case: idempotency lookup → prices from Catalog → domain creates order → save; race on the same key returns the winner’s order |
| **Infrastructure** | `OrdersDbContext`, `OrderRepository`, `CatalogHttpClient`, `DependencyInjection` | EF Core (owned `OrderItems` table, `IdempotencyKey` shadow property, unique index (`CustomerId`, `IdempotencyKey`), xmin concurrency); typed `HttpClient` + `AddStandardResilienceHandler`; Catalog failures → `CatalogUnavailableException` |
| **API** | `OrderEndpoints`, `OrderContracts`, `DevelopmentCurrentCustomer`, `Program.cs` | `POST` → 202/400/422/503, `GET` → 200/404; Scalar; readiness check on the DB; port 5102 |

**Tests written:**

| Project | Tests |
|---|---|
| `Orders.Domain.Tests` (unit, no Docker) | Valid order + total, no items, invalid quantity (0, −1, 101), negative price, duplicate product, empty customer, invalid currency |
| `Orders.Api.IntegrationTests` (PostgreSQL in Docker, fake Catalog) | 202 + Location + Catalog prices (client price ignored), same key twice → one row, **10 parallel requests same key → one order**, same key different customers → two orders, missing key → 400, unknown product → 422, quantity 0 → 400, other customer’s order → 404, Catalog down → 503 |

**Next action:** 👤 tasks 1 and 12, then build and run the tests.

### 2026-10-04 — Branch, database, tests (tasks 1, 12, 13)

**What we did:** created `feature/sprint-2-orders` from `main` (`4378aea`, after PR #2) and committed the scaffold (`b5372c1`). The solution builds with 0 warnings. Set the `OrdersDb` user-secret, created migration `InitialCreate`, and ran `database update` → **Done.** `Orders.Domain.Tests` 10/10 and `Orders.Api.IntegrationTests` 9/9 (Testcontainers, 1 min 18 s) passed.

**Next action:** 👤 GitHub auto-delete setting (task 1), manual test with Catalog + Orders (task 14), then PR (task 15).

### 2026-10-04 — Manual test (task 14)

**What we did:** ran Catalog (5101) and Orders (5102) in two terminals and called Orders with `Idempotency-Key: checkout-001`, 2 × Keyboard (sent `"price": 0.01`) + 1 × Monitor:

| Check | Result |
|---|---|
| `POST` | `202 Accepted`, `Location: /api/v1/orders/01a10694-8a14-77c0-a8f9-59a41ce5f024`, status `Pending` |
| Same `POST` again (same key) | Same `orderId` |
| `GET` the order | Keyboard `unitPrice` **89.99** (client's 0.01 ignored), Monitor 349.00, total **528.98 EUR** |
| `GET` as another customer (`X-Customer-Id`) | `404` |
| `POST` while Catalog was stopped | `503` "Prices can't be checked right now. Please retry with the same Idempotency-Key." |

**Next action:** 👤 GitHub auto-delete setting (task 1), `dotnet format --verify-no-changes`, then PR (task 15).

### 2026-10-04 — PR #3 (task 15)

**What we did:** `dotnet format --verify-no-changes` passed after the naming fix. Pushed `feature/sprint-2-orders` and opened **PR #3** (3 commits, 40 files, +1,662/−21). CI passed. Merged into `main` as `0f1d605`; local branch deleted, `feature/sprint-3-docker` created from `main`. This sprint-close log update goes into the Sprint 3 PR.

**Open:** the remote branch was again not deleted automatically → enable *Settings → General → Pull Requests → "Automatically delete head branches"* (task 1).

## 3. Issues and fixes

| Date | Problem | Cause | Fix |
|---|---|---|---|
| 2026-10-04 | `git checkout main` refused: local changes would be overwritten | Sprint-close doc edits were uncommitted on `feature/sprint-1-catalog`, and `main` had different versions of the same files | Created the Sprint 2 branch directly from the current state (`git checkout -b`), which already included the merged `main` |
| 2026-10-04 | `database update` → `Failed to connect to 127.0.0.1:5433` | Container `shopeasy-pg` had stopped (exit 255 after a Docker/PC restart) | `docker start shopeasy-pg` |
| 2026-10-04 | Scalar on 5102 → `ERR_CONNECTION_REFUSED`; later `POST` → 503 | Only one service was running at a time (the second `dotnet run` was started in the same terminal) | Run Catalog and Orders in **two separate terminals** |
| 2026-10-04 | `GET /api/v1/orders/{id}` → 404 during the manual test | A **product** id was used instead of the `orderId` returned by `POST` | Place the order first; GET with its `orderId` |
| 2026-10-04 | `dotnet format --verify-no-changes` → 4 × IDE1006 "Missing prefix: '_'" | `private static readonly` fields (`Products`, `CustomerId`, `Now`, `FallbackCustomerId`) matched the private `_camelCase` rule | `.editorconfig`: rule for `static readonly` fields = PascalCase (like constants), listed before the private field rule |
| 2026-10-04 | `database update` → `28P01: password authentication failed for user "..."` | The user-secret still had placeholder values `Username=...;Password=...` | Set the real secret with the same user/password as Catalog and port **5433** |

## 4. Decisions made in this sprint

| Decision | Why |
|---|---|
| Idempotency key unique **per customer** (`CustomerId`, `IdempotencyKey`) | Two customers may generate the same key; one customer must not see another’s order through a key |
| Idempotency key as an EF **shadow property** | It is a persistence/API concern, not a domain concept |
| Race handling: unique violation → return the existing order | The DB is the final guard; the first lookup alone can’t prevent concurrent duplicates |
| Same key with a *different* body returns the first order (not 422) | Simple for now. **Known limitation:** storing a request hash and rejecting mismatches is a later improvement |
| Request contract has **no price field**; unknown JSON fields are ignored | Prices only from Catalog (Chapter 2, §9) |
| `GET` other customer’s order → **404**, not 403 | Don’t reveal that the order exists |
| Temporary `X-Customer-Id` header / config customer | Lets us test ownership before authentication (Sprint 12). **Must be removed before a real deployment** |
| Catalog down → **503** with “retry with the same key” | Safe retry thanks to idempotency |
| Orders keeps **its own copy** of Catalog’s response contract | Services never share model classes |
| Integration tests use a **fake `ICatalogClient`** | Test Orders alone; Orders↔Catalog contract tests come in Sprint 11 |
| Order ids are **UUID v7** (`Guid.CreateVersion7`) | Time-ordered: better index locality than random GUIDs |

## 5. What I learned

-

## 6. Sprint demo

- [x] Order is saved with Catalog’s price even if the client sends a different price
- [x] Retrying with the same key returns the same order ID
