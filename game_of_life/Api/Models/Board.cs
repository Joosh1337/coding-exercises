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
}
