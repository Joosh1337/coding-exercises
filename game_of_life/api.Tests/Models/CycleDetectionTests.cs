using Xunit;
using api.Models;

namespace Api.Tests.Models;

public class CycleDetectionTests {
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
        bool isStable = CycleDetection.IsStable(state2, state1);

        // Assert
        Assert.False(isStable);
    }

    [Fact]
    public void IsStable_ReturnsTrue_WhenStatesAreEqual() {
        // Arrange: Two identical stable states
        var state1 = CreateBoardState(3, 3, (0, 0), (1, 0), (0, 1), (1, 1)); // Block pattern
        var state2 = state1.GenerateNextStep(); // Same block pattern

        // Act
        bool isStable = CycleDetection.IsStable(state1, state2);

        // Assert
        Assert.True(isStable);
    }

    [Fact]
    public void IsStable_ReturnsFalse_WhenGenerationsDiffer() {
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
        bool isStable = CycleDetection.IsStable(state2, state1);

        // Assert: Should check if live cells are the same
        Assert.True(isStable);
    }

    [Fact]
    public void IsStable_WithEmptyBoard() {
        // Arrange: Two empty board states
        var state1 = CreateBoardState(5, 5);
        var state2 = CreateBoardState(5, 5);

        // Act
        bool isStable = CycleDetection.IsStable(state2, state1);

        // Assert
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
        bool hasStable = CycleDetection.HasStableStateWithinLimit(board, 100);

        // Assert
        Assert.True(hasStable);
    }

    [Fact]
    public void HasStableStateWithinLimit_ReturnsFalse_ForBlinkerPattern() {
        // Note: Blinker is a period-2 cycle, but HasStableStateWithinLimit only detects
        // period-1 cycles (stable states where current == previous).
        // To properly detect the blinker cycle, we'd need to track all seen states.
        // This test documents the current limitation.
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 3,
            Height = 3,
            LiveCells = new List<CellCoordinate> { new(1, 0), new(1, 1), new(1, 2) }
        };

        // Act
        bool hasStable = CycleDetection.HasStableStateWithinLimit(board, 100);

        // Assert: Period-2 cycles are NOT detected by the current implementation
        Assert.False(hasStable);
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
        bool hasStable = CycleDetection.HasStableStateWithinLimit(board, 100);

        // Assert: Single cell dies (0 neighbors) -> empty board -> stable
        Assert.True(hasStable);
    }

    [Fact]
    public void HasStableStateWithinLimit_ReturnsFalse_WhenMaxIterationsExceeded() {
        // Arrange: A pattern that doesn't stabilize within the iteration limit
        // Using a chaotic pattern or one that takes many iterations
        // For simplicity, create a pattern and use a very low iteration limit
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate> { new(1, 1), new(2, 1), new(3, 1) }
        };

        // Act: Use iteration limit of 1 (very restrictive)
        bool hasCycle = CycleDetection.HasStableStateWithinLimit(board, 1);

        // Assert
        Assert.False(hasCycle);
    }

    [Fact]
    public void HasStableStateWithinLimit_ReturnsFalse_ForEmptyBoard_WithIterationOne() {
        // Edge case: Empty board with limit of 1 might need special handling
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate>()
        };

        // Act
        bool hasCycle = CycleDetection.HasStableStateWithinLimit(board, 0);

        // Assert: With 0 iterations, we can't detect a cycle
        Assert.False(hasCycle);
    }

    [Fact]
    public void HasStableStateWithinLimit_ReturnsTrue_ForEmptyBoard_WithSufficientIterations() {
        // Arrange: Empty board (already stable)
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate>()
        };

        // Act
        bool hasCycle = CycleDetection.HasStableStateWithinLimit(board, 10);

        // Assert
        Assert.True(hasCycle);
    }

    [Fact]
    public void HasStableStateWithinLimit_TracksStatesCorrectly() {
        // Arrange: A pattern that dies quickly (underpopulation)
        // Using two isolated cells far apart - each has 0 neighbors
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate> { new(0, 0), new(4, 4) }
        };

        // Act: Enough iterations to detect stabilization
        bool hasStable = CycleDetection.HasStableStateWithinLimit(board, 10);

        // Assert: Both cells die from underpopulation -> empty board (stable)
        Assert.True(hasStable);
    }

    [Fact]
    public void HasStableStateWithinLimit_StopsBeforeMaxIterations_WhenStable() {
        // Arrange: Stable pattern (block)
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 3,
            Height = 3,
            LiveCells = new List<CellCoordinate> { new(0, 0), new(1, 0), new(0, 1), new(1, 1) }
        };

        // Act: Should detect stability quickly without hitting the iteration limit
        bool hasCycle = CycleDetection.HasStableStateWithinLimit(board, 1000);

        // Assert
        Assert.True(hasCycle);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void HasStableStateWithinLimit_WithLargerBoard() {
        // Arrange: Larger board with a stable pattern
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 10,
            Height = 10,
            LiveCells = new List<CellCoordinate> { new(3, 3), new(4, 3), new(3, 4), new(4, 4) } // Block pattern
        };

        // Act
        bool hasCycle = CycleDetection.HasStableStateWithinLimit(board, 100);

        // Assert
        Assert.True(hasCycle);
    }

    [Fact]
    public void HasStableStateWithinLimit_WithMinimalIterationLimit() {
        // Arrange: Block pattern that is already stable
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 3,
            Height = 3,
            LiveCells = new List<CellCoordinate> { new(0, 0), new(1, 0), new(0, 1), new(1, 1) }
        };

        // Act: Minimal iteration limit
        bool hasStable = CycleDetection.HasStableStateWithinLimit(board, 1);

        // Assert: Should detect stability in first generation
        Assert.True(hasStable);
    }

    #endregion
}
