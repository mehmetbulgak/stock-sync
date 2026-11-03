# StockSync

StockSync is a full-stack web application for managing products, inventory and orders from multiple sales channels in one dashboard. It keeps a shared stock count so sellers can see what is available, reserved and shipped.

[Open the live demo](https://stock-sync-h869.onrender.com/)

Click **Open the demo store** to explore the application without creating an account.

## What it does

- Manage products, prices and stock levels.
- View orders by sales channel and status.
- Reserve stock when an order is created, release it when cancelled and deduct it when shipped.
- Track stock movements and identify products running low.
- View sales totals, recent orders and revenue by channel.
- Keep each store’s products and orders separate from other stores.

For example, if a product has 10 units and an order reserves 3, only 7 remain available. Cancelling the order makes all 10 available again. Shipping it reduces the physical stock to 7.

## Built with

- **Backend:** C#, ASP.NET Core 10, Entity Framework Core, PostgreSQL.
- **Frontend:** React, TypeScript, TanStack Query, Tailwind CSS.
- **Testing and delivery:** xUnit, integration tests with PostgreSQL, Vitest, Docker, GitHub Actions.

## Engineering highlights

- Stock changes for an order are saved together in a database transaction.
- Concurrent orders are checked to prevent selling the same remaining stock twice.
- Authentication uses short-lived access tokens and rotating refresh tokens in HttpOnly cookies.
- Automated tests cover stock workflows, store isolation and authentication.

## Current scope

Orders are created inside the application. Trendyol, Hepsiburada, web store and manual orders are represented as sales channels. Connections to real marketplace APIs are planned.

The live demo uses shared sample data that resets when the service restarts. The first visit may take about a minute to load.

## Run locally

With Docker installed:

```bash
docker compose up --build
```

Open [localhost:8080](http://localhost:8080) and select **Open the demo store**.

See the [development guide](docs/development.md) for architecture, configuration, API endpoints and test commands, or the [security notes](SECURITY.md) for deployment considerations.
