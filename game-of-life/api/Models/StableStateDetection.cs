using api.Exceptions;

namespace api.Models;

/// <summary>
/// Utility class for detecting when a Game of Life board reaches a stable state or cycles.
/// </summary>
public static class StableStateDetection {
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
    /// Computes and returns the final stable state of the board.
    /// Iterates up to the configured maximum iterations to find a stable state.
    /// </summary>
    /// <param name="board">The initial board state.</param>
    /// <param name="maxIterations">Maximum number of iterations to compute.</param>
    /// <returns>
    ///     The final BoardState, if detected within maxIterations.
    ///     Throws NoFinalStateException otherwise
    /// </returns>
    public static BoardState GetStableStateWithinLimit(Board board, int maxIterations) {
        var currentState = new BoardState(board);

        for (int i = 0; i < maxIterations; i++) {
            var nextState = currentState.GenerateNextStep();

            // Check if we've reached a stable state (no change from previous generation)
            if (IsStable(nextState, currentState))
                return nextState;

            currentState = nextState;
        }

        // If we exit the loop without finding a stable state, throw an exception
        throw new NoFinalStateException(maxIterations);
    }
}
