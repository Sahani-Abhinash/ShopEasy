# 0003 — Orders orchestrates checkout as a saga

**Status:** Accepted
**Date:** 2026-10-03

## Context

Checkout spans Inventory (reserve stock), Payments (charge), and Notifications. There is no transaction across services. Failures (payment declined, service down) must leave the system consistent, for example with stock released.

## Options

1. Synchronous HTTP chain from Orders with a manual rollback
2. Choreography: each service reacts to the others’ events
3. Orchestration: Orders coordinates the steps with commands and events, and runs compensations

## Decision

We choose an **orchestrated saga in Orders**: Orders sends commands (`ReserveStock`, `ProcessPayment`, `ReleaseStock`) and reacts to events. Compensation releases stock when payment fails. Notifications subscribes to the final events (choreography for side effects).

## Consequences

- ➕ The workflow and the current state of each order are visible in one place
- ➕ Compensation and timeouts are easy to reason about
- ➖ Orders knows its participants and owns the coordination logic
- ➖ Requires idempotent handlers, an outbox, and timeouts
- Revisit if: many independent workflows appear, then consider a workflow engine (Temporal, Dapr Workflow)
