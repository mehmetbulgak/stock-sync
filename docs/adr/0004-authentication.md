# ADR 0004: Short-lived JWTs in memory + rotating refresh tokens in HttpOnly cookies

**Status:** Accepted

## Decision

- **Access token:** a 15-minute JWT returned in the response body. The SPA keeps it **in memory only** (never
  `localStorage`), so an XSS bug cannot exfiltrate a persisted credential.
  - Signed with HMAC-SHA256, and validation accepts **only** HS256, so `alg: none` or algorithm-confusion tokens are rejected.
  - Issuer, audience, lifetime and signature are all required, with 30 seconds of clock skew.
  - Claims are limited to `sub`, `tenant_id`, `role` and `jti`. The token is readable by anyone who holds it, so it carries
    no email address or name.
- **Signing key:** read from `Jwt:SigningKey` (32+ characters). Nothing is committed to the repository. If the key is
  missing, the API generates a random 64-byte key for the lifetime of the process and logs a warning: a forgotten setting
  can never fall back to a publicly known key. A fixed key is required for more than one instance or for sessions that
  survive a restart.
- **Refresh token:** 64 random bytes in an `HttpOnly; Secure; SameSite=Strict; Path=/api/auth` cookie. The database stores
  only its SHA-256 hash. JavaScript can't read the token, and the browser sends it only to the auth endpoints.
- **Rotation with reuse detection:** every refresh revokes the presented token and issues a new one.
  - Presenting a token that was rotated more than **30 seconds** ago means it was probably stolen, so **every** session of
    that user is revoked.
  - Within those 30 seconds the request fails with `Auth.RefreshTokenRotated` and **leaves the cookie alone**, because the
    browser already holds the winner's new token. The SPA retries once, so two tabs refreshing at once both stay signed in.
  - The refresh token row has a row version (`xmin`). If two requests rotate the same token at the same moment, only one
    commits; the other gets `401`, so a single token can never fork into two live sessions.
- **Absolute session lifetime:** each refresh token records when its session started. Rotation never extends a session
  past `Jwt:SessionLifetimeDays` (30 by default), so a stolen token chain cannot be kept alive forever.
- **Passwords:** ASP.NET Core Identity's PBKDF2 hasher (HMAC-SHA512, **210,000 iterations**), without the rest of Identity.
  Hashes made with older settings are upgraded on the next successful sign-in. Unknown emails still run a comparison
  against a decoy hash, so response timing doesn't reveal which emails exist.
- **Brute force:**
  - Register and login: 10 requests per minute per client IP. Refresh and logout have their own, larger budget, so a
    burst of bad logins never blocks session refresh for everyone else.
  - Failed sign-ins are also counted per account: after 10 failures in 15 minutes that account answers `429` for the rest
    of the window, from any IP. Successful sign-ins don't count.
  - Behind nginx, the client IP comes from `X-Forwarded-For` only when the previous hop is a private (trusted) address,
    so the per-IP limits stay per client instead of becoming one shared bucket.
  - Everything else: 300 API requests per minute per user.
- **Multi-tenancy:** the JWT carries `tenant_id`, and EF Core global query filters scope every tenant-owned table to it,
  so a forgotten `Where` clause cannot leak another seller's data. Integration tests cover reads, writes, orders and shipping.

## Consequences

- The SPA silently refreshes on page load and on any `401`. Concurrent refreshes in one tab share one request
  (`frontend/src/lib/api/session.ts`).
- Revoking a session takes effect at the next refresh (at most 15 minutes for an access token already issued).
- The per-account limit lets someone pause sign-in for a known email address for up to 15 minutes. That is the usual
  trade-off for slowing distributed password guessing; existing sessions keep working.
- Rate limit counters live in memory, per API instance.
- Without `Jwt:SigningKey`, restarting the API signs everyone out. That is acceptable for the demo and is called out in the
  startup log.
