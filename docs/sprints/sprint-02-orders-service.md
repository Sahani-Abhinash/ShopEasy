# Sprint 2 — Orders service

**Status:** ◐ In progress
**Started:** 2026-10-03
**Finished:** —
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
| 14 | Manual test with Catalog + Orders running | 👤 | ☐ |
| 15 | PR, green CI, merge | 👤 | ☐ |

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

## 3. Issues and fixes

| Date | Problem | Cause | Fix |
|---|---|---|---|
| 2026-10-04 | `git checkout main` refused: local changes would be overwritten | Sprint-close doc edits were uncommitted on `feature/sprint-1-catalog`, and `main` had different versions of the same files | Created the Sprint 2 branch directly from the current state (`git checkout -b`), which already included the merged `main` |
| 2026-10-04 | `database update` → `Failed to connect to 127.0.0.1:5433` | Container `shopeasy-pg` had stopped (exit 255 after a Docker/PC restart) | `docker start shopeasy-pg` |
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

- [ ] Order is saved with Catalog’s price even if the client sends a different price
- [ ] Retrying with the same key returns the same order ID
