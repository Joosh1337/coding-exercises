# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## API (`api/` + `api.Tests/`)

**.NET 8.0 ASP.NET Core REST API** implementing Conway's Game of Life.

### Commands

```bash
# Run with hot reload (http://localhost:5141, Swagger at /swagger)
cd api && dotnet watch

# Run tests
cd api.Tests && dotnet test

# Run tests with verbose output
cd api.Tests && dotnet test --verbosity detailed

# Build / restore
dotnet build
dotnet restore
```

### Architecture

```
Controllers → Services → Repositories → LiteDB
```

**Key separation:** `Board` (persisted entity with initial state) vs `BoardState` (computed at runtime, never persisted). Intermediate states are never cached — each request recomputes from the stored initial state.

**Request/Response flow:**
- All responses are wrapped in `ApiResponse<T>` (success) or `ErrorResponse` (failure)
- Custom exceptions (`BoardNotFoundException`, `InvalidBoardStateException`, `InvalidStepsException`, `NoFinalStateException`) are caught by `GlobalExceptionHandlingMiddleware` and mapped to HTTP status codes (404, 400, 422)

**Persistence:** LiteDB embedded file database (`game_of_life.db`), abstracted behind `IBoardRepository`. `LiteBoardRepository` stores `Board` entities indexed by `Guid`.

**Stable state detection** (`StableStateDetection.cs`): iterates up to `MaxIterationsForFinalState` (configured in `appsettings.json`, default 9999) to find a stable/repeating state, throwing `NoFinalStateException` (422) if not found.

### Endpoints

Base: `http://localhost:5141/api/boards`

| Method | Route | Notes |
|--------|-------|-------|
| POST | `/boards` | Create board; body: `{ name, width, height, initialCells }` |
| GET | `/boards?page=1&pageSize=10` | Paginated list |
| GET | `/boards/{id}` | Get board at generation 0 |
| PUT | `/boards/{id}` | Update board; body: `{ name, width, height, initialCells }` |
| GET | `/boards/{id}/states/next` | Advance one generation |
| GET | `/boards/{id}/states?steps=N` | Advance N generations |
| GET | `/boards/{id}/states/final` | Compute stable state |
| DELETE | `/boards/{id}` | Delete board |

`/states/*` endpoints return `BoardRepresentationResponse` — a full 2D grid (1=alive, 0=dead). Other board endpoints return `BoardResponse` — live cells as `[x, y]` coordinate arrays.

### Game of Life Implementation

Core logic lives in `BoardState.GenerateNextStep()`. Uses `HashSet<(int x, int y)>` for O(1) live cell lookup and a neighbor-count dictionary to apply the four standard rules. The `BoardState` constructor and `GenerateNextStep` are the most important methods for understanding state transitions.

### Testing

Tests in `api.Tests/` using XUnit + Moq + FluentAssertions. Test files mirror the `api/` structure. `BoardStateTests` cover rule correctness (underpopulation, survival, overpopulation, reproduction, oscillators, boundary conditions) and are the best place to verify rule changes.

---

## Frontend (`frontend/`)

React 19 SPA built with Vite, TypeScript, Tailwind CSS, TanStack Query, and React Router v7.

### Commands

```bash
# Run dev server (http://localhost:5173)
cd frontend && npm run dev

# Run tests
cd frontend && npm test

# Run tests with coverage
cd frontend && npm run test:coverage
```

### Architecture

```
pages/ → hooks/ → api/client.ts → API
```

- All API calls in `frontend/src/api/client.ts`
- Each React Query hook lives in `frontend/src/hooks/`
- API base URL set via `VITE_API_URL` env var; defaults to `http://localhost:5141/api/boards`

### Pages (routes)

| Route | Component | Purpose |
|-------|-----------|---------|
| `/` | `BoardListPage` | Paginated board list with delete |
| `/boards/new` | `CreateBoardPage` | Draw initial state on a grid, set name/dimensions |
| `/boards/:id` | `BoardSimulationPage` | Step through generations (Next, N steps, End/final state) |
| `/boards/:id/edit` | `EditBoardPage` | Edit board name, dimensions, and initial cells |

### Testing

Vitest + Testing Library + jsdom. Test files co-located with source (`*.test.tsx`). Coverage threshold: 80% lines/functions/branches/statements enforced in `vite.config.ts`.
