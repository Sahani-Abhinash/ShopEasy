# ShopEasy documentation

ShopEasy is a small ecommerce system for learning microservices on Azure, from design to production.

## Architecture guide

| # | Chapter | Topics |
|---|---|---|
| 1 | [Requirements and architecture](architecture-guide/01-requirements-and-architecture.md) | Scope, service boundaries, data ownership, saga, ADRs |
| 2 | [ASP.NET Core services: Catalog and Orders](architecture-guide/02-aspnet-core-services-catalog-orders.md) | .NET 10, Minimal APIs, Clean Architecture, EF Core, idempotency |
| 3 | [Docker and Docker Compose](architecture-guide/03-docker-and-docker-compose.md) | Multi-stage images, Compose, config, migrations, registry |
| 4 | [Messaging: Kafka and the outbox](architecture-guide/04-messaging-kafka-and-outbox.md) | Commands/events, Kafka, outbox, inbox, DLT, schema evolution, Dapr |
| 5 | [Checkout saga: Inventory, Payments, Notifications](architecture-guide/05-checkout-saga-inventory-payments-notifications.md) | Orchestration, state machine, stock reservation, timeouts, CQRS |
| 6 | [Kubernetes](architecture-guide/06-kubernetes.md) | Deployments, probes, Gateway API, HPA/KEDA, Kustomize, service mesh |
| 7 | [Azure infrastructure with Terraform](architecture-guide/07-azure-infrastructure-with-terraform.md) | State, modules, AKS, PostgreSQL, Event Hubs, Workload Identity |
| 8 | [Observability: OpenTelemetry, Prometheus, Grafana](architecture-guide/08-observability-opentelemetry-prometheus-grafana.md) | Metrics, traces, logs, dashboards, SLOs, alerts |
| 9 | [CI/CD: GitHub Actions and GitOps](architecture-guide/09-cicd-github-actions-and-gitops.md) | Pipelines, OIDC, scanning, Argo CD, canary, Terraform pipeline |
| 10 | [Production readiness and final architecture](architecture-guide/10-production-readiness-and-final-architecture.md) | Auth, Angular frontend, gateway/BFF, rate limiting, chaos, DR, cost |
| 11 | [Appendix: glossary, interview Q&A, roadmap](architecture-guide/11-appendix-glossary-interview-qa-roadmap.md) | Glossary, C4 + ADRs, 40 interview questions, implementation phases |

## Planning

- [Sprint backlog](planning/sprint-backlog.md) — sprint-wise tasks with checkboxes
- [Sprint logs](sprints/) — per-sprint record of what we did, what was missing, and fixes
  - [Sprint 0 — Repository and tooling](sprints/sprint-00-repository-and-tooling.md) ☑
  - [Sprint 1 — Catalog service](sprints/sprint-01-catalog-service.md) ☑
  - [Sprint 2 — Orders service](sprints/sprint-02-orders-service.md) ◐

## Requirements

- [Original requirements](requirements/original-requirements.md)
