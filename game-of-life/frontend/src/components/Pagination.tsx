interface PaginationProps {
  page: number;
  pageSize: number;
  itemCount: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, pageSize, itemCount, onPageChange }: PaginationProps) {
  const isFirst = page === 1;
  const isLast = itemCount < pageSize;

  return (
    <div className="flex items-center gap-3 mt-4">
      <button
        onClick={() => onPageChange(page - 1)}
        disabled={isFirst}
        className="px-3 py-1.5 text-sm bg-gray-700 hover:bg-gray-600 disabled:opacity-40 text-white rounded transition-colors"
      >
        Previous
      </button>
      <span className="text-sm text-gray-400">Page {page}</span>
      <button
        onClick={() => onPageChange(page + 1)}
        disabled={isLast}
        className="px-3 py-1.5 text-sm bg-gray-700 hover:bg-gray-600 disabled:opacity-40 text-white rounded transition-colors"
      >
        Next
      </button>
    </div>
  );
}
