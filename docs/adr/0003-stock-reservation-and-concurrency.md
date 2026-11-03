# ADR 0003: Reserve stock on order, with optimistic concurrency

**Status:** Accepted

## Context

A seller lists the same product on Trendyol, Hepsiburada and their own store. Two customers can buy the last unit on
two channels within the same second. Both requests read "1 available"; without protection both succeed and the seller oversells.

## Decision

1. **Three numbers per product:** `OnHand` (in the warehouse), `Reserved` (promised to open orders) and
   `Available = OnHand - Reserved`, the figure every channel sells against.
2. **Order lifecycle:** placing an order *reserves* units; shipping *commits* them (`OnHand` and `Reserved` both drop);
   cancelling *releases* them. `Order.Place` checks every line before reserving any, so it is all-or-nothing.
3. **Optimistic concurrency:** `products` and `orders` use PostgreSQL's `xmin` system column as a row version.
   If two transactions modify the same product, the second `UPDATE ... WHERE xmin = @old` affects 0 rows,
   EF Core throws `DbUpdateConcurrencyException` and the API answers `409 Conflict`. The client (or, in phase 2, the worker) retries.
4. **Defence in depth:** a `CHECK (reserved >= 0 AND on_hand >= reserved)` constraint in the database.
5. **Ledger:** every change writes a `stock_movements` row with the resulting on-hand and reserved values.

The integration test `Concurrent_orders_for_the_last_units_never_oversell` fires 12 parallel orders at 3 units and asserts
that at most 3 succeed and that `Reserved` equals the number of successful orders.

## Alternatives considered

- **Pessimistic locking (`SELECT ... FOR UPDATE`):** simpler retries, but holds locks while the request runs and
  serialises all traffic for popular products. This could be revisited for very hot SKUs.
- **Atomic SQL (`UPDATE ... SET reserved = reserved + n WHERE on_hand - reserved >= n`):** fastest, but moves the
  business rule out of the domain model. A good fit for the phase 2 bulk importer if throughput requires it.
