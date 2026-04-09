namespace api.Models;

/// <summary>
/// Represents a persisted Game of Life board entity.
/// This is the data model stored in the database.
/// </summary>
public class Board {
    /// <summary>
    /// Unique identifier for the board.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Width of the board (number of columns).
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of the board (number of rows).
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Set of coordinates representing live cells.
    /// Each tuple is (x, y) where x is column, y is row.
    /// </summary>
    public HashSet<(int x, int y)> LiveCells { get; set; } = new();
}
