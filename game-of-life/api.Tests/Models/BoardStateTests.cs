using api.Models;

namespace api.Tests.Models;

public class BoardStateTests {
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

    /// <summary>
    /// Helper to convert array of coordinates to BoardState
    /// </summary>
    private BoardState CreateBoardFromPattern(int width, int height, int[,] pattern) {
        var liveCells = new List<(int, int)>();
        for (int y = 0; y < pattern.GetLength(0); y++) {
            for (int x = 0; x < pattern.GetLength(1); x++) {
                if (pattern[y, x] == 1) {
                    liveCells.Add((x, y));
                }
            }
        }
        return CreateBoardState(width, height, liveCells.ToArray());
    }

    #region Rule 1: Underpopulation
    
    [Fact]
    public void SingleLiveCell_Dies_WithZeroNeighbors() {
        // Arrange: Single live cell at (1,1) on 3x3 board
        var state = CreateBoardState(3, 3, (1, 1));

        // Act
        var nextState = state.GenerateNextStep();

        // Assert: Cell should die
        Assert.Empty(nextState.LiveCells);
    }

    [Fact]
    public void TwoAdjacentLiveCells_BothDie_WithOneNeighborEach() {
        // Arrange: Two horizontally adjacent cells at (1,1) and (2,1)
        var state = CreateBoardState(3, 3, (1, 1), (2, 1));

        // Act
        var nextState = state.GenerateNextStep();

        // Assert: Both cells should die (each has only 1 neighbor)
        Assert.Empty(nextState.LiveCells);
    }

    #endregion

    #region Rule 2: Survival

    [Fact]
    public void LiveCell_Survives_WithTwoNeighbors() {
        // Arrange: Create a configuration where a cell has exactly 2 neighbors
        // Pattern: 
        //   0 1 0
        //   1 1 0  <- center cell (1,1) has 2 neighbors at (0,1) and (1,0)
        //   0 0 0
        var state = CreateBoardState(3, 3, (0, 1), (1, 0), (1, 1));

        // Act
        var nextState = state.GenerateNextStep();

        // Assert: Center cell should survive
        Assert.Contains((1, 1), nextState.LiveCells);
    }

    [Fact]
    public void LiveCell_Survives_WithThreeNeighbors() {
        // Arrange: Create a configuration where a cell has exactly 3 neighbors
        // Pattern: 
        //   1 1 0
        //   1 1 0  <- center cells have 3 neighbors each
        //   0 0 0
        var state = CreateBoardState(3, 3, (0, 0), (1, 0), (0, 1), (1, 1));

        // Act
        var nextState = state.GenerateNextStep();

        // Assert: All 4 cells should survive (each has 3 neighbors)
        Assert.Contains((0, 0), nextState.LiveCells);
        Assert.Contains((1, 0), nextState.LiveCells);
        Assert.Contains((0, 1), nextState.LiveCells);
        Assert.Contains((1, 1), nextState.LiveCells);
    }

    #endregion

    #region Rule 3: Overpopulation

    [Fact]
    public void LiveCell_Dies_WithFourNeighbors() {
        // Arrange: Cell surrounded by 4 or more neighbors
        // Pattern:
        //   1 1 1
        //   1 1 1
        //   0 0 0
        // Center cell (1,1) has 5 neighbors: all except (2,0)
        var state = CreateBoardState(3, 3, (0, 0), (1, 0), (2, 0), (0, 1), (1, 1), (2, 1));

        // Act
        var nextState = state.GenerateNextStep();

        // Assert: Some corner/edge cells should die due to overpopulation
        // Only cells with exactly 3 neighbors survive
        Assert.DoesNotContain((1, 1), nextState.LiveCells); // Overpopulated
    }

    #endregion

    #region Rule 4: Reproduction

    [Fact]
    public void DeadCell_BecomeAlive_WithExactlyThreeNeighbors() {
        // Arrange: Dead cell surrounded by 3 live neighbors (vertical line below becomes horizontal)
        // Pattern:
        //   0 0 0
        //   1 0 1  <- cell (1,1) is dead but has 2 neighbors horizontally
        //   1 0 0  <- with horizontal neighbor = 3 total? No, testing properly:
        // Better pattern for reproduction:
        //   1 1 1
        //   0 0 0
        //   0 0 0
        // Dead cells at (0,1), (1,1), (2,1) each have 2-3 neighbors from row above
        // Actually, let me be precise:
        //   1 0 0
        //   1 0 1  <- dead cell at (1,1) has neighbors at (0,0), (0,1), (2,1) = 3 neighbors
        //   0 0 0
        var state = CreateBoardState(3, 3, (0, 0), (0, 1), (2, 1));

        // Act
        var nextState = state.GenerateNextStep();

        // Assert: Dead cell (1,1) should become alive
        Assert.Contains((1, 1), nextState.LiveCells);
    }

    #endregion

    #region Edge Cases: Dead Board

    [Fact]
    public void DeadBoard_RemainsDead() {
        // Arrange: Completely dead board
        var state = CreateBoardState(5, 5);

        // Act
        var nextState = state.GenerateNextStep();

        // Assert: Should remain empty
        Assert.Empty(nextState.LiveCells);
    }

    #endregion

    #region Oscillators: Blinker Pattern

    [Fact]
    public void BlinkerPattern_OscillatesVerticalToHorizontal() {
        // Arrange: Start with vertical blinker
        //   0 1 0
        //   0 1 0
        //   0 1 0
        var state = CreateBoardState(3, 3, (1, 0), (1, 1), (1, 2));

        // Act: Generate next state
        var nextState = state.GenerateNextStep();

        // Assert: Should become horizontal
        //   0 0 0
        //   1 1 1
        //   0 0 0
        Assert.Contains((0, 1), nextState.LiveCells);
        Assert.Contains((1, 1), nextState.LiveCells);
        Assert.Contains((2, 1), nextState.LiveCells);
        Assert.DoesNotContain((1, 0), nextState.LiveCells);
        Assert.DoesNotContain((1, 2), nextState.LiveCells);
    }

    [Fact]
    public void BlinkerPattern_OscillatesHorizontalToVertical() {
        // Arrange: Start with horizontal blinker
        //   0 0 0
        //   1 1 1
        //   0 0 0
        var state = CreateBoardState(3, 3, (0, 1), (1, 1), (2, 1));

        // Act: Generate next state
        var nextState = state.GenerateNextStep();

        // Assert: Should become vertical
        //   0 1 0
        //   0 1 0
        //   0 1 0
        Assert.Contains((1, 0), nextState.LiveCells);
        Assert.Contains((1, 1), nextState.LiveCells);
        Assert.Contains((1, 2), nextState.LiveCells);
        Assert.DoesNotContain((0, 1), nextState.LiveCells);
        Assert.DoesNotContain((2, 1), nextState.LiveCells);
    }

    #endregion

    #region Boundary Handling

    [Fact]
    public void LiveCell_AtEdge_DoesNotCauseOutOfBoundsError() {
        // Arrange: Live cells at all four corners
        var state = CreateBoardState(3, 3, (0, 0), (2, 0), (0, 2), (2, 2));

        // Act & Assert: Should not throw
        var nextState = state.GenerateNextStep();
        Assert.NotNull(nextState); // All corner cells die (2 neighbors each)
    }

    [Fact]
    public void OutOfBoundsCells_TreatedAsDead() {
        // Arrange: Live cell at top-left corner (0,0)
        var state = CreateBoardState(5, 5, (0, 0));

        // Act: Generate next state
        var nextState = state.GenerateNextStep();

        // Assert: Cell dies (neighbors outside grid treated as dead)
        Assert.Empty(nextState.LiveCells);
    }

    #endregion

    #region Generation Tracking

    [Fact]
    public void MultipleGenerations_IncrementCorrectly() {
        // Arrange
        var state = CreateBoardState(3, 3, (1, 0), (1, 1), (1, 2));

        // Act & Assert
        Assert.Equal(0, state.Generation);
        var gen1 = state.GenerateNextStep();
        Assert.Equal(1, gen1.Generation);
        var gen2 = gen1.GenerateNextStep();
        Assert.Equal(2, gen2.Generation);
    }

    #endregion

    #region Board Dimensions

    [Fact]
    public void BoardState_MaintainsDimensions() {
        // Arrange
        var state = CreateBoardState(10, 15, (5, 7));

        // Act
        var nextState = state.GenerateNextStep();

        // Assert
        Assert.Equal(10, nextState.Width);
        Assert.Equal(15, nextState.Height);
    }

    #endregion
}
