# ShopEasy

A small ecommerce system built as **microservices on .NET 10, Kafka, Kubernetes, and Azure**, for learning how to design, build, deploy, and operate a distributed system end to end.

A customer browses products, places an order, stock is reserved, a (simulated) payment is processed, and the customer is notified, with retries, duplicates, and failures handled correctly.

## Architecture at a glance

| Service | Responsibility |
|---|---|
| Catalog | Products and current prices |
| Orders | Order lifecycle; coordinates the checkout saga |
| Inventory | Stock and reservations |
| Payments | Payment attempts (simulated provider) |
| Notifications | Customer notifications |
| Web + BFF | Angular frontend and backend-for-frontend (YARP) |

Full design: [docs/README.md](docs/README.md) (10-chapter architecture guide).

## Repository layout

```text
src/              service code (one folder per service) + BuildingBlocks
tests/            unit and integration tests
deploy/           Docker Compose, Kubernetes (Kustomize), Argo CD
infrastructure/   Terraform for Azure
docs/             architecture guide, ADRs, sprint backlog and logs
```

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 10.0.x (pinned in `global.json`) |
| Docker Desktop | with WSL 2 / Linux containers |
| Node.js | LTS (frontend, from Sprint 8) |
| Azure CLI, Terraform, kubectl, kind, Helm | from Sprint 9 |

## Build and test

```bash
dotnet restore ShopEasy.slnx
dotnet build ShopEasy.slnx
dotnet test --solution ShopEasy.slnx   # needs Docker running (Testcontainers)
dotnet format ShopEasy.slnx --verify-no-changes
```

How to run the services locally will be added in Sprint 3 (Docker Compose).

## Project status

See the [sprint backlog](docs/planning/sprint-backlog.md) and the [sprint logs](docs/sprints/).
