# Rules of the Game of Life

1. Any live cell with fewer than two live neighbors dies, as if by underpopulation.
2. Any live cell with two or three live neighbors lives on to the next generation.
3. Any live cell with more than three live neighbors dies, as if by overpopulation.
4. Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction.

## Basic Rules Logic

public BoardState GenerateNextStep() {
   var neighborCounts = new Dictionary<(int x, int y), int>();
   var nextLiveCells = new HashSet<(int x, int y)>();
   
   // Count neighbors of each live cell
   foreach (var liveCell in this.LiveCells) {
      // loop through any surrounding neighbors
      for (int dx = -1; dx < 2; dx++) 
      for (int dy = -1; dy < 2; dy++) {
         /*
            liveCell does not get evaluated, only neighbors
            You might think: "Doesn't this mean that the liveCell is never evaluated, unless by a neighbor?"
            You'd be right, but that's ok it's because if it has no neighbors, it would die anyway
         */
         if (dx == 0 && dy == 0)
            continue;

         int neighborX = liveCell.x + dx;
         int neighborY = liveCell.y + dy;

         // skip invalid indexes
         if (neighborX < 0 || neighborX >= this.Width || neighborY < 0 || neighborY >= this.Height)
            continue;

         var neighbor = (neighborX, neighborY);
         neighborCounts.TryGetValue(neighbor, out int count);
         neighborCounts[neighbor] = count + 1;
      }
   }

   // Add any living cells for next generation
   foreach (var (cell, count) in neighborCounts) {
      var isAlive = this.LiveCells.Contains(cell);
      if (!isAlive && count == 3) {
         nextLiveCells.Add(cell);
      } else if (isAlive && (count == 2 || count == 3)) {
         nextLiveCells.Add(cell);
      }
   }

   return new BoardState() {
      Generation = this.Generation + 1,
      Width = this.Width,
      Height = this.Height,
      LiveCells = nextLiveCells
   }
}

# Endpoints
1. POST     /boards
2. GET      /boards/{id}
3. GET      /boards/{id}/states/next
4. GET      /boards/{id}/states?steps=N
5. GET      /boards/{id}/states/final
6. DELETE   /boards/{id}

# Classes   
1. Board (persisted in DB)
   1. System.Guid ID
   2. int Width
   3. int Height
   4. HashSet<(int x, int y)> LiveCells
2. BoardState (computed in runtime)
   1. int Generation
   2. int Width
   3. int Height
   4. HashSet<(int x, int y)> LiveCells
   5. GenerateNextStep() -> BoardState
   6. GenerateBoardArray() -> int[,]

# Functions
1. UploadBoard(int width, int height, int[,] cells) -> Guid
2. GetBoard(Guid id) -> Board

# Test Cases
1. Uploading a board works (test with 0 steps)
2. Uploading the same board twice will create two boards with different guids
4. Next state for a board works
5. Next N states for a board works
   1. Test negative numbers
   2. Test high numbers
      1. On board with final state
         1. state should be the same at 9998 and 9999
      2. On board with no reasonably reachable final state
         1. state should be the different at 9998 and 9999
6. Final board state returns
7. Final board state cannot be reached, throws an error
8. Still life stays stable - covers rules 2 and an unspoken rule that dead cells without exactly 3 live neighbors stay dead
   1. 11 -> 11
      11    11
   2. 00 -> 00
      00    00
9. Oscillator cycles - covers rules 1 & 4
   1. 010    000    010
      010 -> 111 -> 010
      010    000    010
10. Overpopulation test
   1. 111    101
      111 -> 000
      111    101