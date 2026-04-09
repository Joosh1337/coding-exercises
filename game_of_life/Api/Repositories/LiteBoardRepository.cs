namespace api.Repositories;

using api.Models;
using LiteDB;

/// <summary>
/// LiteDB implementation of the board repository.
/// Handles all persistence operations for Game of Life boards.
/// </summary>
public class LiteBoardRepository : IBoardRepository {
    private readonly LiteDatabase _database;
    private const string CollectionName = "boards";

    /// <summary>
    /// Initializes a new instance of the LiteBoardRepository.
    /// </summary>
    /// <param name="database">The LiteDB database instance.</param>
    public LiteBoardRepository(LiteDatabase database) {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        
        // Ensure the collection exists and ID is indexed
        var collection = _database.GetCollection<Board>(CollectionName);
        collection.EnsureIndex(x => x.Id);
    }

    /// <summary>
    /// Creates and persists a new board.
    /// </summary>
    /// <param name="board">The board entity to save.</param>
    /// <returns>The saved board with ID populated.</returns>
    public async Task<Board> CreateBoard(Board board) {
        ArgumentNullException.ThrowIfNull(board);

        // Ensure the board has an ID
        if (board.Id == Guid.Empty)
            board.Id = Guid.NewGuid();

        return await Task.Run(() => {
            var collection = _database.GetCollection<Board>(CollectionName);
            collection.Insert(board);
            return board;
        });
    }

    /// <summary>
    /// Retrieves a board by its unique identifier.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>The board if found; null otherwise.</returns>
    public async Task<Board?> GetBoardById(Guid id) {
        return await Task.Run(() => {
            var collection = _database.GetCollection<Board>(CollectionName);
            return collection.FindById(id);
        });
    }

    /// <summary>
    /// Deletes a board from persistence.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>True if the board was deleted; false if it was not found.</returns>
    public async Task<bool> DeleteBoard(Guid id) {
        return await Task.Run(() => {
            var collection = _database.GetCollection<Board>(CollectionName);
            return collection.DeleteMany(b => b.Id == id) > 0;
        });
    }

    /// <summary>
    /// Checks whether a board exists in persistence.
    /// </summary>
    /// <param name="id">The board's Guid.</param>
    /// <returns>True if the board exists; false otherwise.</returns>
    public async Task<bool> BoardExists(Guid id) {
        return await Task.Run(() => {
            var collection = _database.GetCollection<Board>(CollectionName);
            return collection.Exists(b => b.Id == id);
        });
    }
}
