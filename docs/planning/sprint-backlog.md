# ShopEasy — Sprint backlog

How to use this file:

- Tick a task (`- [x]`) only when it works **and** is verified (test or manual check written in the task).
- A sprint is done when every task **and** its **Sprint demo** checks are ticked.
- Don't start the next sprint before the previous demo works.
- **Who does what:**
  - 👤 **You:** all configuration and interaction with external tools and services. That means installing tools, creating the GitHub repository and its settings (branch protection, secrets, environments), Azure portal/CLI actions, `terraform apply`/`destroy`, Docker/kind/cluster commands, and account setup (Entra, Argo CD, and so on). Claude gives step-by-step instructions and explains each step; you run them.
  - 🤖 **Claude:** writes and edits files in the repository (code, Dockerfiles, YAML, Terraform, workflows, docs) and reviews your results.
  - Each task is marked 👤 or 🤖. If a task needs both, Claude prepares the files first and you then run or configure.
- **Sprint logs:** every sprint has its own file in [`docs/sprints/`](../sprints/), for example `sprint-00-repository-and-tooling.md`, created from [`_sprint-log-template.md`](../sprints/_sprint-log-template.md) when the sprint starts. It records what we did, what was missing, problems and fixes, decisions, and lessons learned. It is updated after every working session. This backlog stays the summary checklist.
- Sprints follow the roadmap in [Appendix, Part D](../architecture-guide/11-appendix-glossary-interview-qa-roadmap.md). 💰 = creates Azure cost.

## Progress overview

| Sprint | Goal | Guide | Status |
|---|---|---|---|
| 0 | Repository and tooling | Ch. 1 | ☑ Done — [log](../sprints/sprint-00-repository-and-tooling.md) |
| 1 | Catalog service | Ch. 2 | ☑ Done — [log](../sprints/sprint-01-catalog-service.md) |
| 2 | Orders service | Ch. 2 | ☑ Done — [log](../sprints/sprint-02-orders-service.md) |
| 3 | Docker and Compose | Ch. 3 | ◐ In progress — [log](../sprints/sprint-03-docker-and-compose.md) |
| 4 | Kafka, outbox, inbox | Ch. 4 | ☐ Not started |
| 5 | Inventory and reservation | Ch. 5 | ☐ Not started |
| 6 | Payments, Notifications, full saga | Ch. 5 | ☐ Not started |
| 7 | Observability | Ch. 8 | ☐ Not started |
| 8 | BFF and frontend | Ch. 10 | ☐ Not started |
| 9 | Kubernetes on kind | Ch. 6 | ☐ Not started |
| 10 | Azure with Terraform 💰 | Ch. 7 | ☐ Not started |
| 11 | CI/CD and GitOps 💰 | Ch. 9 | ☐ Not started |
| 12 | Production hardening 💰 | Ch. 10 | ☐ Not started |

Status values: ☐ Not started · ◐ In progress · ☑ Done

---

## Sprint 0 — Repository and tooling

**Goal:** an empty but well-structured repository with CI running on every PR.

- [x] 👤 Install tools: .NET 10 SDK, Node.js LTS, Docker Desktop (WSL 2), Git, VS Code/Visual Studio, Azure CLI, Terraform, kubectl, kind, Helm (verify with the version commands)
- [x] 👤 `git init` locally
- [x] 🤖 `.gitignore` (dotnet + terraform + node), `.gitattributes`
- [x] 🤖 Folder structure: `src/`, `tests/`, `deploy/`, `infrastructure/`, `docs/`
- [x] 🤖 `ShopEasy.slnx`, `Directory.Build.props` (net10.0, nullable, warnings as errors), `Directory.Packages.props`
- [x] 🤖 `.editorconfig`; 👤 run `dotnet format --verify-no-changes`
- [x] 🤖 `src/BuildingBlocks/ShopEasy.ServiceDefaults` project (empty `AddServiceDefaults()` for now)
- [x] 🤖 Root `README.md`: purpose, prerequisites, how to run
- [x] 🤖 `docs/adr/` with ADR template and ADRs 0001–0003 from Chapter 1
- [x] 🤖 GitHub Actions `.github/workflows/ci.yml`: restore, build, test on PR
- [x] 👤 First commit; create the GitHub repository; add remote and push `main`
- [ ] 👤 Branch protection on `main`: PR required, CI must pass. **Deferred:** repo is private (not enforced on GitHub Free); follow the PR process by habit and enforce when the repo becomes public

**Sprint demo**
- [x] A PR with a trivial change shows a green CI check and can be merged

---

## Sprint 1 — Catalog service

**Goal:** Catalog returns seeded products from PostgreSQL.

- [x] 👤 Feature branch `feature/sprint-1-catalog`
- [x] 👤 Local PostgreSQL in Docker (temporary; Compose comes in Sprint 3)
- [x] 👤 Install the EF Core CLI (`dotnet tool install --global dotnet-ef`)
- [x] 🤖 Create `Catalog.Api` (Minimal APIs) and add it to the solution
- [x] 🤖 `Product` entity, `CatalogDbContext`, Npgsql provider, central package versions
- [x] 🤖 ServiceDefaults: ProblemDetails, health checks `/health/live` and `/health/ready`, OpenAPI in Development
- [x] 🤖 `GET /api/v1/products` (with optional `?ids=` batch filter)
- [x] 🤖 `GET /api/v1/products/{id}` → 200 / 404
- [x] 🤖 Return `ProductDto`, not the entity
- [x] 🤖 Seed data (keyboard, mouse, monitor); 👤 generate the initial migration and update the database
- [x] 👤 Connection string via user-secrets (nothing in Git)
- [x] 🤖 Integration tests with Testcontainers PostgreSQL; 👤 run them
- [x] 👤 PR, green CI, merge

**Sprint demo**
- [x] `GET /api/v1/products` returns the 3 seeded products
- [x] `/health/ready` fails when PostgreSQL is stopped and recovers when started

---

## Sprint 2 — Orders service

**Goal:** Orders accepts an order with authoritative prices and safe retries.

- [ ] 👤 Switch to `main`, pull, create branch `feature/sprint-2-orders`; enable auto-delete of merged branches on GitHub
- [x] 🤖 Projects: `Orders.Domain`, `Orders.Application`, `Orders.Infrastructure`, `Orders.Api` with correct references
- [x] 🤖 `Order` aggregate, `OrderItem`, `OrderStatus`, `DomainException`
- [x] 🤖 Domain rules: ≥1 item, positive quantity, no duplicate products, one currency
- [x] 🤖 Unit tests for domain rules (`Orders.Domain.Tests`)
- [x] 🤖 `ICatalogClient` port + `CatalogHttpClient` (typed client + `AddStandardResilienceHandler`)
- [x] 🤖 `PlaceOrderHandler` (idempotency check, prices from Catalog, unknown products)
- [x] 🤖 EF Core mapping: owned items, unique index on (`CustomerId`, `IdempotencyKey`), row version
- [x] 🤖 `POST /api/v1/orders` → `202 Accepted` + `Location`; `Idempotency-Key` required
- [x] 🤖 `GET /api/v1/orders/{id}` (only own orders; fixed dev customer for now)
- [x] 🤖 Unknown product → `422`; Catalog unavailable → `503` ProblemDetails
- [x] 👤 User-secrets (`OrdersDb`), migration, database update
- [x] 🤖 Integration tests: same key twice → one row; parallel identical requests → one order; fake Catalog; 👤 run them
- [x] 👤 Manual test with Catalog + Orders running (Scalar)
- [x] 👤 PR, green CI, merge

**Sprint demo**
- [x] Order is saved with Catalog’s price even if the client sends a different price
- [x] Retrying with the same key returns the same order ID

---

## Sprint 3 — Docker and Docker Compose

**Goal:** the whole system starts with one command.

- [x] `.dockerignore` at repo root
- [x] Multi-stage Dockerfile for Catalog and Orders (chiseled, non-root, port 8080)
- [x] `compose.yaml` with PostgreSQL (volume, healthcheck), Catalog, Orders (PostgreSQL published on host port **5433**: 5432 is used by a Windows PostgreSQL service)
- [x] `deploy/local/init-databases.sh`: one database + user per service (`.sh` so passwords come from `.env`)
- [x] `.env.example` committed, `.env` ignored
- [x] EF migration bundles + migrator services in Compose
- [x] Orders reaches Catalog via `http://catalog-api:8080`

**Sprint demo**
- [x] `docker compose up -d --build` → place an order → `202`
- [x] Images ~110–130 MB and run as non-root
- [x] Data survives `down` / `up`
- [x] Changing one `.cs` file doesn’t re-run `dotnet restore`

---

## Sprint 4 — Kafka, outbox, inbox

**Goal:** Orders publishes messages reliably; consumers process each message once.

- [ ] Kafka (KRaft) + Kafka UI in Compose; `kafka-init` creates topics with partitions
- [ ] Message envelope (messageId, type, version, correlationId, causationId, data)
- [ ] Contracts: `OrderPlaced`, `ReserveStock`, and the remaining saga messages
- [ ] `OutboxMessages` table + `IOutbox` port, saved in the same transaction as the order
- [ ] Outbox publisher `BackgroundService` (`FOR UPDATE SKIP LOCKED`, idempotent producer, `acks=all`)
- [ ] Outbox cleanup of old sent rows
- [ ] Consumer base: manual commit after processing, graceful shutdown
- [ ] `InboxMessages` table and duplicate skipping
- [ ] Dead-letter topic handling for poison messages
- [ ] Contract snapshot tests for messages

**Sprint demo**
- [ ] Order creates order + outbox rows in one transaction; messages visible in Kafka UI with key = orderId
- [ ] Kafka stopped → order still accepted → published after Kafka returns
- [ ] Duplicate message processed only once
- [ ] Malformed message lands in `.dlt` and doesn’t block the partition

---

## Sprint 5 — Inventory and stock reservation

**Goal:** stock can never go negative.

- [ ] Inventory projects (Domain, Application, Infrastructure, Api), Dockerfile, Compose service, own DB user
- [ ] `StockItems` (`OnHand`, `Reserved`, check constraint) and `Reservations` (unique `OrderId`)
- [ ] `ReserveStock` handler with atomic conditional UPDATE, all items in one transaction
- [ ] Replies `StockReserved` / `StockReservationFailed` via outbox
- [ ] `CommitReservation` and `ReleaseStock` (+ `StockReleased`)
- [ ] Reservation expiry job
- [ ] Stock admin endpoints and `GET /stock/{productId}`
- [ ] Integration test: 20 parallel reservations for the last item → exactly 1 succeeds

**Sprint demo**
- [ ] Reserve, release, and commit change stock exactly as expected
- [ ] Same `ReserveStock` twice reserves once

---

## Sprint 6 — Payments, Notifications, full saga

**Goal:** checkout reaches `Confirmed` or `Rejected` correctly in every scenario.

- [ ] Order state machine methods (`OnStockReserved`, `OnPaymentSucceeded`, `OnPaymentFailed`, …) + unit tests
- [ ] Orders handlers for incoming events (inbox + state + outbox in one transaction)
- [ ] Payments service: simulator (`.13` fails, failure rate, delay), unique payment per order, `Unknown` status
- [ ] Notifications service: subscribes to order events, unique delivery per order/type, retries, log sender
- [ ] `StateDeadline` + sweeper for stuck orders
- [ ] Late `PaymentSucceeded` on rejected order → `RefundRequired` alert/log
- [ ] End-to-end test against Compose

**Sprint demo**
- [ ] Normal order → `Confirmed`, stock reduced
- [ ] Total ending `.13` → `Rejected`, stock restored
- [ ] Insufficient stock → `Rejected`, no stock change
- [ ] Stop Payments mid-checkout, start again → order completes
- [ ] One notification per final order

---

## Sprint 7 — Observability

**Goal:** one checkout = one trace; problems are visible on dashboards.

- [ ] OpenTelemetry tracing, metrics, logs in ServiceDefaults; OTLP exporter
- [ ] `grafana/otel-lgtm` in Compose
- [ ] Trace context in Kafka headers and outbox rows
- [ ] Correlation ID in log scopes
- [ ] Business metrics: orders placed/completed by outcome, checkout duration, stuck orders, outbox age, DLT count
- [ ] Dashboards as code: Overview, Service RED, Messaging
- [ ] Alert rules: stuck orders, outbox age, consumer lag, DLT messages
- [ ] Runbooks in `docs/runbooks/` for each alert
- [ ] Check: no secrets/PII in logs, no IDs as metric labels

**Sprint demo**
- [ ] One trace spans Orders → Kafka → Inventory → Payments → Notifications
- [ ] Stopping Payments fires the lag/stuck-orders alert

---

## Sprint 8 — BFF and frontend

**Goal:** a customer can shop through a browser.

- [ ] `Web.Bff` with YARP routes to Catalog and Orders
- [ ] Development login (fake user) with HttpOnly cookie session; anti-forgery header
- [ ] Rate limiting on checkout (token bucket per user)
- [ ] Install Node.js LTS + Angular CLI; `ng new shopeasy-web` in `src/Web/` (standalone, routing, strict TypeScript)
- [ ] ESLint (`angular-eslint`) + Prettier; lint and `ng test` in CI
- [ ] Core: `ProductsApi`, `OrdersApi`, `AuthService`; runtime `config.json` loaded at startup
- [ ] Functional HTTP interceptors: anti-forgery header, `401` → BFF login
- [ ] Products page (product list)
- [ ] Cart with signals (persisted in `sessionStorage`)
- [ ] Checkout page with reactive form; idempotency key per checkout attempt, reused on retry; no prices sent
- [ ] Order status page polling with RxJS until `Confirmed` / `Rejected`
- [ ] Order history page behind a route guard
- [ ] Unit/component tests for cart, checkout, and status polling
- [ ] Web container: multi-stage Dockerfile (Node build → unprivileged NGINX) with runtime `config.json`
- [ ] Web + BFF in Compose behind one origin
- [ ] Playwright end-to-end checkout test

**Sprint demo**
- [ ] Buy two keyboards in the browser and watch status go `Pending` → `Confirmed`
- [ ] The trace starts in the browser

---

## Sprint 9 — Kubernetes on kind

**Goal:** ShopEasy runs on a local multi-node cluster with zero-downtime updates.

- [ ] `deploy/local/kind.yaml` (1 control plane + 2 workers) and image load script
- [ ] Kustomize `base/` per service: Deployment, Service, ServiceAccount, ConfigMap
- [ ] Probes (startup, liveness, readiness), resources, security context, topology spread
- [ ] Migration Jobs
- [ ] Envoy Gateway + Gateway + HTTPRoutes
- [ ] HPA for HTTP services; KEDA for consumers
- [ ] PDBs, default-deny NetworkPolicies + allow rules, Pod Security `restricted`
- [ ] `overlays/local`
- [ ] PostgreSQL and Kafka reachable from the cluster

**Sprint demo**
- [ ] Rolling restart of Orders under load → zero failed requests
- [ ] Deleted pod is replaced; DB outage → not ready, not restarted
- [ ] KEDA scales Inventory up and down
- [ ] NetworkPolicy blocks unauthorized pod access

---

## Sprint 10 — Azure with Terraform 💰

**Goal:** the same system running in Azure, passwordless and reproducible.

- [ ] Azure budget + alert created **before** anything else
- [ ] `infrastructure/bootstrap`: state storage (versioning, Entra auth)
- [ ] Modules: network, aks, acr, postgres, eventhubs, keyvault, monitoring
- [ ] `environments/dev` with `dev.tfvars`
- [ ] Managed identity + federated credential per service; role assignments
- [ ] PostgreSQL Entra auth roles per service
- [ ] Event Hubs: hubs and consumer groups matching Kafka topics; Kafka OAuth config in services
- [ ] Gateway API implementation in AKS (Application Gateway for Containers or managed Gateway)
- [ ] `overlays/dev` (ACR images, Workload Identity annotations, Terraform outputs)
- [ ] TFLint + Checkov passing

**Sprint demo**
- [ ] Order through the public endpoint reaches `Confirmed`
- [ ] No passwords in any config; no public access on data services
- [ ] `terraform destroy` + `apply` recreates a working environment
- [ ] Environment destroyed/stopped after the session

---

## Sprint 11 — CI/CD and GitOps 💰

**Goal:** merge to `main` deploys automatically; prod is approved and safe.

- [ ] Reusable service workflow + path filters per service
- [ ] OIDC federation for GitHub; separate identities per job
- [ ] CI checks: format, tests, CodeQL, Dependabot, secret scanning, Hadolint, kubeconform, Checkov
- [ ] Image build: SHA tag, cache, SBOM, provenance, Trivy scan, signing, push to ACR
- [ ] Argo CD installed; Applications for dev and prod
- [ ] CI updates dev overlay image tag; migration Job as PreSync hook
- [ ] E2E test in dev after sync
- [ ] Prod promotion PR with environment approval
- [ ] Argo Rollouts canary for Orders with Prometheus analysis
- [ ] Terraform pipeline: plan in PR, apply saved plan, nightly drift check

**Sprint demo**
- [ ] Merge → dev updated within minutes, nobody runs `kubectl`
- [ ] Broken Orders version aborts its canary automatically
- [ ] `git revert` restores the previous prod version
- [ ] No Azure secret stored in GitHub

---

## Sprint 12 — Production hardening 💰

**Goal:** production readiness checklist complete.

- [ ] Entra External ID tenant, app registrations, scopes and roles
- [ ] BFF OIDC login (code flow + PKCE); JWT validation in every service; own-orders rule
- [ ] Front Door + WAF in front of the Gateway; web static caching
- [ ] Rate limits at edge and BFF verified
- [ ] Load test (Azure Load Testing / k6): targets met, breaking point found
- [ ] Chaos experiments: pod kill, dependency down, latency, node drain
- [ ] Backup restore test; DR drill with measured RTO/RPO
- [ ] Defender for Cloud enabled; findings reviewed
- [ ] Cost review per service; budgets per environment
- [ ] All ADRs written; C4 context and container diagrams in `docs/architecture/`

**Sprint demo**
- [ ] Every item of the production readiness checklist (Ch. 10, §12) ticked

---

## Optional practice tracks

- [ ] gRPC for Orders → Catalog (Ch. 2)
- [ ] `CustomerOrderView` projection (Ch. 5)
- [ ] `OrderConfirmed` v2 using expand–contract (Ch. 4)
- [ ] Azure Service Bus adapter instead of Kafka (Ch. 4)
