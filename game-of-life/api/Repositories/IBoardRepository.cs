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
    Board CreateBoard(Board board);

    /// <summary>
    /// Retrieves all boards within the paging parameters.
    /// </summary>
    /// <returns>All boards within the paging parameters</returns>
    List<Board> GetBoards(int page, int pageSize);

    /// <summary>
    /// Retrieves a board by its unique identifier.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>The board if found; null otherwise.</returns>
    Board? GetBoardById(Guid id);

    /// <summary>
    /// Deletes a board from persistence.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>True if the board was deleted; false if it was not found.</returns>
    bool DeleteBoard(Guid id);

    /// <summary>
    /// Updates an existing board in persistence.
    /// </summary>
    /// <param name="board">The board entity with updated values.</param>
    /// <returns>True if the board was updated; false if it was not found.</returns>
    bool UpdateBoard(Board board);

    /// <summary>
    /// Checks whether a board exists in persistence.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>True if the board exists; false otherwise.</returns>
    bool BoardExists(Guid id);
}
