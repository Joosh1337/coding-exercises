namespace api.Models;

/// <summary>
/// Represents a computed state of a Game of Life board at a specific generation.
/// BoardState is computed at runtime and not persisted to the database.
/// </summary>
public class BoardState {
    /// <summary>
    /// The generation number (0 for initial state).
    /// </summary>
    public int Generation { get; set; }

    /// <summary>
    /// Width of the board (number of columns).
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of the board (number of rows).
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Set of coordinates representing live cells at this generation.
    /// Each tuple is (x, y) where x is column, y is row.
    /// </summary>
    public HashSet<(int x, int y)> LiveCells { get; set; } = new();

    /// <summary>
    /// Creates a BoardState from a persisted Board entity at generation 0.
    /// </summary>
    public BoardState(Board board) {
        Generation = 0;
        Width = board.Width;
        Height = board.Height;
        LiveCells = new HashSet<(int x, int y)>(board.LiveCells);
    }

    /// <summary>
    /// Default constructor for deserialization or manual creation.
    /// </summary>
    public BoardState() { }

    /// <summary>
    /// Generates the next generation state based on Conway's Game of Life rules:
    /// 1. Any live cell with fewer than two live neighbors dies (underpopulation).
    /// 2. Any live cell with two or three live neighbors lives on.
    /// 3. Any live cell with more than three live neighbors dies (overpopulation).
    /// 4. Any dead cell with exactly three live neighbors becomes alive (reproduction).
    /// </summary>
    public BoardState GenerateNextStep() {
        var neighborCounts = new Dictionary<(int x, int y), int>();
        var nextLiveCells = new HashSet<(int x, int y)>();

        // Count neighbors for each live cell's neighbors
        foreach (var liveCell in LiveCells) {
            // Loop through all 8 surrounding neighbors
            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    // Skip the cell itself
                    if (dx == 0 && dy == 0)
                        continue;

                    int neighborX = liveCell.x + dx;
                    int neighborY = liveCell.y + dy;

                    // Skip cells outside the board
                    if (neighborX < 0 || neighborX >= Width || neighborY < 0 || neighborY >= Height)
                        continue;

                    var neighbor = (neighborX, neighborY);
                    neighborCounts.TryGetValue(neighbor, out int count);
                    neighborCounts[neighbor] = count + 1;
                }
            }
        }

        // Apply the Game of Life rules
        foreach (var (cell, count) in neighborCounts) {
            bool isAlive = LiveCells.Contains(cell);

            if (!isAlive && count == 3) {
                // Dead cell with exactly 3 neighbors becomes alive (reproduction)
                nextLiveCells.Add(cell);
            } else if (isAlive && (count == 2 || count == 3)) {
                // Live cell with 2 or 3 neighbors survives
                nextLiveCells.Add(cell);
            }
            // All other cases: cell dies or stays dead
        }

        return new BoardState {
            Generation = Generation + 1,
            Width = Width,
            Height = Height,
            LiveCells = nextLiveCells
        };
    }

    /// <summary>
    /// Generates a 2D array representation of the board where 1 = alive, 0 = dead.
    /// Array is [y][x] indexed (rows first, then columns).
    /// </summary>
    public int[][] GenerateBoardArray() {
        var board = new int[Height][];

        for (int y = 0; y < Height; y++) {
            board[y] = new int[Width];
            for (int x = 0; x < Width; x++) {
                board[y][x] = LiveCells.Contains((x, y)) ? 1 : 0;
            }
        }

        return board;
    }
}
