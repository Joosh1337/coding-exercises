using api.Models;
using api.Repositories;

using LiteDB;

namespace api.Tests.Repositories;

public class LiteBoardRepositoryTests : IDisposable {
    private LiteDatabase? _database;
    private LiteBoardRepository? _repository;
    private string _testDbPath = null!;

    public LiteBoardRepositoryTests() {
        // Setup: Create a temporary test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_game_of_life_{Guid.NewGuid()}.db");
        _database = new LiteDatabase(_testDbPath);
        _repository = new LiteBoardRepository(_database);
    }

    public void Dispose() {
        // Cleanup: Dispose database and delete test file
        _database?.Dispose();

        if (File.Exists(_testDbPath)) {
            File.Delete(_testDbPath);
        }
    }

    private Board CreateTestBoard(int width = 5, int height = 5) {
        return new Board {
            Id = Guid.NewGuid(),
            Width = width,
            Height = height,
            LiveCells = new List<CellCoordinate> { new(1, 1), new(2, 1), new(3, 1) }
        };
    }

    #region Create Tests

    [Fact]
    public async Task CreateBoard_SavesBoardSuccessfully() {
        // Arrange
        var board = CreateTestBoard();
        var originalId = board.Id;

        // Act
        var savedBoard = _repository!.CreateBoard(board);

        // Assert
        Assert.NotNull(savedBoard);
        Assert.Equal(originalId, savedBoard.Id);
        Assert.Equal(board.Width, savedBoard.Width);
        Assert.Equal(board.Height, savedBoard.Height);
        Assert.Equal(board.LiveCells.Count, savedBoard.LiveCells.Count);
    }

    [Fact]
    public async Task CreateBoard_PreservesLiveCells() {
        // Arrange
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate> {
                new(0, 0), new(1, 1), new(2, 2), new(3, 3), new(4, 4)
            }
        };

        // Act
        var savedBoard = _repository!.CreateBoard(board);

        // Assert
        Assert.Equal(5, savedBoard.LiveCells.Count);
        foreach (var cell in board.LiveCells) {
            Assert.Contains(cell, savedBoard.LiveCells);
        }
    }

    [Fact]
    public async Task CreateBoard_WithEmptyLiveCells() {
        // Arrange
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 10,
            Height = 10,
            LiveCells = new List<CellCoordinate>()
        };

        // Act
        var savedBoard = _repository!.CreateBoard(board);

        // Assert
        Assert.Empty(savedBoard.LiveCells);
    }

    [Fact]
    public async Task CreateMultipleBoards_WithSameData_HaveDifferentIds() {
        // Arrange
        var board1 = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate> { new(1, 1), new(2, 2), new(3, 3) }
        };
        
        var board2 = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate> { new(1, 1), new(2, 2), new(3, 3) }
        };

        // Act
        var saved1 = _repository!.CreateBoard(board1);
        var saved2 = _repository.CreateBoard(board2);

        // Assert: Same data but different IDs
        Assert.NotEqual(saved1.Id, saved2.Id);
        Assert.Equal(saved1.Width, saved2.Width);
        Assert.Equal(saved1.Height, saved2.Height);
    }

    #endregion

    #region Read Tests

    [Fact]
    public async Task GetBoards_ReturnsBoardAfterCreation() {
        // Arrange
        var board = CreateTestBoard();
        var savedBoard = _repository!.CreateBoard(board);

        // Act
        var retrievedBoard = _repository.GetBoards(1, 1)[0];

        // Assert
        Assert.NotNull(retrievedBoard);
        Assert.Equal(savedBoard.Id, retrievedBoard!.Id);
        Assert.Equal(savedBoard.Width, retrievedBoard.Width);
        Assert.Equal(savedBoard.Height, retrievedBoard.Height);
        Assert.Equal(savedBoard.LiveCells.Count, retrievedBoard.LiveCells.Count);
    }

    [Fact]
    public async Task GetBoards_ReturnsBoardOnSecondPageAfterCreation() {
        // Arrange
        var firstSavedBoard = _repository!.CreateBoard(CreateTestBoard());
        var secondSavedBoard = _repository!.CreateBoard(CreateTestBoard());

        // Act
        var retrievedBoard = _repository.GetBoards(2, 1)[0];

        // Assert
        Assert.NotNull(retrievedBoard);
        Assert.Contains(retrievedBoard!.Id, new List<Guid>() { firstSavedBoard.Id, secondSavedBoard.Id });
        Assert.Contains(retrievedBoard.Width, new List<int>() { firstSavedBoard.Width, secondSavedBoard.Width });
        Assert.Contains(retrievedBoard.Height, new List<int>() { firstSavedBoard.Height, secondSavedBoard.Height });
        Assert.Contains(retrievedBoard.LiveCells.Count, new List<int>() { firstSavedBoard.LiveCells.Count, secondSavedBoard.LiveCells.Count });
    }

    [Fact]
    public async Task GetBoards_ReturnsMultipleBoardsOnPageAfterCreation() {
        // Arrange
        _repository!.CreateBoard(CreateTestBoard());
        _repository!.CreateBoard(CreateTestBoard());

        // Act
        var retrievedBoardList = _repository.GetBoards(1, 2);

        // Assert
        Assert.True(retrievedBoardList.Count == 2);
    }

    [Fact]
    public async Task GetBoards_ReturnsEmptyList_ForNonExistentBoard() {
        // Act
        var boardList = _repository!.GetBoards(1, 1);

        // Assert
        Assert.Empty(boardList);
    }

    [Fact]
    public async Task GetBoardById_ReturnsBoardAfterCreation() {
        // Arrange
        var board = CreateTestBoard();
        var savedBoard = _repository!.CreateBoard(board);

        // Act
        var retrievedBoard = _repository.GetBoardById(savedBoard.Id);

        // Assert
        Assert.NotNull(retrievedBoard);
        Assert.Equal(savedBoard.Id, retrievedBoard!.Id);
        Assert.Equal(savedBoard.Width, retrievedBoard.Width);
        Assert.Equal(savedBoard.Height, retrievedBoard.Height);
        Assert.Equal(savedBoard.LiveCells.Count, retrievedBoard.LiveCells.Count);
    }

    [Fact]
    public async Task GetBoardById_ReturnsNull_ForNonExistentBoard() {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var board = _repository!.GetBoardById(nonExistentId);

        // Assert
        Assert.Null(board);
    }

    [Fact]
    public async Task GetBoardById_PreservesAllLiveCells() {
        // Arrange
        var liveCells = new (int, int)[] {
            (0, 0), (1, 2), (3, 1), (4, 4), (2, 3)
        };
        
        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = liveCells.Select(t => new CellCoordinate(t.Item1, t.Item2)).ToList()
        };
        
        var savedBoard = _repository!.CreateBoard(board);

        // Act
        var retrievedBoard = _repository.GetBoardById(savedBoard.Id);

        // Assert
        Assert.NotNull(retrievedBoard);
        Assert.Equal(liveCells.Length, retrievedBoard!.LiveCells.Count);
        foreach (var cell in liveCells) {
            Assert.Contains(new CellCoordinate(cell.Item1, cell.Item2), retrievedBoard.LiveCells);
        }
    }

    [Fact]
    public async Task GetBoardById_MultipleBoards_ReturnsCorrectOne() {
        // Arrange
        var board1 = new Board {
            Id = Guid.NewGuid(),
            Width = 5,
            Height = 5,
            LiveCells = new List<CellCoordinate> { new(1, 1) }
        };
        
        var board2 = new Board {
            Id = Guid.NewGuid(),
            Width = 10,
            Height = 10,
            LiveCells = new List<CellCoordinate> { new(5, 5) }
        };

        var saved1 = _repository!.CreateBoard(board1);
        var saved2 = _repository.CreateBoard(board2);

        // Act
        var retrieved1 = _repository.GetBoardById(saved1.Id);
        var retrieved2 = _repository.GetBoardById(saved2.Id);

        // Assert
        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.Equal(5, retrieved1!.Width);
        Assert.Equal(10, retrieved2!.Width);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteBoard_RemovesBoardSuccessfully() {
        // Arrange
        var board = CreateTestBoard();
        var savedBoard = _repository!.CreateBoard(board);

        // Act
        var deleteResult = _repository.DeleteBoard(savedBoard.Id);
        var retrievedAfterDelete = _repository.GetBoardById(savedBoard.Id);

        // Assert
        Assert.True(deleteResult);
        Assert.Null(retrievedAfterDelete);
    }

    [Fact]
    public async Task DeleteBoard_ReturnsFalse_ForNonExistentBoard() {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = _repository!.DeleteBoard(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteBoard_DoesNotAffectOtherBoards() {
        // Arrange
        var board1 = CreateTestBoard(5, 5);
        var board2 = CreateTestBoard(10, 10);

        var saved1 = _repository!.CreateBoard(board1);
        var saved2 = _repository.CreateBoard(board2);

        // Act
        _repository.DeleteBoard(saved1.Id);

        // Assert
        var retrieved1 = _repository.GetBoardById(saved1.Id);
        var retrieved2 = _repository.GetBoardById(saved2.Id);

        Assert.Null(retrieved1);
        Assert.NotNull(retrieved2);
    }

    #endregion

    #region Exists Tests

    [Fact]
    public async Task BoardExists_ReturnsFalse_ForNonExistentBoard() {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var exists = _repository!.BoardExists(nonExistentId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task BoardExists_ReturnsTrue_ForExistingBoard() {
        // Arrange
        var board = CreateTestBoard();
        var savedBoard = _repository!.CreateBoard(board);

        // Act
        var exists = _repository.BoardExists(savedBoard.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task BoardExists_ReturnsFalse_AfterBoardDeleted() {
        // Arrange
        var board = CreateTestBoard();
        var savedBoard = _repository!.CreateBoard(board);
        _repository.DeleteBoard(savedBoard.Id);

        // Act
        var exists = _repository.BoardExists(savedBoard.Id);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task MultiipleSavesAndRetrieves_MaintainStateIntegrity() {
        // Arrange: Create multiple boards and retrieve them all
        var boards = new[] {
            CreateTestBoard(3, 3),
            CreateTestBoard(5, 5),
            CreateTestBoard(7, 7)
        };

        var savedIds = new List<Guid>();

        // Act: Create all boards
        foreach (var board in boards) {
            var saved = _repository!.CreateBoard(board);
            savedIds.Add(saved.Id);
        }

        // Assert: Retrieve and verify all boards
        foreach (var id in savedIds) {
            var retrieved = _repository!.GetBoardById(id);
            Assert.NotNull(retrieved);
            Assert.Equal(id, retrieved!.Id);
        }
    }

    [Fact]
    public async Task CreateBoard_WithLargeLiveCellsSet() {
        // Arrange: Create a board with many live cells
        var liveCells = new List<CellCoordinate>();
        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 10; y++) {
                // Checkerboard pattern
                if ((x + y) % 2 == 0) {
                    liveCells.Add(new CellCoordinate(x, y));
                }
            }
        }

        var board = new Board {
            Id = Guid.NewGuid(),
            Width = 10,
            Height = 10,
            LiveCells = liveCells
        };

        // Act
        var savedBoard = _repository!.CreateBoard(board);
        var retrievedBoard = _repository.GetBoardById(savedBoard.Id);

        // Assert
        Assert.NotNull(retrievedBoard);
        Assert.Equal(liveCells.Count, retrievedBoard!.LiveCells.Count);
        foreach (var cell in liveCells) {
            Assert.Contains(new CellCoordinate(cell.X, cell.Y), retrievedBoard.LiveCells);
        }
    }

    [Fact]
    public async Task BoardState_NotAffectedByLargeOrSmallDimensions() {
        // Arrange: Test with various board dimensions
        var testCases = new[] { (1, 1), (5, 5), (100, 100), (50, 75) };

        foreach (var (width, height) in testCases) {
            // Act
            var board = new Board {
                Id = Guid.NewGuid(),
                Width = width,
                Height = height,
                LiveCells = new List<CellCoordinate> { new(0, 0) }
            };

            var saved = _repository!.CreateBoard(board);
            var retrieved = _repository.GetBoardById(saved.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(width, retrieved!.Width);
            Assert.Equal(height, retrieved.Height);
        }
    }

    #endregion
}
