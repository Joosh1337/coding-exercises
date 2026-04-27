import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { BoardGrid } from "../components/BoardGrid";
import { ErrorMessage } from "../components/ErrorMessage";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { useBoard } from "../hooks/useBoard";
import { useUpdateBoard } from "../hooks/useUpdateBoard";

function makeGrid(height: number, width: number): boolean[][] {
  return Array.from({ length: height }, () => new Array(width).fill(false));
}

function liveCellsToGrid(
  width: number,
  height: number,
  liveCells: number[][]
): boolean[][] {
  const grid = makeGrid(height, width);
  for (const [x, y] of liveCells) {
    if (y >= 0 && y < height && x >= 0 && x < width) {
      grid[y][x] = true;
    }
  }
  return grid;
}

export function EditBoardPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: board, isLoading, isError, error } = useBoard(id!);
  const updateBoard = useUpdateBoard();

  const [name, setName] = useState("");
  const [width, setWidth] = useState(10);
  const [height, setHeight] = useState(10);
  const [grid, setGrid] = useState<boolean[][]>(() => makeGrid(10, 10));
  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    if (board && !initialized) {
      setName(board.name ?? "");
      setWidth(board.width);
      setHeight(board.height);
      setGrid(liveCellsToGrid(board.width, board.height, board.liveCells));
      setInitialized(true);
    }
  }, [board, initialized]);

  function handleWidthChange(newWidth: number) {
    setWidth(newWidth);
    setGrid(makeGrid(height, newWidth));
  }

  function handleHeightChange(newHeight: number) {
    setHeight(newHeight);
    setGrid(makeGrid(newHeight, width));
  }

  function toggleCell(y: number, x: number) {
    setGrid((prev) =>
      prev.map((row, ry) =>
        ry === y ? row.map((cell, cx) => (cx === x ? !cell : cell)) : row
      )
    );
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const initialCells = grid.map((row) => row.map((cell) => (cell ? 1 : 0)));
    updateBoard.mutate(
      { id: id!, name, width, height, initialCells },
      { onSuccess: () => navigate(`/boards/${id}`) }
    );
  }

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

  const display = grid.map((row) => row.map((cell) => (cell ? 1 : 0)));

  return (
    <div className="min-h-screen bg-gray-950 text-white p-8">
      <div className="max-w-3xl mx-auto">
        <div className="flex items-center gap-4 mb-2">
          <Link
            to={`/boards/${id}`}
            className="text-gray-400 hover:text-white text-sm transition-colors"
          >
            ← Back
          </Link>
          <h1 className="text-2xl font-bold">Edit Board</h1>
        </div>
        <p className="text-xs text-gray-500 font-mono mb-6">{board.id}</p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-6">
          <label className="flex flex-col gap-1">
            <span className="text-sm text-gray-400">Name</span>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Unnamed board"
              className="w-72 px-3 py-2 bg-gray-800 border border-gray-600 rounded text-white text-sm focus:outline-none focus:border-green-500"
            />
          </label>

          <div className="flex gap-6">
            <label className="flex flex-col gap-1">
              <span className="text-sm text-gray-400">Width</span>
              <input
                type="number"
                min={1}
                max={50}
                value={width}
                onChange={(e) =>
                  handleWidthChange(Math.max(1, Math.min(50, Number(e.target.value))))
                }
                className="w-24 px-3 py-2 bg-gray-800 border border-gray-600 rounded text-white text-sm focus:outline-none focus:border-green-500"
              />
            </label>
            <label className="flex flex-col gap-1">
              <span className="text-sm text-gray-400">Height</span>
              <input
                type="number"
                min={1}
                max={50}
                value={height}
                onChange={(e) =>
                  handleHeightChange(Math.max(1, Math.min(50, Number(e.target.value))))
                }
                className="w-24 px-3 py-2 bg-gray-800 border border-gray-600 rounded text-white text-sm focus:outline-none focus:border-green-500"
              />
            </label>
          </div>

          <div className="flex flex-col gap-2">
            <span className="text-sm text-gray-400">Click cells to toggle them alive</span>
            <BoardGrid display={display} editable onToggle={toggleCell} />
          </div>

          {updateBoard.isError && (
            <ErrorMessage
              message={(updateBoard.error as Error).message ?? "Failed to update board."}
            />
          )}

          <div className="flex items-center gap-3">
            <button
              type="submit"
              disabled={updateBoard.isPending}
              className="flex items-center justify-center gap-2 w-40 px-4 py-2 bg-green-700 hover:bg-green-600 disabled:opacity-50 rounded font-medium transition-colors"
            >
              {updateBoard.isPending && <LoadingSpinner />}
              {updateBoard.isPending ? "Saving…" : "Save Changes"}
            </button>
            <Link
              to={`/boards/${id}`}
              className="px-4 py-2 text-sm text-gray-400 hover:text-white transition-colors"
            >
              Cancel
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}
