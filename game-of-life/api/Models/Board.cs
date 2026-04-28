namespace api.Models;

/// <summary>
/// Represents a single cell coordinate for serialization to LiteDB.
/// </summary>
public class CellCoordinate {
    public int X { get; set; }
    public int Y { get; set; }

    public CellCoordinate() { }

    public CellCoordinate(int x, int y) {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj) {
        if (obj is CellCoordinate other)
            return X == other.X && Y == other.Y;
        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine(X, Y);
    }
}

/// <summary>
/// Represents a persisted Game of Life board entity.
/// This is the data model stored in the database.
/// Board is a simple data container; performance-critical operations happen in BoardState.
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
    /// Optional user-defined name for the board.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Live cells stored as a list of coordinates (serialization-friendly for LiteDB).
    /// Use BoardState for performance-critical operations that need fast lookups.
    /// </summary>
    public List<CellCoordinate> LiveCells { get; set; } = new();

    /// <summary>
    /// Default constructor for deserialization from LiteDB.
    /// </summary>
    public Board() { }

    /// <summary>
    /// Constructor to create a Board from a list of live cell coordinates.
    /// </summary>
    /// <param name="width">Board width (must be > 0)</param>
    /// <param name="height">Board height (must be > 0)</param>
    /// <param name="liveCells">List of live cell coordinates</param>
    /// <exception cref="ArgumentException">If dimensions are invalid or cells are outside bounds</exception>
    public Board(int width, int height, List<CellCoordinate> liveCells) {
        // Validate dimensions
        if (width <= 0)
            throw new ArgumentException($"Board width must be greater than 0, but got {width}.", nameof(width));
        if (height <= 0)
            throw new ArgumentException($"Board height must be greater than 0, but got {height}.", nameof(height));

        // Validate liveCells
        if (liveCells == null)
            throw new ArgumentException("Live cells list cannot be null.", nameof(liveCells));

        // Validate all cells are within board bounds
        foreach (var cell in liveCells) {
            if (cell.X < 0 || cell.X >= width || cell.Y < 0 || cell.Y >= height)
                throw new ArgumentException(
                    $"Cell coordinate [{cell.X}, {cell.Y}] is outside board bounds [{width}x{height}].",
                    nameof(liveCells));
        }

        // Initialize board
        Id = Guid.NewGuid();
        Width = width;
        Height = height;
        LiveCells = new List<CellCoordinate>(liveCells);
    }
}
