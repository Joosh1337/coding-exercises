# Game of Life — Frontend

React 19 SPA for the Game of Life API. Built with Vite, TypeScript, Tailwind CSS, TanStack Query, and React Router v7.

## Getting started

```bash
npm install
cp .env.example .env   # set VITE_API_URL if the API runs elsewhere
npm run dev            # http://localhost:5173
```

The API must be running at the URL specified by `VITE_API_URL` (default: `http://localhost:5141/api/boards`). See `../api/` for setup instructions.

## Commands

| Command | Description |
|---------|-------------|
| `npm run dev` | Start dev server with HMR |
| `npm run build` | Type-check and build for production |
| `npm run lint` | Run ESLint |
| `npm test` | Run Vitest in watch mode |
| `npm run test:coverage` | Run tests with coverage report |

## Structure

```
src/
  api/        API client (all fetch calls)
  components/ Shared UI components
  hooks/      React Query hooks (one per API operation)
  pages/      Route-level page components
  types/      TypeScript interfaces for API responses
  utils/      Pure utility functions
```

## Testing

Vitest + Testing Library + jsdom. Test files are co-located with source (`*.test.tsx`). Coverage threshold: 80% across lines, functions, branches, and statements (enforced in `vite.config.ts`).
