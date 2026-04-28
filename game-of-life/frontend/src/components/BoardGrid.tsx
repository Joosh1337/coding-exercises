import { useMemo } from "react";

interface BoardGridProps {
  display: number[][];
  editable?: boolean;
  onToggle?: (y: number, x: number) => void;
}

export function BoardGrid({ display, editable = false, onToggle }: BoardGridProps) {
  const rows = display.length;
  const cols = rows > 0 ? display[0].length : 0;
  const cellSize = useMemo(
    () => Math.max(6, Math.min(48, Math.floor(560 / Math.max(rows, cols, 1)))),
    [rows, cols]
  );

  return (
    <div
      className="bg-gray-600 inline-block"
      style={{
        display: "grid",
        gridTemplateColumns: `repeat(${cols}, ${cellSize}px)`,
        gap: "1px",
        padding: "1px",
      }}
    >
      {display.map((row, y) =>
        row.map((cell, x) => (
          <div
            key={`${y}-${x}`}
            style={{ width: cellSize, height: cellSize }}
            className={`${cell ? "bg-green-400" : "bg-gray-900"} ${
              editable ? "cursor-pointer hover:opacity-70" : ""
            }`}
            onClick={() => editable && onToggle?.(y, x)}
          />
        ))
      )}
    </div>
  );
}
