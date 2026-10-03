# 0001 — Build ShopEasy as microservices

**Status:** Accepted
**Date:** 2026-10-03

## Context

ShopEasy is a small ecommerce system. The explicit goal is to learn how to design, deploy, and operate microservices on Azure (independent deployment, messaging, Kubernetes, observability, CI/CD). Business complexity is deliberately small.

## Options

1. Monolith: one application, one database
2. Modular monolith: one deployable unit, strict internal module boundaries
3. Microservices: five services split by business capability (Catalog, Orders, Inventory, Payments, Notifications)

## Decision

We choose **microservices**, with boundaries around business capabilities and each service owning its data. For a real shop of this size a modular monolith would be the sensible start; microservices are chosen because learning them is the goal.

## Consequences

- ➕ Independent build, deployment, and scaling per service
- ➕ Covers the distributed-system concepts we want to learn (sagas, messaging, idempotency, tracing)
- ➖ Network failures, duplicate messages, eventual consistency
- ➖ More components to build, deploy, and monitor
- Revisit if: the project is used for a real business with a small team, then consider a modular monolith
