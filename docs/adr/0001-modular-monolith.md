# ADR 0001: A modular monolith, not microservices

**Status:** Accepted

## Context

StockSync's hardest rule is "never sell a unit you don't have", across several sales channels at once.
That rule needs a product's stock and the order that reserves it to change in **one transaction**.

## Decision

One deployable API, split by Clean Architecture layers:

| Project | Depends on | Holds |
|---|---|---|
| `Domain` | nothing | Entities and invariants (`Product.Reserve`, `Order.Place`), `Result`/`Error` |
| `Application` | Domain | Use cases (one `Handler` per action), validation, DTOs |
| `Infrastructure` | Application | EF Core + PostgreSQL, JWT, password hashing, seeding |
| `Api` | all | HTTP endpoints, auth middleware, error mapping |

Channel integrations (phase 2) will run in a **separate worker process** that talks to the same database
through an outbox, so a slow marketplace API can never block a seller's request.

## Consequences

- Stock and orders stay consistent with plain database transactions, without sagas or distributed locks.
- One thing to deploy and debug. Kubernetes isn't needed.
- If one module ever needs to scale on its own, the layer boundaries are already the seams to split along.
