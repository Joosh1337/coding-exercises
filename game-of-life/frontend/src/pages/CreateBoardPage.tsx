import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { BoardGrid } from "../components/BoardGrid";
import { ErrorMessage } from "../components/ErrorMessage";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { useCreateBoard } from "../hooks/useCreateBoard";
import { getErrorMessage } from "../utils/error";
import { makeGrid } from "../utils/grid";

export function CreateBoardPage() {
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [width, setWidth] = useState(10);
  const [height, setHeight] = useState(10);
  const [grid, setGrid] = useState<boolean[][]>(() => makeGrid(10, 10));
  const createBoard = useCreateBoard();

  useEffect(() => {
    setGrid(makeGrid(height, width));
  }, [width, height]);

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
    createBoard.mutate(
      { name, width, height, initialCells },
      {
        onSuccess: (data) => navigate(`/boards/${data.id}`),
      }
    );
  }

  const display = grid.map((row) => row.map((cell) => (cell ? 1 : 0)));

  return (
    <div className="min-h-screen bg-gray-950 text-white p-8">
      <div className="max-w-3xl mx-auto">
        <div className="flex items-center gap-4 mb-6">
          <Link to="/" className="text-gray-400 hover:text-white text-sm transition-colors">
            ← Back
          </Link>
          <h1 className="text-2xl font-bold">New Board</h1>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-6">
          <label className="flex flex-col gap-1">
            <span className="text-sm text-gray-400">Name</span>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="My board"
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
                onChange={(e) => { const v = e.currentTarget.valueAsNumber; if (!isNaN(v)) setWidth(Math.max(1, Math.min(50, v))); }}
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
                onChange={(e) => { const v = e.currentTarget.valueAsNumber; if (!isNaN(v)) setHeight(Math.max(1, Math.min(50, v))); }}
                className="w-24 px-3 py-2 bg-gray-800 border border-gray-600 rounded text-white text-sm focus:outline-none focus:border-green-500"
              />
            </label>
          </div>

          <div className="flex flex-col gap-2">
            <span className="text-sm text-gray-400">Click cells to toggle them alive</span>
            <BoardGrid display={display} editable onToggle={toggleCell} />
          </div>

          {createBoard.isError && (
            <ErrorMessage
              message={getErrorMessage(createBoard.error, "Failed to create board.")}
            />
          )}

          <button
            type="submit"
            disabled={createBoard.isPending || !name.trim()}
            className="flex items-center justify-center gap-2 w-40 px-4 py-2 bg-green-700 hover:bg-green-600 disabled:opacity-50 rounded font-medium transition-colors"
          >
            {createBoard.isPending && <LoadingSpinner />}
            {createBoard.isPending ? "Creating…" : "Create Board"}
          </button>
        </form>
      </div>
    </div>
  );
}
