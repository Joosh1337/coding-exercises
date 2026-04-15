using System.ComponentModel.DataAnnotations;

namespace api.Dtos;

/// <summary>
/// Generic success response wrapper for API responses.
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class SuccessResponse<T> {
    /// <summary>
    /// The response data payload.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// A descriptive message about the operation.
    /// </summary>
    public string Message { get; set; }

    public SuccessResponse(T data, string message = "Success") {
        Data = data;
        Message = message;
    }
}

/// <summary>
/// Standard error response format for API errors.
/// </summary>
public class ErrorResponse {
    /// <summary>
    /// The error code (HTTP status code).
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// A descriptive message about the error.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Detailed validation errors (if applicable).
    /// </summary>
    public string[]? Details { get; set; }

    public ErrorResponse(int errorCode, string message, string[]? details = null) {
        ErrorCode = errorCode;
        Message = message;
        Details = details;
    }
}

/// <summary>
/// Response DTO for representing a board state.
/// </summary>
public class BoardResponse {
    /// <summary>
    /// Unique identifier for the board.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The current generation number.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Generation { get; set; }

    /// <summary>
    /// The width of the board.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Width { get; set; }

    /// <summary>
    /// The height of the board.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Height { get; set; }

    /// <summary>
    /// Array of live cell coordinates [x, y].
    /// </summary>
    public int[][] LiveCells { get; set; }

    public BoardResponse(Guid id, int generation, int width, int height, int[][] liveCells) {
        Id = id;
        Generation = generation;
        Width = width;
        Height = height;
        LiveCells = liveCells;
    }

    /// <summary>
    /// Creates a BoardResponse from a BoardState domain model.
    /// </summary>
    public static BoardResponse FromBoardState(api.Models.BoardState boardState) {
        var liveCells = boardState.LiveCells
            .Select(cell => new int[] { cell.x, cell.y })
            .ToArray();

        return new BoardResponse(
            boardState.Id,
            boardState.Generation,
            boardState.Width,
            boardState.Height,
            liveCells
        );
    }
}

/// <summary>
/// Response DTO for representing a board state with full board representation.
/// Used for /states endpoints to return the complete board grid.
/// </summary>
public class BoardRepresentationResponse {
        /// <summary>
    /// Unique identifier for the board.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The current generation number.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Generation { get; set; }

    /// <summary>
    /// The width of the board.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Width { get; set; }

    /// <summary>
    /// The height of the board.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Height { get; set; }

    /// <summary>
    /// 2D array representing the board state where 1 = alive, 0 = dead.
    /// Array is [y][x] indexed (rows first, then columns).
    /// </summary>
    public int[][] BoardDisplay { get; set; }

    public BoardRepresentationResponse(Guid id, int generation, int width, int height, int[][] boardDisplay) {
        Id = id;
        Generation = generation;
        Width = width;
        Height = height;
        BoardDisplay = boardDisplay;
    }

    /// <summary>
    /// Creates a BoardRepresentationResponse from a BoardState domain model.
    /// </summary>
    public static BoardRepresentationResponse FromBoardState(api.Models.BoardState boardState) {
        return new BoardRepresentationResponse(
            boardState.Id,
            boardState.Generation,
            boardState.Width,
            boardState.Height,
            boardState.GenerateBoardArray()
        );
    }
}

/// <summary>
/// Request DTO for creating a new board.
/// </summary>
public class CreateBoardDto {
    /// <summary>
    /// The width of the board (must be > 0).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Width { get; set; }

    /// <summary>
    /// The height of the board (must be > 0).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Height { get; set; }

    /// <summary>
    /// Array of live cell coordinates [x, y].
    /// </summary>
    public required int[][] InitialCells { get; set; }
}

/// <summary>
/// Response DTO for board creation containing the new board ID.
/// </summary>
public class CreateBoardResponse {
    /// <summary>
    /// The unique identifier of the created board.
    /// </summary>
    public Guid Id { get; set; }

    public CreateBoardResponse(Guid id) {
        Id = id;
    }
}

/// <summary>
/// Response DTO for board creation containing the new board ID.
/// </summary>
public class PaginationDto {
    /// <summary>
    /// The page number
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// The size of the page
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
