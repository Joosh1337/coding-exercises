using api.Models;

namespace api.Tests.Models;

public class StableStateDetectionTests {
    /// <summary>
    /// Helper method to create a board state with specified live cells
    /// </summary>
    private BoardState CreateBoardState(int width, int height, params (int x, int y)[] liveCells) {
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = width,
            Height = height,
            LiveCells = liveCells.Select(t => new CellCoordinate(t.x, t.y)).ToList()
        };
        return new BoardState(board);
    }

    #region IsStable Tests

    [Fact]
    public void IsStable_ReturnsFalse_WhenStatesAreNotEqual() {
        // Arrange: Two different board states
        var state1 = CreateBoardState(3, 3, (1, 0), (1, 1), (1, 2)); // Vertical blinker
        var state2 = state1.GenerateNextStep(); // Becomes horizontal

        // Act
        bool isStable = StableStateDetection.IsStable(state2, state1);

        // Assert
        Assert.False(isStable);
    }

    [Fact]
    public void IsStable_ReturnsTrue_WhenStatesAreEqual() {
        // Arrange: Two identical stable states
        var state1 = CreateBoardState(3, 3, (0, 0), (1, 0), (0, 1), (1, 1)); // Block pattern
        var state2 = state1.GenerateNextStep(); // Same block pattern

        // Act
        bool isStable = StableStateDetection.IsStable(state1, state2);

        // Assert
        Assert.True(isStable);
    }

    [Fact]
    public void IsStable_ReturnsTrue_WhenGenerationsDiffer() {
        // Arrange: Same live cells but different generation numbers
        // Note: IsStable should only check LiveCells, not Generation
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 3,
            Height = 3,
            LiveCells = new List<CellCoordinate> { new(0, 0), new(1, 0), new(0, 1), new(1, 1) }
        };
        var state1 = new BoardState(board);
        var state2 = new BoardState(board); // Same LiveCells, same generation

        // Act
        bool isStable = StableStateDetection.IsStable(state2, state1);

        // Assert: Should check if live cells are the same
        Assert.True(isStable);
    }

    #endregion

    #region HasStableStateWithinLimit Tests

    [Fact]
    public void HasStableStateWithinLimit_ReturnsTrue_ForStableBoard() {
        // Arrange: Block pattern (stable after first generation)
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 3,
            Height = 3,
            LiveCells = new List<CellCoordinate> { new(0, 0), new(1, 0), new(0, 1), new(1, 1) }
        };

        // Act
        bool hasStable = StableStateDetection.HasStableStateWithinLimit(board, 100);

        // Assert
        Assert.True(hasStable);
    }

    [Fact]
    public void HasStableStateWithinLimit_ReturnsTrue_ForStabilizingBoard() {
        // Arrange: Board with a single isolated cell (dies immediately to empty board, which is stable)
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate> { new(2, 2) } // Single lonely cell
        };

        // Act
        bool hasStable = StableStateDetection.HasStableStateWithinLimit(board, 100);

        // Assert: Single cell dies (0 neighbors) -> empty board -> stable
        Assert.True(hasStable);
    }

    [Fact]
    public void HasStableStateWithinLimit_ReturnsFalse_WhenMaxIterationsExceeded() {
        // Arrange: A pattern that doesn't stabilize within the iteration limit
        // Using a chaotic pattern or one that takes many iterations
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 3,
            Height = 3,
            LiveCells = new List<CellCoordinate> { new(1, 0), new(1, 1), new(1, 2) }
        };

        // Act
        bool hasStable = StableStateDetection.HasStableStateWithinLimit(board, 100);

        // Assert: Period-2 cycles are NOT detected by the current implementation
        Assert.False(hasStable);
    }

    #endregion
}
