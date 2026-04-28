import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { BoardGrid } from "../components/BoardGrid";
import { ErrorMessage } from "../components/ErrorMessage";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { useBoard } from "../hooks/useBoard";
import { useFinalState, useStatesAhead } from "../hooks/useBoardStates";
import { gridsEqual, liveCellsToDisplay } from "../utils/grid";

const DEFAULT_PLAY_SPEED = 500;
const MIN_PLAY_SPEED = 100;
const MAX_PLAY_SPEED = 1000;

interface DisplayState {
  generation: number;
  boardDisplay: number[][];
}

export function BoardSimulationPage() {
  const { id } = useParams<{ id: string }>();

  const { data: board, isLoading, isError, error } = useBoard(id!);
  const [history, setHistory] = useState<DisplayState[]>([]);
  const [historyIndex, setHistoryIndex] = useState(0);
  const [jumpSteps, setJumpSteps] = useState(1);
  const [isPlaying, setIsPlaying] = useState(false);
  const [reachedFinalState, setReachedFinalState] = useState(false);
  const [playSpeed, setPlaySpeed] = useState(DEFAULT_PLAY_SPEED);

  const statesAhead = useStatesAhead();
  const finalState = useFinalState();

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

  // Play loop: schedule the next step after each state update.
  useEffect(() => {
    if (!isPlaying) return;

    const current = history[historyIndex];
    if (!current) return;

    const timeout = setTimeout(async () => {
      try {
        const data = await statesAhead.mutateAsync({
          id: id!,
          steps: current.generation + 1,
        });
        const stable = gridsEqual(current.boardDisplay, data.boardDisplay);
        setHistory((h) => [
          ...h.slice(0, historyIndex + 1),
          { generation: data.generation, boardDisplay: data.boardDisplay },
        ]);
        setHistoryIndex((i) => i + 1);
        if (stable) {
          setIsPlaying(false);
          setReachedFinalState(true);
        }
      } catch {
        setIsPlaying(false);
      }
    }, playSpeed);

    return () => clearTimeout(timeout);
  }, [isPlaying, historyIndex, history, id, playSpeed, statesAhead.mutateAsync]);

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

  function handleRevertToInitial() {
    setIsPlaying(false);
    setReachedFinalState(false);
    setHistoryIndex(0);
  }

  function handlePrev() {
    setReachedFinalState(false);
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
      onSuccess: (data) => {
        pushState(
          { generation: data.generation, boardDisplay: data.boardDisplay },
          capturedIndex
        );
        setReachedFinalState(true);
      },
    });
  }

  function handlePlay() {
    statesAhead.reset();
    setReachedFinalState(false);
    setIsPlaying(true);
  }

  function handleStop() {
    setIsPlaying(false);
  }

  function handleRestart() {
    statesAhead.reset();
    setReachedFinalState(false);
    setHistoryIndex(0);
    setIsPlaying(true);
  }

  return (
    <div className="min-h-screen bg-gray-950 text-white p-8">
      <div className="max-w-3xl mx-auto">
        <div className="flex items-center gap-4 mb-2">
          <Link to="/" className="text-gray-400 hover:text-white text-sm transition-colors">
            ← Back
          </Link>
          <h1 className="text-2xl font-bold">
            {board.name || <span className="text-gray-500 italic font-normal">Unnamed board</span>}
          </h1>
          <Link
            to={`/boards/${board.id}/edit`}
            className="ml-auto px-3 py-1.5 text-sm bg-gray-700 hover:bg-gray-600 text-white rounded transition-colors"
          >
            Edit
          </Link>
        </div>
        {/* Grid */}
        <div className="mb-6 flex justify-center">
          <div className="flex flex-col items-center gap-2">
            <div className="text-sm text-gray-400">{board.width} × {board.height}</div>
            {display.length > 0 && <BoardGrid display={display} />}
          </div>
        </div>

        {/* Primary navigation */}
        <div className="flex items-center justify-center gap-3 mb-4">
          <button
            onClick={handleRevertToInitial}
            disabled={busy || historyIndex === 0}
            title="Return to generation 0"
            className="px-3 py-2.5 bg-gray-700 hover:bg-gray-600 disabled:opacity-30 rounded-lg text-sm font-medium transition-colors"
          >
            ⏮ Start
          </button>

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

          <button
            onClick={handleFinal}
            disabled={busy || reachedFinalState}
            title="Find final state"
            className="px-3 py-2.5 bg-gray-700 hover:bg-gray-600 disabled:opacity-30 rounded-lg text-sm font-medium transition-colors"
          >
            End ⏭
          </button>
        </div>

        {/* Play / Stop / Restart + speed control */}
        <div className="flex flex-col items-center gap-3 mb-5">
          {reachedFinalState ? (
            <button
              onClick={handleRestart}
              className="px-8 py-2.5 bg-teal-700 hover:bg-teal-600 rounded-lg text-sm font-semibold tracking-wide transition-colors"
            >
              ↺ Restart
            </button>
          ) : isPlaying ? (
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

          <div className="flex items-center gap-3">
            <label className="text-sm text-gray-400 whitespace-nowrap">Interval</label>
            <input
              type="range"
              min={MIN_PLAY_SPEED}
              max={MAX_PLAY_SPEED}
              step={50}
              value={playSpeed}
              onChange={(e) => setPlaySpeed(Number(e.target.value))}
              className="w-32 accent-green-400"
            />
            <span className="text-sm text-gray-300 w-14 tabular-nums">{playSpeed}ms</span>
          </div>
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
        </div>

        {/* Errors */}
        <div className="mt-4 flex flex-col gap-2">
          {statesAhead.isError && (
            <ErrorMessage message={(statesAhead.error as Error).message} />
          )}
          {finalState.isError && (
            <ErrorMessage message={(finalState.error as Error).message} />
          )}
        </div>
      </div>
    </div>
  );
}
