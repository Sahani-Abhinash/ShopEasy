# 0002 — Database per service on PostgreSQL

**Status:** Accepted
**Date:** 2026-10-03

## Context

Services must change and deploy independently. Shared tables would couple their schemas. Orders and stock are relational and need transactions.

## Options

1. One shared database for all services
2. Database per service on one PostgreSQL server (separate databases and credentials)
3. Separate database servers per service
4. Different store per service (e.g., Cosmos DB for Catalog)

## Decision

We choose **database per service on a shared PostgreSQL server**: one database and one user per service. A service never reads or writes another service’s tables; it uses APIs, messages, or local snapshots instead. PostgreSQL runs identically locally, in Docker, and in Azure (Flexible Server).

## Consequences

- ➕ Schema changes stay inside one service
- ➕ One server keeps cost and operations low
- ➖ Cross-service queries need API composition or projections
- ➖ A shared server is a shared failure and capacity point
- Revisit if: one service’s load or availability needs differ strongly, then give it its own server
