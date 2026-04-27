import { Link } from "react-router-dom";
import type { BoardResponse } from "../types/api";

interface BoardCardProps {
  board: BoardResponse;
  onDelete: () => void;
  isDeleting: boolean;
}

export function BoardCard({ board, onDelete, isDeleting }: BoardCardProps) {
  return (
    <div className="flex items-center justify-between p-4 bg-gray-800 rounded-lg border border-gray-700">
      <div className="flex flex-col gap-1">
        <span className="text-xs text-gray-400 font-mono">{board.id}</span>
        <span className="text-sm text-gray-200">
          {board.width} × {board.height} &mdash; {board.liveCells.length} live cells
        </span>
      </div>
      <div className="flex items-center gap-2">
        <Link
          to={`/boards/${board.id}`}
          className="px-3 py-1.5 text-sm bg-green-700 hover:bg-green-600 text-white rounded transition-colors"
        >
          Simulate
        </Link>
        <button
          onClick={onDelete}
          disabled={isDeleting}
          className="px-3 py-1.5 text-sm bg-red-800 hover:bg-red-700 disabled:opacity-50 text-white rounded transition-colors"
        >
          {isDeleting ? "Deleting…" : "Delete"}
        </button>
      </div>
    </div>
  );
}
