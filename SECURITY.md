# Security policy

## Reporting a vulnerability

Please do not open a public issue for security problems. Use GitHub's
[private vulnerability reporting](../../security/advisories/new) for this repository instead, with:

- what an attacker can do and which part of the app is affected,
- the steps or a request that reproduces it,
- the commit you tested.

You should get an answer within a week. StockSync is a portfolio project, so there are no bounties, but every valid
report is fixed and credited if you want.

## Things that are intentional

- **The demo account** `demo@stocksync.dev` / `Demo1234!` is public on purpose. It is created only when
  `Database:SeedDemoData` is `true` (Development and the default docker compose setup), and the API logs a warning when it
  is. Anyone can change the demo store's data, so turn it off with `SEED_DEMO_DATA=false` for anything that isn't a demo.
- **Default passwords** in `docker-compose.yml` and `appsettings.Development.json` are for a database bound to
  `127.0.0.1` on your own machine. Set `POSTGRES_PASSWORD` and `APP_DB_PASSWORD` in `.env` for anything else.
- **`style-src 'unsafe-inline'`** in the web app's Content Security Policy: React applies inline `style` attributes (for
  example the width of the stock bars) and Recharts renders inline styles. Scripts stay limited to `'self'`.
- **Registration says when an email is taken.** Sign-in doesn't reveal which accounts exist, but sign-up answers
  `409 Auth.EmailTaken`, like most products without email verification. Sign-up is rate limited per IP.

## Deploying it safely

- Put the stack behind a reverse proxy or load balancer that terminates **TLS** and sends **HSTS**. The refresh token
  cookie is `Secure`, so browsers only store it over HTTPS (or on `localhost`).
- Set **`JWT_SIGNING_KEY`** to 32+ random characters (`openssl rand -base64 48`), not a memorable phrase. Without it
  each API process uses its own random key, which signs everyone out on restart and breaks with more than one instance.
- **Client IPs.** nginx takes the client address from `X-Forwarded-For` only when the request comes from a private
  network (`set_real_ip_from` in `frontend/nginx.conf`), and the API trusts nginx only inside
  `ReverseProxy:KnownNetworks`. If your front proxy has a fixed address, narrow both to it. Clients on those private
  ranges could otherwise choose their own rate-limit bucket.
- **Several API instances.** Rate limits and the per-account sign-in limit are kept in memory per instance. Add a limit
  at the proxy too, or move them to a shared store.
- Keep dependencies current: Dependabot opens weekly updates for NuGet, npm, Docker images and GitHub Actions. Images
  and Actions are referenced by version tag; pin them to digests and commit SHAs if your policy requires it.
