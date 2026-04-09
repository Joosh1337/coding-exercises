namespace Api.Dtos;

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
public class BoardStateResponse {
    /// <summary>
    /// The current generation number.
    /// </summary>
    public int Generation { get; set; }

    /// <summary>
    /// The width of the board.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// The height of the board.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Array of live cell coordinates [x, y].
    /// </summary>
    public int[][] LiveCells { get; set; }

    public BoardStateResponse(int generation, int width, int height, int[][] liveCells) {
        Generation = generation;
        Width = width;
        Height = height;
        LiveCells = liveCells;
    }

    /// <summary>
    /// Creates a BoardStateResponse from a BoardState domain model.
    /// </summary>
    public static BoardStateResponse FromBoardState(api.Models.BoardState boardState) {
        var boardArray = boardState.GenerateBoardArray();
        var liveCells = new List<int[]>();

        for (int y = 0; y < boardArray.Length; y++) {
            for (int x = 0; x < boardArray[y].Length; x++) {
                if (boardArray[y][x] == 1) {
                    liveCells.Add(new[] { x, y });
                }
            }
        }

        return new BoardStateResponse(
            boardState.Generation,
            boardState.Width,
            boardState.Height,
            liveCells.ToArray()
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
    public int Width { get; set; }

    /// <summary>
    /// The height of the board (must be > 0).
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Array of live cell coordinates [x, y].
    /// </summary>
    public required int[][] LiveCells { get; set; }
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
