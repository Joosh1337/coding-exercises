using Api.Dtos;
using Api.Exceptions;
using Api.Services;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for managing boards and computing Game of Life states.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase {
    private readonly IGameOfLifeService _gameOfLifeService;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(IGameOfLifeService gameOfLifeService, ILogger<BoardsController> logger) {
        _gameOfLifeService = gameOfLifeService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new board with the specified initial state.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDto request) {
        try {
            if (request == null || request.LiveCells == null)
                return BadRequest(new ErrorResponse(400, "Invalid request: LiveCells array is required."));

            var boardId = await _gameOfLifeService.CreateBoard(request.Width, request.Height, request.LiveCells);

            return Ok(new SuccessResponse<CreateBoardResponse>(
                new CreateBoardResponse(boardId),
                "Board created successfully."
            ));
        } catch (InvalidBoardStateException ex) {
            _logger.LogWarning("Invalid board state: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(400, ex.Message));
        } catch (Exception ex) {
            _logger.LogError(ex, "Error creating board");
            return StatusCode(500, new ErrorResponse(500, "Internal server error while creating board."));
        }
    }

    /// <summary>
    /// Retrieves the initial board state (generation 0).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoard(Guid id) {
        try {
            var boardState = await _gameOfLifeService.GetBoardState(id);
            var response = BoardStateResponse.FromBoardState(boardState);

            return Ok(new SuccessResponse<BoardStateResponse>(
                response,
                "Board state retrieved successfully."
            ));
        } catch (BoardNotFoundException ex) {
            _logger.LogWarning("Board not found: {BoardId}", ex.BoardId);
            return NotFound(new ErrorResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Retrieves the next generation state of a board (generation + 1).
    /// </summary>
    [HttpGet("{id}/states/next")]
    public async Task<IActionResult> GetNextState(Guid id) {
        try {
            var boardState = await _gameOfLifeService.GetStatesAhead(id, 1);
            var response = BoardStateResponse.FromBoardState(boardState);

            return Ok(new SuccessResponse<BoardStateResponse>(
                response,
                "Next generation state retrieved successfully."
            ));
        } catch (BoardNotFoundException ex) {
            _logger.LogWarning("Board not found: {BoardId}", ex.BoardId);
            return NotFound(new ErrorResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Retrieves the board state after a specified number of steps.
    /// </summary>
    [HttpGet("{id}/states")]
    public async Task<IActionResult> GetStatesAhead(Guid id, [FromQuery] int steps) {
        try {
            if (steps <= 0)
                return BadRequest(new ErrorResponse(400, "Steps must be greater than 0.", new[] { $"Received steps: {steps}" }));

            var boardState = await _gameOfLifeService.GetStatesAhead(id, steps);
            var response = BoardStateResponse.FromBoardState(boardState);

            return Ok(new SuccessResponse<BoardStateResponse>(
                response,
                $"Board state after {steps} steps retrieved successfully."
            ));
        } catch (BoardNotFoundException ex) {
            _logger.LogWarning("Board not found: {BoardId}", ex.BoardId);
            return NotFound(new ErrorResponse(404, ex.Message));
        } catch (InvalidStepsException ex) {
            _logger.LogWarning("Invalid steps provided: {Steps}", ex.Steps);
            return BadRequest(new ErrorResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Retrieves the final stable state of a board.
    /// </summary>
    [HttpGet("{id}/states/final")]
    public async Task<IActionResult> GetFinalState(Guid id) {
        try {
            var boardState = await _gameOfLifeService.GetFinalState(id);
            var response = BoardStateResponse.FromBoardState(boardState);

            return Ok(new SuccessResponse<BoardStateResponse>(
                response,
                "Final stable state retrieved successfully."
            ));
        } catch (BoardNotFoundException ex) {
            _logger.LogWarning("Board not found: {BoardId}", ex.BoardId);
            return NotFound(new ErrorResponse(404, ex.Message));
        } catch (NoFinalStateException ex) {
            _logger.LogWarning("No final state found within max iterations: {MaxIterations}", ex.MaxIterations);
            return StatusCode(422, new ErrorResponse(422, ex.Message));
        }
    }

    /// <summary>
    /// Deletes a board from the system.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(Guid id) {
        try {
            var deleted = await _gameOfLifeService.DeleteBoard(id);

            if (!deleted) {
                _logger.LogWarning("Board not found for deletion: {BoardId}", id);
                return NotFound(new ErrorResponse(404, $"Board with ID '{id}' not found."));
            }

            _logger.LogInformation("Board deleted: {BoardId}", id);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, "Error deleting board {BoardId}", id);
            return StatusCode(500, new ErrorResponse(500, "Internal server error while deleting board."));
        }
    }
}
