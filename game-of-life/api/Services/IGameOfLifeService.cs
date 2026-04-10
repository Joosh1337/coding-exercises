using api.Models;

namespace api.Services;

/// <summary>
/// Defines the contract for Game of Life business logic operations.
/// </summary>
public interface IGameOfLifeService {
    /// <summary>
    /// Creates a new board with the specified dimensions and initial live cells.
    /// </summary>
    /// <param name="width">Board width (must be > 0)</param>
    /// <param name="height">Board height (must be > 0)</param>
    /// <param name="initialCells">2D array where non-zero values indicate live cells</param>
    /// <returns>The ID of the created board</returns>
    /// <exception cref="InvalidBoardStateException">If dimensions are invalid or cells are out of bounds</exception>
    Guid CreateBoard(int width, int height, int[][] initialCells);

    /// <summary>
    /// Retrieves all initial board states at generation 0.
    /// </summary>
    /// <returns>A list of all board states at generation 0</returns>
    List<BoardState> GetAllBoardStates();

    /// <summary>
    /// Retrieves the current board state at generation 0.
    /// </summary>
    /// <param name="boardId">The ID of the board</param>
    /// <returns>The board state at generation 0</returns>
    /// <exception cref="BoardNotFoundException">If the board is not found</exception>
    BoardState GetBoardState(Guid boardId);

    /// <summary>
    /// Computes the board state after a specified number of steps.
    /// </summary>
    /// <param name="boardId">The ID of the board</param>
    /// <param name="steps">Number of generations to compute (must be > 0)</param>
    /// <returns>The board state after N generations</returns>
    /// <exception cref="BoardNotFoundException">If the board is not found</exception>
    /// <exception cref="InvalidStepsException">If steps is not greater than 0</exception>
    BoardState GetStatesAhead(Guid boardId, int steps);

    /// <summary>
    /// Computes and returns the final stable state of the board.
    /// Iterates up to the configured maximum iterations to find a stable state or cycle.
    /// </summary>
    /// <param name="boardId">The ID of the board</param>
    /// <returns>The final stable board state</returns>
    /// <exception cref="BoardNotFoundException">If the board is not found</exception>
    /// <exception cref="NoFinalStateException">If no final state is found within max iterations</exception>
    BoardState GetFinalState(Guid boardId);

    /// <summary>
    /// Deletes a board from persistence.
    /// </summary>
    /// <param name="boardId">The ID of the board to delete</param>
    /// <returns>true if the board was deleted, false if not found</returns>
    bool DeleteBoard(Guid boardId);
}
