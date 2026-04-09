using Api.Controllers;
using Api.Dtos;
using Api.Exceptions;
using Api.Services;
using api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace api.Tests.Controllers;

/// <summary>
/// Comprehensive test suite for the BoardsController API endpoints.
/// Tests cover happy paths, edge cases, error handling, and exception scenarios.
/// </summary>
public class BoardsControllerTests {
    private readonly Mock<IGameOfLifeService> _mockGameOfLifeService;
    private readonly Mock<ILogger<BoardsController>> _mockLogger;
    private readonly BoardsController _controller;

    public BoardsControllerTests() {
        _mockGameOfLifeService = new Mock<IGameOfLifeService>();
        _mockLogger = new Mock<ILogger<BoardsController>>();
        _controller = new BoardsController(_mockGameOfLifeService.Object, _mockLogger.Object);
    }

    #region CreateBoard Tests

    [Fact]
    public async Task CreateBoard_WithValidRequest_ReturnsOkWithBoardId() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new CreateBoardDto {
            Width = 10,
            Height = 10,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 1, 1 } }
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(request.Width, request.Height, request.InitialCells))
            .ReturnsAsync(boardId);

        // Act
        var result = await _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<CreateBoardResponse>;
        response.Should().NotBeNull();
        response?.Data.Id.Should().Be(boardId);
        response?.Message.Should().Be("Board created successfully.");

        _mockGameOfLifeService.Verify(
            s => s.CreateBoard(request.Width, request.Height, request.InitialCells),
            Times.Once);
    }

    [Fact]
    public async Task CreateBoard_WithNullRequest_ReturnsBadRequest() {
        // Act
        var result = await _controller.CreateBoard(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Invalid request");
    }

    [Fact]
    public async Task CreateBoard_WithNullInitialCells_ReturnsBadRequest() {
        // Arrange
        var request = new CreateBoardDto {
            Width = 10,
            Height = 10,
            InitialCells = null!
        };

        // Act
        var result = await _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.Message.Should().Contain("Invalid request");
    }

    [Fact]
    public async Task CreateBoard_WhenServiceThrowsInvalidBoardStateException_ReturnsBadRequest() {
        // Arrange
        var request = new CreateBoardDto {
            Width = -1,
            Height = 10,
            InitialCells = new[] { new[] { 0, 0 } }
        };
        var exceptionMessage = "Width must be positive";

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>()))
            .ThrowsAsync(new InvalidBoardStateException(exceptionMessage));

        // Act
        var result = await _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task CreateBoard_WhenServiceThrowsGenericException_Returns500InternalServerError() {
        // Arrange
        var request = new CreateBoardDto {
            Width = 10,
            Height = 10,
            InitialCells = new[] { new[] { 0, 0 } }
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(500);

        var response = objectResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(500);
        response?.Message.Should().Contain("Internal server error while creating board");
    }

    [Fact]
    public async Task CreateBoard_WithEmptyInitialCells_ReturnsOkWithBoardId() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new CreateBoardDto {
            Width = 10,
            Height = 10,
            InitialCells = Array.Empty<int[]>()
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(request.Width, request.Height, request.InitialCells))
            .ReturnsAsync(boardId);

        // Act
        var result = await _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as SuccessResponse<CreateBoardResponse>;
        response?.Data.Id.Should().Be(boardId);
    }

    #endregion

    #region GetBoard Tests

    [Fact]
    public async Task GetBoard_WithValidBoardId_ReturnsOkWithBoardState() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 0,
            Width = 10,
            Height = 10,
            LiveCells = new HashSet<(int x, int y)> { (0, 0), (1, 1) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetBoardState(boardId))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.GetBoard(boardId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<BoardStateResponse>;
        response.Should().NotBeNull();
        response?.Data.Generation.Should().Be(0);
        response?.Data.Width.Should().Be(10);
        response?.Data.Height.Should().Be(10);
        response?.Message.Should().Be("Board state retrieved successfully.");

        _mockGameOfLifeService.Verify(s => s.GetBoardState(boardId), Times.Once);
    }

    [Fact]
    public async Task GetBoard_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.GetBoardState(boardId))
            .ThrowsAsync(new BoardNotFoundException(boardId));

        // Act
        var result = await _controller.GetBoard(boardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
        response?.Message.Should().Contain(boardId.ToString());
    }

    [Fact]
    public async Task GetBoard_WithEmptyBoard_ReturnsOkWithEmptyLiveCells() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 0,
            Width = 5,
            Height = 5,
            LiveCells = new HashSet<(int x, int y)>()
        };

        _mockGameOfLifeService
            .Setup(s => s.GetBoardState(boardId))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.GetBoard(boardId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as SuccessResponse<BoardStateResponse>;
        response?.Data.LiveCells.Should().BeEmpty();
    }

    #endregion

    #region GetNextState Tests

    [Fact]
    public async Task GetNextState_WithValidBoardId_ReturnsOkWithNextGeneration() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 1,
            Width = 10,
            Height = 10,
            LiveCells = new HashSet<(int x, int y)> { (0, 1), (1, 1), (2, 1) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, 1))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.GetNextState(boardId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<BoardRepresentationResponse>;
        response.Should().NotBeNull();
        response?.Data.Generation.Should().Be(1);
        response?.Message.Should().Be("Next generation state retrieved successfully.");

        _mockGameOfLifeService.Verify(s => s.GetStatesAhead(boardId, 1), Times.Once);
    }

    [Fact]
    public async Task GetNextState_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, 1))
            .ThrowsAsync(new BoardNotFoundException(boardId));

        // Act
        var result = await _controller.GetNextState(boardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
    }

    #endregion

    #region GetStatesAhead Tests

    [Fact]
    public async Task GetStatesAhead_WithValidSteps_ReturnsOkWithBoardState() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 5;
        var boardState = new BoardState {
            Generation = steps,
            Width = 10,
            Height = 10,
            LiveCells = new HashSet<(int x, int y)> { (3, 3), (4, 4) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, steps))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<BoardRepresentationResponse>;
        response.Should().NotBeNull();
        response?.Data.Generation.Should().Be(steps);
        response?.Message.Should().Contain($"after {steps} steps");

        _mockGameOfLifeService.Verify(s => s.GetStatesAhead(boardId, steps), Times.Once);
    }

    [Fact]
    public async Task GetStatesAhead_WithZeroSteps_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 0;

        // Act
        var result = await _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Steps must be greater than 0");
        response?.Details?.Should().Contain($"Received steps: {steps}");

        _mockGameOfLifeService.Verify(s => s.GetStatesAhead(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetStatesAhead_WithNegativeSteps_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = -5;

        // Act
        var result = await _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.Message.Should().Contain("Steps must be greater than 0");

        _mockGameOfLifeService.Verify(s => s.GetStatesAhead(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetStatesAhead_WithValidStepsButBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 3;

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, steps))
            .ThrowsAsync(new BoardNotFoundException(boardId));

        // Act
        var result = await _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
    }

    [Fact]
    public async Task GetStatesAhead_WhenServiceThrowsInvalidStepsException_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 10;

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, steps))
            .ThrowsAsync(new InvalidStepsException(steps));

        // Act
        var result = await _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Steps");
    }

    [Fact]
    public async Task GetStatesAhead_WithLargeStepValue_ReturnsOk() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 1000;
        var boardState = new BoardState {
            Generation = steps,
            Width = 10,
            Height = 10,
            LiveCells = new HashSet<(int x, int y)>()
        };

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, steps))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);
    }

    #endregion

    #region GetFinalState Tests

    [Fact]
    public async Task GetFinalState_WithValidBoardId_ReturnsOkWithFinalState() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 42,
            Width = 10,
            Height = 10,
            LiveCells = new HashSet<(int x, int y)> { (5, 5), (5, 6), (6, 5) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetFinalState(boardId))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.GetFinalState(boardId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<BoardRepresentationResponse>;
        response.Should().NotBeNull();
        response?.Message.Should().Be("Final stable state retrieved successfully.");

        _mockGameOfLifeService.Verify(s => s.GetFinalState(boardId), Times.Once);
    }

    [Fact]
    public async Task GetFinalState_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.GetFinalState(boardId))
            .ThrowsAsync(new BoardNotFoundException(boardId));

        // Act
        var result = await _controller.GetFinalState(boardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
    }

    [Fact]
    public async Task GetFinalState_WhenNoFinalStateFound_ReturnsUnprocessableEntity() {
        // Arrange
        var boardId = Guid.NewGuid();
        var maxIterations = 1000;

        _mockGameOfLifeService
            .Setup(s => s.GetFinalState(boardId))
            .ThrowsAsync(new NoFinalStateException(maxIterations));

        // Act
        var result = await _controller.GetFinalState(boardId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(422);

        var response = objectResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(422);
        response?.Message.Should().Contain("No final stable state");
        response?.Message.Should().Contain(maxIterations.ToString());
    }

    #endregion

    #region DeleteBoard Tests

    [Fact]
    public async Task DeleteBoard_WithValidBoardId_ReturnsNoContent() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.DeleteBoard(boardId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult?.StatusCode.Should().Be(204);

        _mockGameOfLifeService.Verify(s => s.DeleteBoard(boardId), Times.Once);
    }

    [Fact]
    public async Task DeleteBoard_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.DeleteBoard(boardId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
        response?.Message.Should().Contain(boardId.ToString());
    }

    [Fact]
    public async Task DeleteBoard_WhenServiceThrowsException_Returns500InternalServerError() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.DeleteBoard(boardId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(500);

        var response = objectResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(500);
        response?.Message.Should().Contain("Internal server error while deleting board");
    }

    #endregion

    #region Integration Scenario Tests

    [Fact]
    public async Task MultipleOperations_CreateGetAndDelete_WorksCorrectly() {
        // Arrange
        var boardId = Guid.NewGuid();
        var createRequest = new CreateBoardDto {
            Width = 10,
            Height = 10,
            InitialCells = new[] { new[] { 5, 5 } }
        };
        var boardState = new BoardState {
            Generation = 0,
            Width = 10,
            Height = 10,
            LiveCells = new HashSet<(int x, int y)> { (5, 5) }
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>()))
            .ReturnsAsync(boardId);

        _mockGameOfLifeService
            .Setup(s => s.GetBoardState(boardId))
            .ReturnsAsync(boardState);

        _mockGameOfLifeService
            .Setup(s => s.DeleteBoard(boardId))
            .ReturnsAsync(true);

        // Act - Create
        var createResult = await _controller.CreateBoard(createRequest);
        createResult.Should().BeOfType<OkObjectResult>();

        // Act - Get
        var getResult = await _controller.GetBoard(boardId);
        getResult.Should().BeOfType<OkObjectResult>();

        // Act - Delete
        var deleteResult = await _controller.DeleteBoard(boardId);
        deleteResult.Should().BeOfType<NoContentResult>();

        // Assert
        _mockGameOfLifeService.Verify(
            s => s.CreateBoard(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>()),
            Times.Once);
        _mockGameOfLifeService.Verify(s => s.GetBoardState(boardId), Times.Once);
        _mockGameOfLifeService.Verify(s => s.DeleteBoard(boardId), Times.Once);
    }

    #endregion

    #region Edge Cases and Data Validation Tests

    [Fact]
    public async Task CreateBoard_WithMinimumValidDimensions_ReturnsOk() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new CreateBoardDto {
            Width = 1,
            Height = 1,
            InitialCells = Array.Empty<int[]>()
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(1, 1, request.InitialCells))
            .ReturnsAsync(boardId);

        // Act
        var result = await _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateBoard_WithLargeDimensions_ReturnsOk() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new CreateBoardDto {
            Width = 10000,
            Height = 10000,
            InitialCells = Array.Empty<int[]>()
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(10000, 10000, request.InitialCells))
            .ReturnsAsync(boardId);

        // Act
        var result = await _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStatesAhead_WithOneStep_ReturnsOk() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 1,
            Width = 5,
            Height = 5,
            LiveCells = new HashSet<(int x, int y)>()
        };

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, 1))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.GetStatesAhead(boardId, 1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as SuccessResponse<BoardRepresentationResponse>;
        response?.Data.Generation.Should().Be(1);
    }

    #endregion
}
