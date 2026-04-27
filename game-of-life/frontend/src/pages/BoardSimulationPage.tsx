import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { fetchStatesAhead } from "../api/client";
import { BoardGrid } from "../components/BoardGrid";
import { ErrorMessage } from "../components/ErrorMessage";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { useBoard } from "../hooks/useBoard";
import { useFinalState, useStatesAhead } from "../hooks/useBoardStates";
import { useDeleteBoard } from "../hooks/useDeleteBoard";

interface DisplayState {
  generation: number;
  boardDisplay: number[][];
}

function liveCellsToDisplay(
  width: number,
  height: number,
  liveCells: number[][]
): number[][] {
  const grid = Array.from({ length: height }, () => new Array(width).fill(0));
  for (const [x, y] of liveCells) {
    if (y >= 0 && y < height && x >= 0 && x < width) {
      grid[y][x] = 1;
    }
  }
  return grid;
}

function gridsEqual(a: number[][], b: number[][]): boolean {
  return (
    a.length === b.length &&
    a.every(
      (row, y) =>
        row.length === b[y].length && row.every((cell, x) => cell === b[y][x])
    )
  );
}

export function BoardSimulationPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: board, isLoading, isError, error } = useBoard(id!);
  const [history, setHistory] = useState<DisplayState[]>([]);
  const [historyIndex, setHistoryIndex] = useState(0);
  const [jumpSteps, setJumpSteps] = useState(1);
  const [isPlaying, setIsPlaying] = useState(false);
  const [playError, setPlayError] = useState<string | null>(null);

  const statesAhead = useStatesAhead();
  const finalState = useFinalState();
  const deleteBoard = useDeleteBoard();

  // Seed history with generation 0 once board data arrives.
  useEffect(() => {
    if (board && history.length === 0) {
      setHistory([
        {
          generation: 0,
          boardDisplay: liveCellsToDisplay(board.width, board.height, board.liveCells),
        },
      ]);
      setHistoryIndex(0);
    }
  }, [board, history.length]);

  // Play loop: schedule the next step 50ms after each state update.
  // Re-runs whenever isPlaying, historyIndex, or history changes.
  useEffect(() => {
    if (!isPlaying) return;

    const current = history[historyIndex];
    if (!current) return;

    const timeout = setTimeout(async () => {
      try {
        const data = await fetchStatesAhead(id!, current.generation + 1);
        const stable = gridsEqual(current.boardDisplay, data.boardDisplay);
        setHistory((h) => [
          ...h.slice(0, historyIndex + 1),
          { generation: data.generation, boardDisplay: data.boardDisplay },
        ]);
        setHistoryIndex((i) => i + 1);
        if (stable) setIsPlaying(false);
      } catch (err) {
        setIsPlaying(false);
        setPlayError((err as Error).message);
      }
    }, 50);

    return () => clearTimeout(timeout);
  }, [isPlaying, historyIndex, history, id]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-950 flex items-center justify-center">
        <LoadingSpinner />
      </div>
    );
  }

  if (isError || !board) {
    return (
      <div className="min-h-screen bg-gray-950 text-white p-8">
        <ErrorMessage message={(error as Error)?.message ?? "Board not found."} />
        <Link to="/" className="mt-4 inline-block text-gray-400 hover:text-white text-sm">
          ← Back to boards
        </Link>
      </div>
    );
  }

  const current = history[historyIndex];
  const generation = current?.generation ?? 0;
  const display = current?.boardDisplay ?? [];

  const manualPending = statesAhead.isPending || finalState.isPending;
  const busy = manualPending || isPlaying;

  function pushState(newState: DisplayState, fromIndex: number) {
    setHistory((h) => [...h.slice(0, fromIndex + 1), newState]);
    setHistoryIndex(fromIndex + 1);
  }

  function handlePrev() {
    if (historyIndex > 0) setHistoryIndex((i) => i - 1);
  }

  function handleNext() {
    const nextIndex = historyIndex + 1;
    if (nextIndex < history.length) {
      setHistoryIndex(nextIndex);
      return;
    }
    const capturedIndex = historyIndex;
    statesAhead.mutate(
      { id: id!, steps: generation + 1 },
      {
        onSuccess: (data) =>
          pushState(
            { generation: data.generation, boardDisplay: data.boardDisplay },
            capturedIndex
          ),
      }
    );
  }

  function handleJump() {
    const capturedIndex = historyIndex;
    statesAhead.mutate(
      { id: id!, steps: generation + jumpSteps },
      {
        onSuccess: (data) =>
          pushState(
            { generation: data.generation, boardDisplay: data.boardDisplay },
            capturedIndex
          ),
      }
    );
  }

  function handleFinal() {
    const capturedIndex = historyIndex;
    finalState.mutate(id!, {
      onSuccess: (data) =>
        pushState(
          { generation: data.generation, boardDisplay: data.boardDisplay },
          capturedIndex
        ),
    });
  }

  function handlePlay() {
    setPlayError(null);
    setIsPlaying(true);
  }

  function handleStop() {
    setIsPlaying(false);
  }

  function handleDelete() {
    setIsPlaying(false);
    deleteBoard.mutate(id!, { onSuccess: () => navigate("/") });
  }

  return (
    <div className="min-h-screen bg-gray-950 text-white p-8">
      <div className="max-w-3xl mx-auto">
        <div className="flex items-center gap-4 mb-2">
          <Link to="/" className="text-gray-400 hover:text-white text-sm transition-colors">
            ← Back
          </Link>
          <h1 className="text-2xl font-bold">Board Simulation</h1>
        </div>
        <p className="text-xs text-gray-500 font-mono mb-4">{board.id}</p>

        {/* Grid */}
        <div className="mb-6 flex justify-center">
          <div className="flex flex-col items-center gap-2">
            <div className="text-sm text-gray-400">
              {board.width} × {board.height}
            </div>
            {display.length > 0 && <BoardGrid display={display} />}
          </div>
        </div>

        {/* Primary navigation */}
        <div className="flex items-center justify-center gap-4 mb-4">
          <button
            onClick={handlePrev}
            disabled={busy || historyIndex === 0}
            className="px-5 py-2.5 bg-gray-700 hover:bg-gray-600 disabled:opacity-30 rounded-lg text-sm font-medium transition-colors"
          >
            ← Previous
          </button>

          <div className="flex flex-col items-center min-w-[100px]">
            <span className="text-3xl font-bold text-green-400">{generation}</span>
            <span className="text-xs text-gray-500 uppercase tracking-wide">generation</span>
          </div>

          <button
            onClick={handleNext}
            disabled={busy}
            className="flex items-center gap-2 px-5 py-2.5 bg-gray-700 hover:bg-gray-600 disabled:opacity-30 rounded-lg text-sm font-medium transition-colors"
          >
            {statesAhead.isPending && historyIndex >= history.length - 1 && (
              <LoadingSpinner />
            )}
            Next →
          </button>
        </div>

        {/* Play / Stop */}
        <div className="flex justify-center mb-5">
          {isPlaying ? (
            <button
              onClick={handleStop}
              className="px-8 py-2.5 bg-amber-600 hover:bg-amber-500 rounded-lg text-sm font-semibold tracking-wide transition-colors"
            >
              ⏹ Stop
            </button>
          ) : (
            <button
              onClick={handlePlay}
              disabled={manualPending}
              className="px-8 py-2.5 bg-green-700 hover:bg-green-600 disabled:opacity-50 rounded-lg text-sm font-semibold tracking-wide transition-colors"
            >
              ▶ Play
            </button>
          )}
        </div>

        {/* Jump controls */}
        <div className="flex flex-wrap items-center justify-center gap-3 mb-3">
          <div className="flex items-center gap-2">
            <label className="text-sm text-gray-400 whitespace-nowrap">Advance</label>
            <input
              type="number"
              min={1}
              value={jumpSteps}
              onChange={(e) => setJumpSteps(Math.max(1, Number(e.target.value)))}
              disabled={busy}
              className="w-20 px-3 py-2 bg-gray-800 border border-gray-600 rounded text-white text-sm focus:outline-none focus:border-blue-500 disabled:opacity-50"
            />
            <label className="text-sm text-gray-400 whitespace-nowrap">steps</label>
            <button
              onClick={handleJump}
              disabled={busy}
              className="flex items-center gap-2 px-4 py-2 bg-blue-700 hover:bg-blue-600 disabled:opacity-50 rounded text-sm font-medium transition-colors"
            >
              {statesAhead.isPending && <LoadingSpinner />}
              Jump
            </button>
          </div>

          <button
            onClick={handleFinal}
            disabled={busy}
            className="flex items-center gap-2 px-4 py-2 bg-purple-700 hover:bg-purple-600 disabled:opacity-50 rounded text-sm font-medium transition-colors"
          >
            {finalState.isPending && <LoadingSpinner />}
            Find Final State
          </button>
        </div>

        {/* Danger zone */}
        <div className="flex justify-center mt-6">
          <button
            onClick={handleDelete}
            disabled={deleteBoard.isPending}
            className="px-4 py-2 bg-red-900 hover:bg-red-800 disabled:opacity-50 rounded text-sm text-red-300 transition-colors"
          >
            {deleteBoard.isPending ? "Deleting…" : "Delete Board"}
          </button>
        </div>

        {/* Errors */}
        <div className="mt-4 flex flex-col gap-2">
          {playError && <ErrorMessage message={playError} />}
          {statesAhead.isError && (
            <ErrorMessage message={(statesAhead.error as Error).message} />
          )}
          {finalState.isError && (
            <ErrorMessage message={(finalState.error as Error).message} />
          )}
          {deleteBoard.isError && (
            <ErrorMessage message={(deleteBoard.error as Error).message} />
          )}
        </div>
      </div>
    </div>
  );
}
