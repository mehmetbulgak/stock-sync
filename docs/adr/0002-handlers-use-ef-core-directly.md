# ADR 0002: Handlers use EF Core directly (no repository layer, no mediator library)

**Status:** Accepted

## Context

A common Clean Architecture setup wraps EF Core in generic repositories and dispatches use cases through a mediator library.
In a project this size, both mostly add indirection.

## Decision

- `Application` depends on `IAppDbContext` (EF Core `DbSet`s). Handlers write LINQ queries and project straight to DTOs,
  so EF Core generates the SQL with only the columns each screen needs.
- Every use case is a nested `Handler` class (`CreateProduct.Handler`, `ShipOrder.Handler`). `AddApplication()` registers them
  by convention, and endpoints inject the one handler they call. No mediator package, so no licensing questions and no reflection-heavy pipeline.
- Cross-cutting validation runs in a minimal-API endpoint filter backed by FluentValidation.
- Business rules stay in the domain model. Handlers only load, call one domain method and save.

## Consequences

- Fewer files and nothing hidden behind generic abstractions. Each query is easy to read and tune.
- `Application` references EF Core. That's an accepted trade-off: the EF Core in-memory provider isn't used for tests anyway;
  integration tests run against real PostgreSQL (Testcontainers).
