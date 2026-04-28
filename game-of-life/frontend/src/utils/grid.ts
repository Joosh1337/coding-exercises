export function makeGrid(height: number, width: number): boolean[][] {
  return Array.from({ length: height }, () => new Array(width).fill(false));
}

export function liveCellsToDisplay(
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

export function liveCellsToGrid(
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

export function gridsEqual(a: number[][], b: number[][]): boolean {
  return (
    a.length === b.length &&
    a.every(
      (row, y) =>
        row.length === b[y].length && row.every((cell, x) => cell === b[y][x])
    )
  );
}
