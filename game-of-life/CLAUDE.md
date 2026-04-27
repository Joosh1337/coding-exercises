# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run API with hot reload (http://localhost:5141, Swagger at /swagger)
cd api && dotnet watch

# Run all tests
cd api.Tests && dotnet test

# Run tests with verbose output
cd api.Tests && dotnet test --verbosity detailed

# Build
dotnet build

# Restore packages
dotnet restore
```

## Architecture

This is a **.NET 8.0 ASP.NET Core REST API** implementing Conway's Game of Life with a classic layered architecture:

```
Controllers → Services → Repositories → LiteDB
```

**Key separation:** `Board` (persisted entity with initial state) vs `BoardState` (computed at runtime, never persisted). Intermediate states are never cached — each request recomputes from the stored initial state.

**Request/Response flow:**
- All responses are wrapped in `ApiResponse<T>` (success) or `ErrorResponse` (failure)
- Custom exceptions (`BoardNotFoundException`, `InvalidBoardStateException`, `InvalidStepsException`, `NoFinalStateException`) are caught by `GlobalExceptionHandlingMiddleware` and mapped to HTTP status codes (404, 400, 422)

**Persistence:** LiteDB embedded file database (`game_of_life.db`), abstracted behind `IBoardRepository`. `LiteBoardRepository` stores `Board` entities indexed by `Guid`.

**Stable state detection** (`StableStateDetection.cs`): iterates up to `MaxIterationsForFinalState` (configured in `appsettings.json`, default 9999) to find a stable/repeating state, throwing `NoFinalStateException` (422) if not found.

## API Endpoints

Base: `http://localhost:5141/api/boards`

| Method | Route | Notes |
|--------|-------|-------|
| POST | `/boards` | Create board; body: `{ width, height, initialCells }` |
| GET | `/boards?page=1&pageSize=10` | Paginated list |
| GET | `/boards/{id}` | Get board at generation 0 |
| GET | `/boards/{id}/states/next` | Advance one generation |
| GET | `/boards/{id}/states?steps=N` | Advance N generations |
| GET | `/boards/{id}/states/final` | Compute stable state |
| DELETE | `/boards/{id}` | Delete board |

`/states/*` endpoints return `BoardRepresentationResponse` — a full 2D grid (1=alive, 0=dead). Other board endpoints return `BoardResponse` — live cells as `[x, y]` coordinate arrays.

## Game of Life Implementation

Core logic lives in `BoardState.GenerateNextStep()`. Uses `HashSet<(int x, int y)>` for O(1) live cell lookup and a neighbor-count dictionary to apply the four standard rules. The `BoardState` constructor and `GenerateNextStep` are the most important methods for understanding state transitions.

## Testing

Tests are in `api.Tests/` using XUnit + Moq + FluentAssertions. Test files mirror the `api/` structure. The `BoardStateTests` cover Game of Life rule correctness (underpopulation, survival, overpopulation, reproduction, oscillators, boundary conditions) and are the best place to verify rule changes.
