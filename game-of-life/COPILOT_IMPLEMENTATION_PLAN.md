# Implementation Plan: Conway's Game of Life API

## Overview
Build a production-ready RESTful API for Conway's Game of Life using .NET 8.0 with LiteDB persistence, ASP.NET Controllers, and a repository-based architecture. Implement using TDD (unit tests first), ensuring comprehensive coverage of Game of Life rules and edge cases.

**Key Architecture Decisions:**
- API: ASP.NET Controllers (not minimal APIs)
- Persistence: Repository pattern on top of LiteDB
- State Computation: BoardState computed on-the-fly (not cached in DB)
- Max Iterations: Configurable via appsettings.json (default 9999)
- Error Handling: Standardized response DTOs with meaningful error messages

---

## Phase 1: Domain Models & Core Logic

### 1.1 Create Domain Model Classes
**Files to create:**
- `api/Models/Board.cs` — Persistent entity (ID, Width, Height, LiveCells)
- `api/Models/BoardState.cs` — Runtime computed state (Generation, Width, Height, LiveCells + logic)

**CellCoordinate Requirements:**
This exists to help with DB mapping
- `int X`
- `int Y`
- `override Equals` (to compare by value rather to reference)

**Board.cs Requirements:**
- `Guid Id` (unique identifier)
- `int Width`, `int Height` (board dimensions)
- `List<CellCoordinate> LiveCells` (state representation)

**BoardState.cs Requirements:**
- `int Generation` (generation number)
- `int Width`, `int Height`
- `HashSet<(int x, int y)> LiveCells`
- `GenerateNextStep()` → `BoardState` method (implements the 4 rules of Life)
- `GenerateBoardArray()` → `int[,]` method (returns 2D array for API responses)
- Constructor that accepts Board, must convert a CellCoordinate List to a tuple HashSet

### 1.2 Implement BoardState.GenerateNextStep()
**Logic:** Implement the pseudocode from PLANNING.md
- Loop through live cells and count neighbors
- Apply the 4 Conway's Game of Life rules:
  1. Live cell with <2 neighbors → dies
  2. Live cell with 2-3 neighbors → survives
  3. Live cell with >3 neighbors → dies
  4. Dead cell with exactly 3 neighbors → becomes alive

### 1.3 Implement Stable State Detection Logic
**File:** `api/Models/StableStateDetection.cs`

Create utility class for detecting when a board reaches a final state:
- Method: `IsStable(BoardState current, BoardState previous)` → `bool`
  - Returns true if current.LiveCells == previous.LiveCells
- Method: `HasStableStateWithinLimit(Board board, int maxIterations)` → `bool`
  - Iterates up to maxIterations computing states
  - Returns whether the board stabilizes or cycles

---

## Phase 2: Repository Layer & Persistence

### 2.1 Create Repository Interface
**File:** `api/Repositories/IBoardRepository.cs`

Methods:
- `CreateBoard(Board board)` → `Task<Board>` (saves and returns with ID populated)
- `GetBoardById(Guid id)` → `Task<Board?>` (returns null if not found)
- `DeleteBoard(Guid id)` → `Task<bool>` (returns true if deleted, false if not found)
- `BoardExists(Guid id)` → `Task<bool>`

### 2.2 Implement LiteDB Repository
**File:** `api/Repositories/LiteBoardRepository.cs`

Requirements:
- Use injected LiteDatabase instance
- Create "boards" collection with ID as primary key
- Handle LiteDB database operations (create, read, delete)
- Map between domain Board model and LiteDB documents
- All methods should be async (use Task-based pattern)

### 2.3 Configure LiteDB in Program.cs
- Create or connect to `game_of_life.db` file in application data directory
- Register LiteDatabase as singleton in DI container
- Register IBoardRepository as transient (or scoped) in DI container

---

## Phase 3: Unit Tests (TDD) — **Write BEFORE Implementation**

### 3.1 Create Test File Structure
**File:** `api.Tests/Models/BoardStateTests.cs`

**Test Cases for GenerateNextStep():**
1. **Underpopulation (Rule 1)**
   - Single live cell → dies (0 neighbors)
   - Two adjacent live cells → both die (1 neighbor each)

2. **Survival (Rule 2)**
   - Live cell with 2 neighbors → survives
   - Live cell with 3 neighbors → survives

3. **Overpopulation (Rule 3)**
   - Live cell with 4 neighbors → dies

4. **Reproduction (Rule 4)**
   - Dead cell with exactly 3 live neighbors → becomes alive

5. **Empty Board**
   - Cross (empty): `00/00` stays `00/00`

6. **Oscillators (Blinker)**
   - 1x3 vertical → 3x1 horizontal → 1x3 vertical (period 2)

7. **Boundary Handling**
   - Live cells at edges don't cause out-of-bounds errors
   - Cells outside grid are treated as dead

### 3.2 Create Test File for StableStateDetection Detection
**File:** `api.Tests/Models/StableStateDetectionTests.cs`

**Test Cases:**
1. Stable board (all cells dead for 5 iterations)
2. Board with 2-iteration cycle
3. Board that stabilizes after N generations
4. Max iteration limit reached (no final state detected)

### 3.3 Create Integration Tests for Repository
**File:** `api.Tests/Repositories/LiteBoardRepositoryTests.cs`

**Test Cases:**
1. Create and retrieve board
2. Two identical boards have different IDs
3. Retrieve non-existent board returns null
4. Delete board removes it from persistence
5. Multiple saves and retrieves maintain state integrity

### 3.4 Create Tests for Board State Computation
**File:** `api.Tests/Services/GameOfLifeServiceTests.cs` (covered in Phase 4)

---

## Phase 4: Business Logic Service Layer

### 4.1 Create Game of Life Service
**File:** `api/Services/IGameOfLifeService.cs` + `api/Services/GameOfLifeService.cs`

**Methods:**
- `CreateBoard(int width, int height, int[,] initialCells)` → `Task<Guid>`
  - Converts live cells in 2D array to HashSet<(int, int)>
  - Creates Board entity with Guid.NewGuid()
  - Persists via repository
  - Validation: width/height > 0, cells within bounds

- `GetBoardState(Guid boardId)` → `Task<BoardState>`
  - Retrieves Board from repository
  - Returns BoardState at generation 0
  - Throws if board not found

- `GetStatesAhead(Guid boardId, int steps)` → `Task<BoardState>`
  - Validates steps > 0 (throw if negative or zero)
  - Computes N generations ahead
  - Returns final state only

- `GetFinalState(Guid boardId)` → `Task<BoardState>`
  - Uses StableStateDetection to find stable state
  - Respects configurable max iterations (from appsettings)
  - Throws `NoFinalStateException` if max iterations exceeded

- `DeleteBoard(Guid boardId)` → `Task<bool>`
  - Delegates to repository

**Dependency Injection:**
- Inject IBoardRepository
- Inject IConfiguration (for max iterations setting)

### 4.2 Create Custom Exceptions
**File:** `api/Exceptions/GameOfLifeExceptions.cs`

- `BoardNotFoundException(Guid id)` — Board not found
- `InvalidBoardStateException(string message)` — Invalid dimensions or bounds
- `NoFinalStateException(int maxIterations)` — Max iterations exceeded
- `InvalidStepsException(int steps)` — Steps <= 0

---

## Phase 5: API Controllers & Endpoints

### 5.1 Create Response DTOs
**File:** `api/Dtos/ApiResponses.cs`

**SuccessResponse<T>:**
- `T Data`
- `string Message`

**ErrorResponse:**
- `int ErrorCode`
- `string Message`
- `string[] Details` (array of validation errors if any)

**BoardResponse:**
- `int Generation`
- `int Width, Height`
- `int[][] LiveCells` (array of [x, y] coordinates)

### 5.2 Create Boards Controller
**File:** `api/Controllers/BoardsController.cs`

**Endpoints (6 total):**

1. **POST /boards**
   - Request body: `{ width: int, height: int, cells: int[][] }`
   - Response: `{ id: Guid }`
   - Calls GameOfLifeService.CreateBoard()
   - Error: 400 if invalid input, 500 if persistence fails

2. **GET /boards/{id}**
   - Response: Board state at generation 0
   - Uses BoardResponse format
   - Error: 404 if board not found

3. **GET /boards/{id}/states/next**
   - Response: Next generation state
   - Calls `GameOfLifeService.GetStatesAhead(boardId, 1)` directly
   - Uses BoardResponse format
   - Error: 404 if board not found

4. **GET /boards/{id}/states?steps=N**
   - Query parameter: steps (int)
   - Response: State after N generations
   - Calls `GameOfLifeService.GetStatesAhead(boardId, steps)`
   - Validation: steps > 0 (400 if not)
   - Error: 404 if board not found

5. **GET /boards/{id}/states/final**
   - Response: Final stable state
   - Runs up to configured max iterations (9999 default)
   - Error: 404 if board not found, 422 if no final state within max iterations

6. **DELETE /boards/{id}**
   - Response: 204 No Content if successful
   - Error: 404 if board not found

### 5.3 Add Error Handling Middleware
**Update:** `api/Program.cs`

Create global exception handler or use controller-level try-catch:
- Map custom exceptions to appropriate HTTP status codes
- Return standardized ErrorResponse DTOs
- Log exceptions for debugging

---

## Phase 6: Configuration & Setup

### 6.1 Update appsettings.json
Add configuration:
```json
{
  "GameOfLife": {
    "MaxIterationsForFinalState": 9999,
    "DatabasePath": "game_of_life.db"
  }
}
```

### 6.2 Update Program.cs
- Register all services (Repository, Service)
- Configure LiteDB
- Register exception handlers
- Ensure Swagger still works for documentation

---

## Implementation Sequence (Dependencies)

**Phase 1 → (PARALLEL) Phase 2 + Phase 3 → Phase 4 → Phase 5 → Phase 6**

| Step | Task | Depends On |
|------|------|-----------|
| 1    | Board & BoardState models | — |
| 2    | StableStateDetection utility | Step 1 |
| 3    | Write all unit tests (failing) | Steps 1-2 |
| 4    | IBoardRepository interface | Step 1 |
| 5    | LiteBoardRepository implementation | Steps 3-4 |
| 6    | Update Program.cs (LiteDB setup) | Step 5 |
| 7    | GameOfLifeService interface + class | Steps 1-2, 5-6 |
| 8    | Custom exceptions | — |
| 9    | Run all unit tests (should pass) | Steps 3, 5, 7-8 |
| 10   | Response DTOs | — |
| 11   | BoardsController | Steps 7, 10 |
| 12   | Final Program.cs setup (middleware, DI) | Steps 8, 11 |
| 13   | Test API endpoints manually (Swagger/curl) | Step 12 |

---

## Files to Create/Modify

**New Files:**
- `api/Models/Board.cs`
- `api/Models/BoardState.cs`
- `api/Models/StableStateDetection.cs`
- `api/Repositories/IBoardRepository.cs`
- `api/Repositories/LiteBoardRepository.cs`
- `api/Services/IGameOfLifeService.cs`
- `api/Services/GameOfLifeService.cs`
- `api/Exceptions/GameOfLifeExceptions.cs`
- `api/Controllers/BoardsController.cs`
- `api/Dtos/ApiResponses.cs`
- `api.Tests/Models/BoardStateTests.cs`
- `api.Tests/Models/StableStateDetectionTests.cs`
- `api.Tests/Repositories/LiteBoardRepositoryTests.cs`
- `api.Tests/Services/GameOfLifeServiceTests.cs`

**Modified Files:**
- `api/Program.cs` — Add service registration, LiteDB setup, middleware
- `appsettings.json` — Add GameOfLife configuration section

---

## Verification Steps

### Unit Tests
1. All tests in `api.Tests` project run and pass
   - `dotnet test` in api.Tests directory
   - Target: 100% pass rate

### Manual Endpoint Testing (via Swagger or curl)
1. POST /boards → verify board created with unique ID
2. GET /boards/{id} → verify returns initial state
3. GET /boards/{id}/states/next → verify next generation (calls GetStatesAhead(1))
4. GET /boards/{id}/states?steps=5 → verify 5 steps ahead
5. GET /boards/{id}/states/final → verify returns stable state or error
6. DELETE /boards/{id} → verify board removed
7. POST /boards twice with same data → verify different IDs

### Edge Cases
1. Non-existent board ID → 404
2. Invalid board dimensions → 400
3. Negative steps parameter → 400
4. Board with no final state (max iterations) → 422
5. Empty board → returns empty state

---

## Notes & Assumptions
- LiteDB database file persists in bin/Debug or application directory (configured in Program.cs)
- All time-sensitive operations (GetStatesAhead, GetFinalState) compute in-memory; no caching of intermediate states
- API uses camelCase for JSON (LiteDB default or Newtonsoft Json configuration)
- Board coordinates are 0-indexed (0,0 is top-left)
- Out-of-bounds cells are treated as dead (no wrapping)
