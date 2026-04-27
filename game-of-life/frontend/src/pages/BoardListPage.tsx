import { useState } from "react";
import { Link } from "react-router-dom";
import { BoardCard } from "../components/BoardCard";
import { ErrorMessage } from "../components/ErrorMessage";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { Pagination } from "../components/Pagination";
import { useBoards } from "../hooks/useBoards";
import { useDeleteBoard } from "../hooks/useDeleteBoard";

const PAGE_SIZE = 10;

export function BoardListPage() {
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, error } = useBoards(page, PAGE_SIZE);
  const deleteBoard = useDeleteBoard();

  return (
    <div className="min-h-screen bg-gray-950 text-white p-8">
      <div className="max-w-3xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold">Game of Life</h1>
          <Link
            to="/boards/new"
            className="px-4 py-2 bg-green-700 hover:bg-green-600 rounded text-sm font-medium transition-colors"
          >
            + New Board
          </Link>
        </div>

        {isLoading && (
          <div className="flex justify-center py-12">
            <LoadingSpinner />
          </div>
        )}

        {isError && (
          <ErrorMessage message={(error as Error).message ?? "Failed to load boards."} />
        )}

        {data && data.length === 0 && (
          <p className="text-gray-500 text-center py-12">
            No boards yet. Create one to get started.
          </p>
        )}

        {data && data.length > 0 && (
          <div className="flex flex-col gap-3">
            {data.map((board) => (
              <BoardCard
                key={board.id}
                board={board}
                onDelete={() => deleteBoard.mutate(board.id)}
                isDeleting={
                  deleteBoard.isPending && deleteBoard.variables === board.id
                }
              />
            ))}
          </div>
        )}

        {data && (
          <Pagination
            page={page}
            pageSize={PAGE_SIZE}
            itemCount={data.length}
            onPageChange={setPage}
          />
        )}
      </div>
    </div>
  );
}
