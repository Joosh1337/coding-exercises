using Api.Exceptions;
using api.Models;
using api.Repositories;

namespace Api.Services;

/// <summary>
/// Implements the Game of Life business logic service.
/// Handles board creation, state computation, and persistence coordination.
/// </summary>
public class GameOfLifeService : IGameOfLifeService {
    private readonly IBoardRepository _boardRepository;
    private readonly IConfiguration _configuration;
    private readonly int _maxIterationsForFinalState;

    public GameOfLifeService(IBoardRepository boardRepository, IConfiguration configuration) {
        _boardRepository = boardRepository;
        _configuration = configuration;

        // Read max iterations from configuration, default to 9999
        _maxIterationsForFinalState = _configuration.GetValue("GameOfLife:MaxIterationsForFinalState", 9999);
    }

    /// <summary>
    /// Helper method to retrieve a board from the repository or throw if not found.
    /// </summary>
    private async Task<Board> GetBoardOrThrow(Guid boardId) {
        var board = await _boardRepository.GetBoardById(boardId);
        if (board == null)
            throw new BoardNotFoundException(boardId);
        return board;
    }

    /// <summary>
    /// Creates a new board with the specified dimensions and initial live cells.
    /// </summary>
    public async Task<Guid> CreateBoard(int width, int height, int[][] initialCells) {
        try {
            // Convert array format to CellCoordinate list
            var cellCoordinates = ConvertArrayToCellCoordinates(width, height, initialCells);
            
            // Create board with validation
            var board = new Board(width, height, cellCoordinates);
            
            // Persist the board
            var createdBoard = await _boardRepository.CreateBoard(board);
            return createdBoard.Id;
        } catch (ArgumentException ex) {
            // Convert ArgumentException to InvalidBoardStateException for consistent service contract
            throw new InvalidBoardStateException(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the current board state at generation 0.
    /// </summary>
    public async Task<BoardState> GetBoardState(Guid boardId) {
        var board = await GetBoardOrThrow(boardId);
        return new BoardState(board);
    }

    /// <summary>
    /// Computes the board state after a specified number of steps.
    /// </summary>
    public async Task<BoardState> GetStatesAhead(Guid boardId, int steps) {
        if (steps <= 0)
            throw new InvalidStepsException(steps);

        var board = await GetBoardOrThrow(boardId);
        var currentState = new BoardState(board);

        for (int i = 0; i < steps; i++) {
            currentState = currentState.GenerateNextStep();
        }

        return currentState;
    }

    /// <summary>
    /// Computes and returns the final stable state of the board.
    /// Iterates up to the configured maximum iterations to find a stable state.
    /// </summary>
    public async Task<BoardState> GetFinalState(Guid boardId) {
        var board = await GetBoardOrThrow(boardId);
        var currentState = new BoardState(board);
        var previousState = currentState;

        for (int i = 0; i < _maxIterationsForFinalState; i++) {
            var nextState = currentState.GenerateNextStep();

            // Check if we've reached a stable state (no change from previous generation)
            if (CycleDetection.IsStable(nextState, currentState)) {
                return nextState;
            }

            previousState = currentState;
            currentState = nextState;
        }

        // If we exit the loop without finding a stable state, throw an exception
        throw new NoFinalStateException(_maxIterationsForFinalState);
    }

    /// <summary>
    /// Deletes a board from persistence.
    /// </summary>
    public async Task<bool> DeleteBoard(Guid boardId) {
        return await _boardRepository.DeleteBoard(boardId);
    }

    /// <summary>
    /// Converts a jagged array of cell coordinates to a list of CellCoordinate objects.
    /// </summary>
    private List<CellCoordinate> ConvertArrayToCellCoordinates(int width, int height, int[][] cells) {
        var liveCells = new List<CellCoordinate>();

        if (cells.Length != height)
            throw new ArgumentException($"Grid must have {height} rows, but got {cells.Length}.");

        for (int y = 0; y < height; y++) {
            if (cells[y].Length != width)
                throw new ArgumentException($"Row {y} must have {width} columns, but got {cells[y].Length}.");

            for (int x = 0; x < width; x++) {
                if (cells[y][x] == 1) {
                    liveCells.Add(new CellCoordinate(x, y));
                }
            }
        }

        return liveCells;
    }
}
