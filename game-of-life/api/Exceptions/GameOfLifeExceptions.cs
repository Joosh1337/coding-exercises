namespace Api.Exceptions;

/// <summary>
/// Thrown when a board with the specified ID is not found in the repository.
/// </summary>
public class BoardNotFoundException : Exception {
    public Guid BoardId { get; }

    public BoardNotFoundException(Guid id)
        : base($"Board with ID '{id}' not found.") {
        BoardId = id;
    }
}

/// <summary>
/// Thrown when the board state is invalid (e.g., invalid dimensions or cells outside bounds).
/// </summary>
public class InvalidBoardStateException : Exception {
    public InvalidBoardStateException(string message)
        : base(message) {
    }
}

/// <summary>
/// Thrown when the maximum number of iterations is exceeded while searching for a final state.
/// </summary>
public class NoFinalStateException : Exception {
    public int MaxIterations { get; }

    public NoFinalStateException(int maxIterations)
        : base($"No final stable state found within {maxIterations} iterations. The board may be cycling or evolving indefinitely.") {
        MaxIterations = maxIterations;
    }
}

/// <summary>
/// Thrown when an invalid number of steps is provided (must be > 0).
/// </summary>
public class InvalidStepsException : Exception {
    public int Steps { get; }

    public InvalidStepsException(int steps)
        : base($"Steps must be greater than 0, but got {steps}.") {
        Steps = steps;
    }
}
