namespace api.Repositories;

using api.Models;

/// <summary>
/// Repository interface for Board persistence operations.
/// Defines contract for storing and retrieving Game of Life boards.
/// </summary>
public interface IBoardRepository {
    /// <summary>
    /// Creates and persists a new board.
    /// </summary>
    /// <param name="board">The board entity to save.</param>
    /// <returns>The saved board with ID populated by the database.</returns>
    Task<Board> CreateBoard(Board board);

    /// <summary>
    /// Retrieves a board by its unique identifier.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>The board if found; null otherwise.</returns>
    Task<Board?> GetBoardById(Guid id);

    /// <summary>
    /// Deletes a board from persistence.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>True if the board was deleted; false if it was not found.</returns>
    Task<bool> DeleteBoard(Guid id);

    /// <summary>
    /// Checks whether a board exists in persistence.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>True if the board exists; false otherwise.</returns>
    Task<bool> BoardExists(Guid id);
}
