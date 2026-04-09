namespace api.Models;

/// <summary>
/// Utility class for detecting when a Game of Life board reaches a stable state or cycles.
/// </summary>
public static class CycleDetection {
    /// <summary>
    /// Determines if a board state is stable (no change from the previous generation).
    /// </summary>
    /// <param name="current">The current board state.</param>
    /// <param name="previous">The previous board state.</param>
    /// <returns>True if the states are identical, false otherwise.</returns>
    public static bool IsStable(BoardState current, BoardState previous) {
        if (current == null || previous == null)
            return false;

        // Two states are stable if they have the same live cells
        return current.LiveCells.SetEquals(previous.LiveCells);
    }

    /// <summary>
    /// Iterates through board generations to detect if the board reaches a stable state
    /// or cycles within the specified iteration limit.
    /// </summary>
    /// <param name="board">The initial board state.</param>
    /// <param name="maxIterations">Maximum number of iterations to compute.</param>
    /// <returns>True if a stable state was detected within maxIterations, false otherwise</returns>
    public static bool HasCycleWithinLimit(Board board, int maxIterations) {
        if (board == null || maxIterations <= 0)
            return false;

        var currentState = new BoardState(board);
        BoardState previousState;

        for (int i = 0; i < maxIterations; i++) {
            previousState = currentState;
            currentState = currentState.GenerateNextStep();

            if (IsStable(currentState, previousState)) {
                return true;
            }
        }

        // Max iterations reached without finding a stable state
        return false;
    }
}
