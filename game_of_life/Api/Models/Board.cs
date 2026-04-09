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
    /// Live cells stored as a list of coordinates (serialization-friendly for LiteDB).
    /// Use BoardState for performance-critical operations that need fast lookups.
    /// </summary>
    public List<CellCoordinate> LiveCells { get; set; } = new();

    /// <summary>
    /// Default constructor for deserialization from LiteDB.
    /// </summary>
    public Board() { }

    /// <summary>
    /// Constructor to create a Board from a 2D array of initial cell states.
    /// Non-zero values in the array represent live cells.
    /// </summary>
    /// <param name="width">Board width (must be > 0)</param>
    /// <param name="height">Board height (must be > 0)</param>
    /// <param name="initialCells">2D array with same dimensions as width x height</param>
    /// <exception cref="ArgumentException">If dimensions are invalid or array dimensions don't match</exception>
    public Board(int width, int height, int[,] initialCells) {
        // Validate dimensions
        if (width <= 0)
            throw new ArgumentException($"Board width must be greater than 0, but got {width}.", nameof(width));
        if (height <= 0)
            throw new ArgumentException($"Board height must be greater than 0, but got {height}.", nameof(height));

        // Validate initialCells array
        if (initialCells == null)
            throw new ArgumentException("Initial cells array cannot be null.", nameof(initialCells));

        // Validate initialCells dimensions match board dimensions
        int cellsHeight = initialCells.GetLength(0);
        int cellsWidth = initialCells.GetLength(1);

        if (cellsHeight != height || cellsWidth != width)
            throw new ArgumentException(
                $"Initial cells array dimensions [{cellsWidth}x{cellsHeight}] must match board dimensions [{width}x{height}].",
                nameof(initialCells));

        // Extract live cells from 2D array
        var liveCells = new List<CellCoordinate>();

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (initialCells[y, x] != 0) {
                    liveCells.Add(new CellCoordinate(x, y));
                }
            }
        }

        // Initialize board
        Id = Guid.NewGuid();
        Width = width;
        Height = height;
        LiveCells = liveCells;
    }
}
