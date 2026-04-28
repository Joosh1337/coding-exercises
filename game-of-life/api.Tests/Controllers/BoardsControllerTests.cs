using api.Controllers;
using api.Dtos;
using api.Exceptions;
using api.Services;
using api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    public void CreateBoard_WithValidRequest_ReturnsOkWithBoardId() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new CreateBoardDto {
            Name = "Test Board",
            Width = 2,
            Height = 2,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 1, 1 } }
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(request.Width, request.Height, request.InitialCells, request.Name))
            .Returns(boardId);

        // Act
        var result = _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<CreateBoardResponse>;
        response.Should().NotBeNull();
        response?.Data.Id.Should().Be(boardId);
        response?.Message.Should().Be("Board created successfully.");

        _mockGameOfLifeService.Verify(
            s => s.CreateBoard(request.Width, request.Height, request.InitialCells, request.Name),
            Times.Once);
    }

    [Fact]
    public void CreateBoard_WithNullRequest_ReturnsBadRequest() {
        // Act
        var result = _controller.CreateBoard(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Invalid request");
    }

    [Fact]
    public void CreateBoard_WithNullInitialCells_ReturnsBadRequest() {
        // Arrange
        var request = new CreateBoardDto {
            Width = 10,
            Height = 10,
            InitialCells = null!
        };

        // Act
        var result = _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.Message.Should().Contain("Invalid request");
    }

    [Fact]
    public void CreateBoard_WithEmptyName_ReturnsBadRequest() {
        // Arrange
        var request = new CreateBoardDto {
            Name = "",
            Width = 2,
            Height = 2,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 0, 0 } }
        };

        // Act
        var result = _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Name is required");
        _mockGameOfLifeService.Verify(s => s.CreateBoard(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CreateBoard_WithWhitespaceName_ReturnsBadRequest() {
        // Arrange
        var request = new CreateBoardDto {
            Name = "   ",
            Width = 2,
            Height = 2,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 0, 0 } }
        };

        // Act
        var result = _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Name is required");
    }

    [Fact]
    public void CreateBoard_WhenServiceThrowsInvalidBoardStateException_ReturnsBadRequest() {
        // Arrange
        var request = new CreateBoardDto {
            Name = "Test Board",
            Width = -1,
            Height = 1,
            InitialCells = new[] { new[] { 0, 0 } }
        };
        var exceptionMessage = "Width must be positive";

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>(), It.IsAny<string>()))
            .Throws(new InvalidBoardStateException(exceptionMessage));

        // Act
        var result = _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Be(exceptionMessage);
    }

    [Fact]
    public void CreateBoard_WhenServiceThrowsGenericException_Returns500InternalServerError() {
        // Arrange
        var request = new CreateBoardDto {
            Name = "Test Board",
            Width = 2,
            Height = 1,
            InitialCells = new[] { new[] { 0, 0 } }
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>(), It.IsAny<string>()))
            .Throws(new Exception("Unexpected error"));

        // Act
        var result = _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(500);

        var response = objectResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(500);
        response?.Message.Should().Contain("Internal server error while creating board");
    }

    [Fact]
    public void CreateBoard_WithEmptyInitialCells_ReturnsOkWithBoardId() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new CreateBoardDto {
            Name = "Test Board",
            Width = 10,
            Height = 10,
            InitialCells = Array.Empty<int[]>()
        };

        _mockGameOfLifeService
            .Setup(s => s.CreateBoard(request.Width, request.Height, request.InitialCells, request.Name))
            .Returns(boardId);

        // Act
        var result = _controller.CreateBoard(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as SuccessResponse<CreateBoardResponse>;
        response?.Data.Id.Should().Be(boardId);
    }

    #endregion

    #region GetBoards Tests

    [Fact]
    public void GetBoards_WhenBoardExists_ReturnsOkWithBoards() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 0,
            Width = 2,
            Height = 2,
            LiveCells = new HashSet<(int x, int y)> { (0, 0), (1, 1) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetBoardStates(1, 1))
            .Returns(new List<BoardState>() { boardState });

        var request = new PaginationDto {
            Page = 1,
            PageSize = 1
        };

        // Act
        var result = _controller.GetBoards(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<List<BoardResponse>>;
        response.Should().NotBeNull();
        response?.Data.Count.Should().Be(1);
        response?.Message.Should().Be("Board states retrieved successfully.");

        _mockGameOfLifeService.Verify(s => s.GetBoardStates(request.Page, request.PageSize), Times.Once);
    }

    [Fact]
    public void GetBoard_WhenBoardsEmpty_ReturnsOkWithNoBoards() {
        // Arrange
        _mockGameOfLifeService
            .Setup(s => s.GetBoardStates(1, 1))
            .Returns(new List<BoardState>() { });

        var request = new PaginationDto {
            Page = 1,
            PageSize = 1
        };

        // Act
        var result = _controller.GetBoards(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<List<BoardResponse>>;
        response.Should().NotBeNull();
        response?.Data.Count.Should().Be(0);
        response?.Message.Should().Be("Board states retrieved successfully.");

        _mockGameOfLifeService.Verify(s => s.GetBoardStates(request.Page, request.PageSize), Times.Once);
    }

    #endregion
    
    #region GetBoard Tests

    [Fact]
    public void GetBoard_WithValidBoardId_ReturnsOkWithBoards() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 0,
            Width = 2,
            Height = 2,
            LiveCells = new HashSet<(int x, int y)> { (0, 0), (1, 1) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetBoardState(boardId))
            .Returns(boardState);

        // Act
        var result = _controller.GetBoard(boardId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be(200);

        var response = okResult?.Value as SuccessResponse<BoardResponse>;
        response.Should().NotBeNull();
        response?.Data.Generation.Should().Be(0);
        response?.Data.Width.Should().Be(2);
        response?.Data.Height.Should().Be(2);
        response?.Message.Should().Be("Board state retrieved successfully.");

        _mockGameOfLifeService.Verify(s => s.GetBoardState(boardId), Times.Once);
    }

    [Fact]
    public void GetBoard_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.GetBoardState(boardId))
            .Throws(new BoardNotFoundException(boardId));

        // Act
        var result = _controller.GetBoard(boardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
        response?.Message.Should().Contain(boardId.ToString());
    }

    #endregion

    #region GetNextState Tests

    [Fact]
    public void GetNextState_WithValidBoardId_ReturnsOkWithNextGeneration() {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState {
            Generation = 1,
            Width = 3,
            Height = 2,
            LiveCells = new HashSet<(int x, int y)> { (0, 1), (1, 1), (2, 1) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, 1))
            .Returns(boardState);

        // Act
        var result = _controller.GetNextState(boardId);

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
    public void GetNextState_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, 1))
            .Throws(new BoardNotFoundException(boardId));

        // Act
        var result = _controller.GetNextState(boardId);

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
    public void GetStatesAhead_WithValidSteps_ReturnsOkWithBoardState() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 5;
        var boardState = new BoardState {
            Generation = steps,
            Width = 2,
            Height = 2,
            LiveCells = new HashSet<(int x, int y)> { (3, 3), (4, 4) }
        };

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, steps))
            .Returns(boardState);

        // Act
        var result = _controller.GetStatesAhead(boardId, steps);

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
    public void GetStatesAhead_WithZeroSteps_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 0;

        // Act
        var result = _controller.GetStatesAhead(boardId, steps);

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
    public void GetStatesAhead_WithNegativeSteps_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = -5;

        // Act
        var result = _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest?.StatusCode.Should().Be(400);

        var response = badRequest?.Value as ErrorResponse;
        response?.Message.Should().Contain("Steps must be greater than 0");

        _mockGameOfLifeService.Verify(s => s.GetStatesAhead(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetStatesAhead_WithBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();
        var steps = 3;

        _mockGameOfLifeService
            .Setup(s => s.GetStatesAhead(boardId, steps))
            .Throws(new BoardNotFoundException(boardId));

        // Act
        var result = _controller.GetStatesAhead(boardId, steps);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
    }

    #endregion

    #region GetFinalState Tests

    [Fact]
    public void GetFinalState_WithValidBoardId_ReturnsOkWithFinalState() {
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
            .Returns(boardState);

        // Act
        var result = _controller.GetFinalState(boardId);

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
    public void GetFinalState_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.GetFinalState(boardId))
            .Throws(new BoardNotFoundException(boardId));

        // Act
        var result = _controller.GetFinalState(boardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be(404);

        var response = notFoundResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
    }

    [Fact]
    public void GetFinalState_WhenNoFinalStateFound_ReturnsUnprocessableEntity() {
        // Arrange
        var boardId = Guid.NewGuid();
        var maxIterations = 1000;

        _mockGameOfLifeService
            .Setup(s => s.GetFinalState(boardId))
            .Throws(new NoFinalStateException(maxIterations));

        // Act
        var result = _controller.GetFinalState(boardId);

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

    #region UpdateBoard Tests

    [Fact]
    public void UpdateBoard_WithValidRequest_ReturnsNoContent() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new UpdateBoardDto {
            Name = "Updated Board",
            Width = 3,
            Height = 3,
            InitialCells = new[] { new[] { 1, 0, 0 }, new[] { 0, 1, 0 }, new[] { 0, 0, 1 } }
        };

        _mockGameOfLifeService
            .Setup(s => s.UpdateBoard(boardId, request.Name, request.Width, request.Height, request.InitialCells));

        // Act
        var result = _controller.UpdateBoard(boardId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        ((NoContentResult)result).StatusCode.Should().Be(204);
        _mockGameOfLifeService.Verify(
            s => s.UpdateBoard(boardId, request.Name, request.Width, request.Height, request.InitialCells),
            Times.Once);
    }

    [Fact]
    public void UpdateBoard_WithNullRequest_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();

        // Act
        var result = _controller.UpdateBoard(boardId, null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Invalid request");
        _mockGameOfLifeService.Verify(s => s.UpdateBoard(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>()), Times.Never);
    }

    [Fact]
    public void UpdateBoard_WithNullInitialCells_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new UpdateBoardDto { Name = "Board", Width = 3, Height = 3, InitialCells = null! };

        // Act
        var result = _controller.UpdateBoard(boardId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        _mockGameOfLifeService.Verify(s => s.UpdateBoard(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>()), Times.Never);
    }

    [Fact]
    public void UpdateBoard_WithEmptyName_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new UpdateBoardDto {
            Name = "",
            Width = 2,
            Height = 2,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 0, 0 } }
        };

        // Act
        var result = _controller.UpdateBoard(boardId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Name is required");
        _mockGameOfLifeService.Verify(s => s.UpdateBoard(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[][]>()), Times.Never);
    }

    [Fact]
    public void UpdateBoard_WithWhitespaceName_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new UpdateBoardDto {
            Name = "   ",
            Width = 2,
            Height = 2,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 0, 0 } }
        };

        // Act
        var result = _controller.UpdateBoard(boardId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Contain("Name is required");
    }

    [Fact]
    public void UpdateBoard_WhenBoardNotFound_ReturnsNotFound() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new UpdateBoardDto {
            Name = "Board",
            Width = 2,
            Height = 2,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 0, 0 } }
        };

        _mockGameOfLifeService
            .Setup(s => s.UpdateBoard(boardId, request.Name, request.Width, request.Height, request.InitialCells))
            .Throws(new BoardNotFoundException(boardId));

        // Act
        var result = _controller.UpdateBoard(boardId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var response = ((NotFoundObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(404);
        response?.Message.Should().Contain(boardId.ToString());
    }

    [Fact]
    public void UpdateBoard_WhenServiceThrowsInvalidBoardStateException_ReturnsBadRequest() {
        // Arrange
        var boardId = Guid.NewGuid();
        var request = new UpdateBoardDto {
            Name = "Board",
            Width = 2,
            Height = 2,
            InitialCells = new[] { new[] { 0, 0 }, new[] { 0, 0 } }
        };
        var exceptionMessage = "Grid dimensions do not match";

        _mockGameOfLifeService
            .Setup(s => s.UpdateBoard(boardId, request.Name, request.Width, request.Height, request.InitialCells))
            .Throws(new InvalidBoardStateException(exceptionMessage));

        // Act
        var result = _controller.UpdateBoard(boardId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var response = ((BadRequestObjectResult)result).Value as ErrorResponse;
        response?.ErrorCode.Should().Be(400);
        response?.Message.Should().Be(exceptionMessage);
    }

    #endregion

    #region DeleteBoard Tests

    [Fact]
    public void DeleteBoard_WithValidBoardId_ReturnsNoContent() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.DeleteBoard(boardId))
            .Returns(true);

        // Act
        var result = _controller.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult?.StatusCode.Should().Be(204);

        _mockGameOfLifeService.Verify(s => s.DeleteBoard(boardId), Times.Once);
    }

    [Fact]
    public void DeleteBoard_WhenBoardNotFound_ReturnsNoContent() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.DeleteBoard(boardId))
            .Returns(false);

        // Act
        var result = _controller.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var notFoundResult = result as NoContentResult;
        notFoundResult?.StatusCode.Should().Be(204);
    }

    [Fact]
    public void DeleteBoard_WhenServiceThrowsException_Returns500InternalServerError() {
        // Arrange
        var boardId = Guid.NewGuid();

        _mockGameOfLifeService
            .Setup(s => s.DeleteBoard(boardId))
            .Throws(new Exception("Database connection failed"));

        // Act
        var result = _controller.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be(500);

        var response = objectResult?.Value as ErrorResponse;
        response?.ErrorCode.Should().Be(500);
        response?.Message.Should().Contain("Internal server error while deleting board");
    }

    #endregion

}
