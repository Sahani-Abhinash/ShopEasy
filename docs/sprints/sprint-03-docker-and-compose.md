# Sprint 3 — Docker and Docker Compose

**Status:** ◐ In progress
**Started:** 2026-10-04
**Finished:** —
**Guide:** [Chapter 3](../architecture-guide/03-docker-and-docker-compose.md) · **Backlog:** [Sprint 3](../planning/sprint-backlog.md#sprint-3--docker-and-docker-compose)

**Goal:** the whole system starts with one command.

## 1. Tasks

| # | Task | Owner | Status |
|---|---|---|---|
| 1 | Branch `feature/sprint-3-docker` from `main` | 👤 | ☑ Done (from `0f1d605`) |
| 2 | `.dockerignore` at repo root | 🤖 | ☑ Done |
| 3 | Multi-stage Dockerfiles for Catalog and Orders (chiseled, non-root, port 8080) | 🤖 | ☑ Done |
| 4 | EF migration bundle + `migrator` target in each Dockerfile | 🤖 | ☑ Done |
| 5 | `deploy/local/init-databases.sh`: one database + user per service | 🤖 | ☑ Done |
| 6 | `.env.example` committed, `.env` ignored | 🤖 | ☑ Done (`.env` already in `.gitignore`) |
| 7 | `compose.yaml`: PostgreSQL (5433, volume, healthcheck), migrators, Catalog, Orders → `http://catalog-api:8080` | 🤖 | ☑ Done |
| 8 | Stop the Sprint 1 container `shopeasy-pg` (port 5433), `cp .env.example .env`, `docker compose up -d --build` | 👤 | ☑ Done (all 5 containers as expected; first build 5 min 39 s) |
| 9 | Check: place an order → 202; image sizes + non-root; data survives `down`/`up`; `.cs` change doesn't re-run restore | 👤 | ☑ Done |
| 10 | PR, green CI, merge (includes the Sprint 2 close-out doc updates) | 👤 | ☐ |

## 2. Work log

### 2026-10-04 — Sprint start and files (tasks 1–7)

**What we did:** branched from `main` after PR #3. Claude wrote the container setup:

| File | What it does |
|---|---|
| `.dockerignore` | Keeps `bin/`, `obj/`, `.git/`, tests, docs, `.env` and secrets out of the build context |
| `src/Catalog/Catalog.Api/Dockerfile`, `src/Orders/Orders.Api/Dockerfile` | `build` (restore from `.csproj` files first, then publish) → `final` (`aspnet:10.0-noble-chiseled`, port 8080, `USER $APP_UID`). Extra targets `bundle` + `migrator` for the EF migration bundle |
| `deploy/local/init-databases.sh` | Creates `catalog_user`/`catalog_db` and `orders_user`/`orders_db`, revokes `CONNECT` from `PUBLIC` |
| `.env.example` | `POSTGRES_PASSWORD`, `CATALOG_DB_PASSWORD`, `ORDERS_DB_PASSWORD` placeholders |
| `compose.yaml` | `postgres` → `*-migrator` (run once, must succeed) → `catalog-api` (5101) and `orders-api` (5102) |

**Next action:** 👤 task 8.

### 2026-10-04 — Compose up and checks (tasks 8, 9)

**What we did:** stopped `shopeasy-pg`, created `.env`, `docker compose up -d --build` (after the bundle fix, see Issues). Checked:

| Check | Result |
|---|---|
| `docker compose ps -a` | `postgres` healthy, `catalog-migrator` + `orders-migrator` `Exited (0)`, both APIs up |
| `/health/ready` (5101, 5102) | `200 Healthy` |
| `POST /api/v1/orders` (`Idempotency-Key: docker-001`) | `202`; retry → same `orderId`; total **528.98** (Catalog prices, called via `http://catalog-api:8080`) |
| Image user | `1654` (`$APP_UID`, non-root) for all four images |
| Image size (API) | 59 MB compressed ("content size"), ~135 MB unpacked; Docker Desktop shows **194–196 MB "disk usage"** = both together. Our app layer is 9.8 MB |
| Migrator images | 68 MB compressed, 229–230 MB disk usage |
| `orders_user` → `catalog_db` | `User does not have CONNECT privilege` (isolation works) |

**Next action:** 👤 data survives `down`/`up`; `.cs` change doesn't re-run restore; then PR.

### 2026-10-04 — Data survives `down` / `up` (task 9)

**What we did:** containers had stopped with `Exited (255)` (Docker engine stopped; no `restart:` policy) → `docker compose up -d`. In Scalar: `POST /api/v1/orders` (`Idempotency-Key: persist-001`, 1 × monitor) → `202`, `orderId` `01a108c4-3262-7973-b144-69ba1c9a16a2`. `docker compose down` → `up -d` → `GET /api/v1/orders/{id}` → `200`, same order, total **349.00 EUR**. The named volume keeps the data; only `down -v` would delete it.

**Next action:** 👤 `.cs` change doesn't re-run restore; then PR.

### 2026-10-04 — Build cache check (task 9)

**What we did:** added a comment to `Catalog.Api/Program.cs` → `docker compose build catalog-api --progress=plain`. `RUN dotnet restore` = `CACHED`; only `COPY` of the source and `dotnet publish` re-ran. Change reverted. All demo checks done.

**Next action:** 👤 commit, PR, green CI, merge (task 10).

## 3. Issues and fixes

| Date | Problem | Cause | Fix |
|---|---|---|---|
| 2026-10-04 | `docker compose up -d --build` created no containers; `migrator` build failed: `Unable to create a 'DbContext' of type 'CatalogDbContext'` / `Connection string 'CatalogDb' is missing` | `dotnet ef migrations bundle` starts `Program.cs` to build the model; no connection string inside the build | Placeholder `ConnectionStrings__*` (and `Services__Catalog` for Orders) set only on that `RUN` line; real values come from Compose at runtime |

## 4. Decisions made in this sprint

| Decision | Why |
|---|---|
| Init script is **`.sh`**, not `.sql` (backlog said `.sql`) | A `.sql` file can't read environment variables, so per-service passwords would be committed. The script passes them to `psql` as variables |
| `REVOKE CONNECT ... FROM PUBLIC` on each database | Orders' user physically cannot connect to Catalog's database (database per service) |
| Migrator = extra **target in the same Dockerfile** | One file per service; `bundle` stage reuses the restored/built layers. BuildKit skips it when building the API image |
| Migrator image is `aspnet` chiseled + framework-dependent bundle, non-root, `DOTNET_BUNDLE_EXTRACT_BASE_DIR=/tmp` | Same small base as the API; the bundle extracts itself and `/app` isn't writable for the app user |
| APIs start only after their migrator **completed successfully** | No startup migrations in the app (Chapter 3, §7) |
| Postgres healthcheck uses `pg_isready -h localhost` | During init the temporary server listens only on the socket, so "healthy" means the init script has finished |
| `global.json` not copied into the image | It pins SDK 10.0.301; the `sdk:10.0` image may have a different feature band. Build settings come from `Directory.*.props` + `.editorconfig` |
| Compose PostgreSQL replaces the Sprint 1 `shopeasy-pg` container on 5433 | Same port; new volume, new per-service users. Update user-secrets for `dotnet run` (task 8) |

## 5. What I learned

- Layer order is the build cache: copy `*.csproj` / `Directory.*.props` first and restore, then copy the source, so a code change skips restore.
- `dotnet ef migrations bundle` runs `Program.cs` at build time, so it needs placeholder config; real secrets only arrive at runtime.
- Docker Desktop shows "disk usage" (compressed + unpacked); the real image size is the unpacked size (`docker image inspect`).
- `docker compose down` keeps named volumes; `down -v` deletes the data.
- Without a `restart:` policy, containers stay stopped after the Docker engine restarts (`Exited (255)`).
- `depends_on: condition: service_completed_successfully` makes migrations a gate before the APIs start.

## 6. Sprint demo

- [x] `docker compose up -d --build` → place an order → `202`
- [x] Images ~110–130 MB and run as non-root (≈135 MB unpacked / 59 MB compressed; user 1654)
- [x] Data survives `down` / `up` (order `01a108c4-…` read back after `down` / `up`)
- [x] Changing one `.cs` file doesn't re-run `dotnet restore` (restore `CACHED`, only publish re-ran)
