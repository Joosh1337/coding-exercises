using Moq;
using FluentAssertions;
using api.Models;
using api.Repositories;
using api.Services;
using api.Exceptions;
using Microsoft.Extensions.Configuration;

namespace api.Tests.Services;

/// <summary>
/// Comprehensive test suite for GameOfLifeService.
/// Tests all public methods including edge cases, error scenarios, and business logic.
/// </summary>
public class GameOfLifeServiceTests {
    private readonly Mock<IBoardRepository> _mockRepository;
    private readonly GameOfLifeService _service;

    public GameOfLifeServiceTests() {
        _mockRepository = new Mock<IBoardRepository>();
        
        // Create a real configuration with test values - no need to mock
        var configDict = new Dictionary<string, string?> {
            { "GameOfLife:MaxIterationsForFinalState", "9999" }
        };
        
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict);
        var realConfig = configBuilder.Build();

        _service = new GameOfLifeService(_mockRepository.Object, realConfig);
    }

    #region CreateBoard Tests

    [Fact]
    public async Task CreateBoard_WithValidInput_CreatesAndPersistsBoardSuccessfully() {
        // Arrange
        int width = 3, height = 3;
        int[][] initialCells = new[] {
            new[] { 1, 0, 0 },
            new[] { 0, 1, 0 },
            new[] { 0, 0, 1 } 
        };
        var expectedId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.CreateBoard(It.IsAny<Board>()))
            .Returns((Board board) => {
                board.Id = expectedId;
                return board;
            });

        // Act
        var result = _service.CreateBoard(width, height, initialCells);

        // Assert
        result.Should().Be(expectedId);
        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Once);
    }

    [Fact]
    public async Task CreateBoard_WithZeroWidth_ThrowsInvalidBoardStateException() {
        // Arrange
        int width = 0, height = 5;
        int[][] initialCells = Array.Empty<int[]>();

        // Act & Assert
        Assert.Throws<InvalidBoardStateException>(() =>
            _service.CreateBoard(width, height, initialCells));

        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task CreateBoard_WithNegativeHeight_ThrowsInvalidBoardStateException() {
        // Arrange
        int width = 5, height = -1;
        int[][] initialCells = Array.Empty<int[]>();

        // Act & Assert
        Assert.Throws<InvalidBoardStateException>(() =>
            _service.CreateBoard(width, height, initialCells));

        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task CreateBoard_WithTooManyRows_ThrowsInvalidBoardStateException() {
        // Arrange
        int width = 2, height = 2;
        int[][] initialCells = new[] {
            new[] { 0, 0 },
            new[] { 0, 0 },
            new[] { 0, 0 }
        };

        // Act & Assert
        Assert.Throws<InvalidBoardStateException>(() =>
            _service.CreateBoard(width, height, initialCells));

        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task CreateBoard_WithTooManyColumns_ThrowsInvalidBoardStateException() {
        // Arrange
        int width = 2, height = 2;
        int[][] initialCells = new[] {
            new[] { 0, 0, 0 },
            new[] { 0, 0, 0 }
        };

        // Act & Assert
        Assert.Throws<InvalidBoardStateException>(() =>
            _service.CreateBoard(width, height, initialCells));

        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task CreateBoard_WithTooFewRows_ThrowsInvalidBoardStateException() {
        // Arrange
        int width = 2, height = 2;
        int[][] initialCells = new[] {
            new[] { 0, 0 },
        };

        // Act & Assert
        Assert.Throws<InvalidBoardStateException>(() =>
            _service.CreateBoard(width, height, initialCells));

        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task CreateBoard_WithTooFewColumns_ThrowsInvalidBoardStateException() {
        // Arrange
        int width = 2, height = 2;
        int[][] initialCells = new[] {
            new[] { 0 },
            new[] { 0 }
        };

        // Act & Assert
        Assert.Throws<InvalidBoardStateException>(() =>
            _service.CreateBoard(width, height, initialCells));

        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task CreateBoard_WithEmptyInitialCells_ThrowsInvalidBoardStateException() {
        // Arrange
        int width = 0, height = 0;
        int[][] initialCells = Array.Empty<int[]>();

        // Act & Assert
        Assert.Throws<InvalidBoardStateException>(() =>
            _service.CreateBoard(width, height, initialCells));

        _mockRepository.Verify(r => r.CreateBoard(It.IsAny<Board>()), Times.Never);
    }

    #endregion

    #region GetBoardStates Tests

    [Fact]
    public async Task GetBoardStates_WithExistingBoard_ReturnsInitialStates() {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(5, 5, new List<CellCoordinate> {
            new CellCoordinate(0, 0),
            new CellCoordinate(1, 1)
        });
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoards(1, 1))
            .Returns(new List<Board>() { board });

        // Act
        var state = _service.GetBoardStates(1, 1);

        // Assert
        state.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBoardState_WithNonExistentBoard_ReturnsEmptyList() {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetBoards(1, 1))
            .Returns(new List<Board>() { });

        // Act
        var state = _service.GetBoardStates(1, 1);

        // Assert
        state.Should().BeEmpty();
    }

    #endregion

    #region GetBoardState Tests

    [Fact]
    public async Task GetBoardState_WithExistingBoard_ReturnsInitialState() {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(5, 5, new List<CellCoordinate> {
            new CellCoordinate(0, 0),
            new CellCoordinate(1, 1)
        });
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns(board);

        // Act
        var state = _service.GetBoardState(boardId);

        // Assert
        state.Should().NotBeNull();
        state.Generation.Should().Be(0);
        state.Width.Should().Be(5);
        state.Height.Should().Be(5);
        state.LiveCells.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBoardState_WithNonExistentBoard_ThrowsBoardNotFoundException() {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns((Board?)null);

        // Act & Assert
        Assert.Throws<BoardNotFoundException>(() =>
            _service.GetBoardState(boardId));
    }

    #endregion

    #region GetStatesAhead Tests

    [Fact]
    public async Task GetStatesAhead_WithValidSteps_ComputesCorrectNumberOfGenerations() {
        // Arrange
        var boardId = Guid.NewGuid();
        // Create a simple blinker pattern (oscillator)
        // Vertical: (1,0), (1,1), (1,2)
        var board = new Board(3, 3, new List<CellCoordinate> {
            new CellCoordinate(1, 0),
            new CellCoordinate(1, 1),
            new CellCoordinate(1, 2)
        });
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns(board);

        // Act
        var state = _service.GetStatesAhead(boardId, 1);

        // Assert
        state.Should().NotBeNull();
        state.Generation.Should().Be(1);
        // After one step, blinker should be horizontal: (0,1), (1,1), (2,1)
        state.LiveCells.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetStatesAhead_WithMultipleSteps_ComputesAllGenerations() {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(3, 3, new List<CellCoordinate> {
            new CellCoordinate(1, 0),
            new CellCoordinate(1, 1),
            new CellCoordinate(1, 2)
        });
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns(board);

        // Act
        var state = _service.GetStatesAhead(boardId, 5);

        // Assert
        state.Should().NotBeNull();
        state.Generation.Should().Be(5);
    }

    [Fact]
    public async Task GetStatesAhead_WithZeroSteps_ThrowsInvalidStepsException() {
        // Arrange
        var boardId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidStepsException>(() =>
            _service.GetStatesAhead(boardId, 0));

        _mockRepository.Verify(r => r.GetBoardById(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetStatesAhead_WithNegativeSteps_ThrowsInvalidStepsException() {
        // Arrange
        var boardId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidStepsException>(() =>
            _service.GetStatesAhead(boardId, -5));

        _mockRepository.Verify(r => r.GetBoardById(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetStatesAhead_WithNonExistentBoard_ThrowsBoardNotFoundException() {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns((Board?)null);

        // Act & Assert
        Assert.Throws<BoardNotFoundException>(() =>
            _service.GetStatesAhead(boardId, 1));
    }

    #endregion

    #region GetFinalState Tests

    [Fact]
    public async Task GetFinalState_WithStableBoard_ReturnsFinalState() {
        // Arrange
        var boardId = Guid.NewGuid();
        // Empty board is immediately stable
        var board = new Board(5, 5, new List<CellCoordinate>());
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns(board);

        // Act
        var finalState = _service.GetFinalState(boardId);

        // Assert
        finalState.Should().NotBeNull();
        finalState.Generation.Should().Be(1); // At least one generation computed
        finalState.LiveCells.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFinalState_WithOscillatingBoard_ThrowsNoFinalStateException() {
        // Arrange
        var boardId = Guid.NewGuid();
        // Blinker pattern oscillates forever (period 2)
        var board = new Board(3, 3, new List<CellCoordinate> {
            new CellCoordinate(1, 0),
            new CellCoordinate(1, 1),
            new CellCoordinate(1, 2)
        });
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns(board);

        // Act & Assert
        Assert.Throws<NoFinalStateException>(() =>
            _service.GetFinalState(boardId));
    }

    [Fact]
    public async Task GetFinalState_WithNonExistentBoard_ThrowsBoardNotFoundException() {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns((Board?)null);

        // Act & Assert
        Assert.Throws<BoardNotFoundException>(() =>
            _service.GetFinalState(boardId));
    }

    [Fact]
    public async Task GetFinalState_RespectMaxIterationConfiguration() {
        // Arrange
        var boardId = Guid.NewGuid();
        var maxIterations = 10;

        // Create a real configuration with custom max iterations
        var customConfigDict = new Dictionary<string, string?> {
            { "GameOfLife:MaxIterationsForFinalState", maxIterations.ToString() }
        };
        
        var customConfigBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(customConfigDict);
        var realCustomConfig = customConfigBuilder.Build();
        
        var customConfigMock = new Mock<IConfiguration>();
        customConfigMock
            .Setup(c => c.GetSection(It.IsAny<string>()))
            .Returns((string key) => realCustomConfig.GetSection(key));
        customConfigMock
            .Setup(c => c[It.IsAny<string>()])
            .Returns((string key) => realCustomConfig[key]);

        var service = new GameOfLifeService(_mockRepository.Object, customConfigMock.Object);

        // Blinker that oscillates forever
        var board = new Board(3, 3, new List<CellCoordinate> {
            new CellCoordinate(1, 0),
            new CellCoordinate(1, 1),
            new CellCoordinate(1, 2)
        });
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns(board);

        // Act & Assert
        var exception = Assert.Throws<NoFinalStateException>(() =>
            service.GetFinalState(boardId));

        exception.Message.Should().Contain(maxIterations.ToString());
    }

    #endregion

    #region DeleteBoard Tests

    [Fact]
    public async Task DeleteBoard_WithExistingBoard_DeletesSuccessfully() {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.DeleteBoard(boardId))
            .Returns(true);

        // Act
        var result = _service.DeleteBoard(boardId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteBoard(boardId), Times.Once);
    }

    [Fact]
    public async Task DeleteBoard_WithNonExistentBoard_ReturnsFalse() {
        // Arrange
        var boardId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.DeleteBoard(boardId))
            .Returns(false);

        // Act
        var result = _service.DeleteBoard(boardId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task CompleteWorkflow_CreateBoardComputeStatesAndDelete() {
        // Arrange
        var boardId = Guid.NewGuid();
        var initialCells = new int[][] {
            new[] { 0, 0, 0, 0, 0 },
            new[] { 0, 1, 1, 1, 0 },
            new[] { 0, 0, 0, 0, 0 },
            new[] { 0, 0, 0, 0, 0 },
            new[] { 0, 0, 0, 0, 0 }
        };

        _mockRepository
            .Setup(r => r.CreateBoard(It.IsAny<Board>()))
            .Returns((Board board) => {
                board.Id = boardId;
                return board;
            });

        var board = new Board(5, 5, new List<CellCoordinate> {
            new CellCoordinate(1, 1),
            new CellCoordinate(1, 2),
            new CellCoordinate(1, 3)
        });
        board.Id = boardId;

        _mockRepository
            .Setup(r => r.GetBoardById(boardId))
            .Returns(board);

        _mockRepository
            .Setup(r => r.DeleteBoard(boardId))
            .Returns(true);

        // Act
        var createdId = _service.CreateBoard(5, 5, initialCells);
        var initialState = _service.GetBoardState(createdId);
        var nextState = _service.GetStatesAhead(createdId, 1);
        var deleteResult = _service.DeleteBoard(createdId);

        // Assert
        createdId.Should().Be(boardId);
        initialState.Generation.Should().Be(0);
        nextState.Generation.Should().Be(1);
        deleteResult.Should().BeTrue();
    }

    #endregion
}
