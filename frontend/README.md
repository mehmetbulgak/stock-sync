# StockSync web

React 19 + TypeScript + Vite single-page app. See the [root README](../README.md) for how to run the whole stack.

```bash
npm install
npm run dev        # http://localhost:5173, proxies /api to http://localhost:5080
npm test           # Vitest + Testing Library
npm run lint       # oxlint
npm run format     # Prettier
npm run typecheck
npm run build
```

Each screen lives in `src/features/<feature>/`: the page (lazy loaded), its dialogs, Zod schemas and the TanStack Query
hooks that talk to the API (`api.ts`). Shared building blocks are in `src/components/ui`, and the fetch client with
silent token refresh is in `src/lib/api`.
