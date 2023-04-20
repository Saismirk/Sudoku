using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

namespace Sudoku {
    public enum Difficulty {
        Easy,
        Medium,
        Hard,
        Expert
    }

    public class SudokuBoard {
        public       Cell[] Cells                  { get; private set; }
        public       int[]  HighlightedCellIndices { get; private set; } = new int[BOARD_SIZE * 3 - 3];
        public       int[]  Solution               { get; private set; }
        public const int    BOARD_SIZE          = 9;
        public const int    SUB_BOARD_SIZE      = 3;
        public const int    CELL_COUNT          = BOARD_SIZE * BOARD_SIZE;
        public const int    MIN_HINT_AMOUNT     = 25;
        public const int    MAX_HINT_AMOUNT     = CELL_COUNT - 1;
        public const int    MAX_RECURSION_DEPTH = CELL_COUNT;

        int    cellsToRemove = 0;
        int    cellsBuffer   = 0;
        int    iterations    = 0;
        Cell[] randomCellIndices;

        public SudokuBoard() {
            Cells = new Cell[CELL_COUNT];
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var column = 0; column < BOARD_SIZE; column++) {
                    var index = row * BOARD_SIZE + column;
                    var block = row / SUB_BOARD_SIZE * SUB_BOARD_SIZE + column / SUB_BOARD_SIZE;
                    Cells[index] = new Cell(0, new BoardPosition(row, column, block));
                }
            }

            PopulateBoard();
            Solution = Cells.Select(c => c.value).ToArray();
        }

        public SudokuBoard(IReadOnlyList<int> values) {
            Cells = new Cell[CELL_COUNT];
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var column = 0; column < BOARD_SIZE; column++) {
                    var index = row * BOARD_SIZE + column;
                    var block = row / SUB_BOARD_SIZE * SUB_BOARD_SIZE + column / SUB_BOARD_SIZE;
                    Cells[index] = new Cell(values[index], new BoardPosition(row, column, block));
                }
            }

            Solution = Cells.Select(c => c.value).ToArray();
        }

        void PopulateBoard() {
            var iterations = 0;
            if (!PopulateCell(0, ref iterations)) {
                Debug.LogError("Failed to generate board");
            }
        }

        public bool SolveBoard() {
            var firstCell  = GetNextEmptyCellIndex(0);
            var iterations = 0;
            if (firstCell >= 0) {
                return PopulateCell(firstCell, ref iterations);
            }

            Debug.Log("Board is already solved");
            return true;
        }

        int SolveCell(int cellIndex, ref int solutions, int maxSolutions = 10) {
            var possibleNumbers = GetShuffledPossibleNumbers(cellIndex);

            if (cellIndex < 0) {
                solutions++;
                return solutions;
            }

            var nextCell = GetNextEmptyCellIndex(cellIndex);
            foreach (var num in possibleNumbers) {
                SetCellValue(cellIndex, num);
                if (SolveCell(nextCell, ref solutions) >= maxSolutions) {
                    return solutions;
                }

                SetCellValue(cellIndex, 0);
            }

            return solutions;
        }

        int[] GetShuffledPossibleNumbers(int cellIndex) {
            var numbers = Enumerable.Range(1, 9)
                                    .Where(number => IsCellValid(cellIndex, number)).ToArray();
            SetCellValue(cellIndex, 0);
            Shuffle(numbers);
            return numbers;
        }

        bool PopulateCell(int cellIndex, ref int iterations) {
            iterations++;
            if (iterations > MAX_RECURSION_DEPTH) {
                iterations = 0;
                return false;
            }

            if (cellIndex is >= CELL_COUNT or < 0) {
                iterations = 0;
                return true;
            }

            var numbers  = GetShuffledPossibleNumbers(cellIndex);
            var nextCell = GetNextEmptyCellIndex(cellIndex);
            if (nextCell < -1) {
                SetCellValue(cellIndex, numbers.FirstOrDefault());
                return true;
            }

            foreach (var num in numbers) {
                SetCellValue(cellIndex, num);
                if (PopulateCell(nextCell, ref iterations)) {
                    return true;
                }

                SetCellValue(cellIndex, 0);
            }

            return false;
        }

        void SetCellValue(int cellIndex, int value) {
            if (cellIndex < 0) {
                return;
            }

            var c = Cells[cellIndex];
            c.value = value;
            Cells[cellIndex] = c;
        }

        public bool IsCurrentBoardSolution() => Cells.Select((cell, i) => (cell, i)).All(c => c.cell.value == Solution[c.i]);

        void SetCellValues(IReadOnlyList<int> values) {
            for (var i = 0; i < values.Count; i++) {
                var c = Cells[i];
                c.value = values[i];
                Cells[i] = c;
            }
        }

        public int GetNumberOfSolutions() {
            var solutions = 0;
            var firstCell = GetNextEmptyCellIndex(0);
            var valueList = Cells.Select(cell => cell.value).ToList();
            solutions = SolveCell(firstCell, ref solutions);
            SetCellValues(valueList);
            Debug.Log($"Found {solutions} solutions");
            return solutions;
        }

        public bool HasUniqueSolution() {
            var valueList = Cells.Select(cell => cell.value).ToList();
            var solutions = 0;
            var firstCell = GetNextEmptyCellIndex(0);
            if (firstCell >= 0) {
                solutions = SolveCell(firstCell, ref solutions, 2);
            } else {
                Debug.Log("Board is already solved");
                return true;
            }

            SetCellValues(valueList);
            return solutions == 1;
        }

        static void Shuffle<T>(IList<T> array) {
            var rand = new Random();
            rand.InitState((uint)Time.frameCount);
            for (var i = array.Count - 1; i > 0; i--) {
                var j = rand.NextInt(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        public void ApplyKnownSolution() => SetCellValues(Solution);

        public async UniTask RemoveCells(Difficulty difficulty) {
            iterations = 0;
            ApplyKnownSolution();
            cellsToRemove = GetDifficultyHoleAmount(difficulty);
            cellsBuffer = cellsToRemove;
            Debug.Log($"Removing {cellsToRemove} cells for difficulty {difficulty}");
            await UniTask.Yield();
            if (!RemoveCell()) {
                Debug.LogError("Failed to remove cells");
            }

            if (!HasUniqueSolution()) Debug.LogError("Board is still not solvable");
        }

        bool RemoveCell() {
            if (cellsToRemove <= 0) {
                if (!HasUniqueSolution()) {
                    Debug.LogError("Removed last cell but board is still not solvable");
                    return false;
                }

                Debug.Log($"Removed last cell and found unique solution in {iterations} steps");
                return true;
            }

            var nonEmptyCells = Cells.Where(c => c.value != 0)
                                     .Select(cell => cell.Index)
                                     .ToList();
            if (nonEmptyCells.Count == 0) {
                Debug.LogWarning("No more cells to remove, backtracking");
                return false;
            }

            Shuffle(nonEmptyCells);

            foreach (var index in nonEmptyCells) {
                if (Cells[index].value == 0) continue;
                iterations++;
                SetCellValue(index, 0);

                if (!HasUniqueSolution()) {
                    SetCellValue(index, Solution[index]);
                    continue;
                }

                cellsToRemove--;
                if (RemoveCell()) {
                    return true;
                }

                SetCellValue(index, Solution[index]);
                cellsToRemove++;
                if (cellsToRemove < cellsBuffer - 1) return false;
            }

            return false;
        }

        static int GetNextCellIndex(int cellIndex) {
            if (cellIndex is >= CELL_COUNT or < 0) {
                return -1;
            }

            var nextIndex = cellIndex + 1;
            return nextIndex >= CELL_COUNT ? -1 : nextIndex;
        }

        public ReadOnlySpan<int> GetValidationCellIndices(int cellIndex) {
            if ((uint)cellIndex >= CELL_COUNT) {
                return Array.Empty<int>();
            }

            var cell  = Cells[cellIndex];
            var index = 0;

            var rowIndexStart = cell.Position.Row * BOARD_SIZE;
            for (var i = rowIndexStart; i < rowIndexStart + BOARD_SIZE; i++) {
                if (i != cellIndex) {
                    HighlightedCellIndices[index++] = i;
                }
            }

            var columnIndexStart = cell.Position.Column;
            for (var i = columnIndexStart; i < CELL_COUNT; i += BOARD_SIZE) {
                if (i != cellIndex) {
                    HighlightedCellIndices[index++] = i;
                }
            }

            var blockRowIndexStart    = (cell.Position.Row / SUB_BOARD_SIZE) * SUB_BOARD_SIZE;
            var blockColumnIndexStart = (cell.Position.Column / SUB_BOARD_SIZE) * SUB_BOARD_SIZE;
            for (var i = blockRowIndexStart; i < blockRowIndexStart + SUB_BOARD_SIZE; i++) {
                for (var j = blockColumnIndexStart; j < blockColumnIndexStart + SUB_BOARD_SIZE; j++) {
                    var blockIndex = i * BOARD_SIZE + j;
                    if (blockIndex != cellIndex) {
                        HighlightedCellIndices[index++] = blockIndex;
                    }
                }
            }

            return HighlightedCellIndices.AsSpan(0, index);
        }

        IEnumerable<int> GetCellRowIndices(Cell cell) => Cells.Where(c => c.Position.Row == cell.Position.Row)
                                                              .Select(c => c.Index);

        IEnumerable<int> GetCellColumnIndices(Cell cell) => Cells.Where(c => c.Position.Column == cell.Position.Column)
                                                                 .Select(c => c.Index);

        IEnumerable<int> GetCellBlockIndices(Cell cell) => Cells.Where(c => c.Position.Block == cell.Position.Block)
                                                                .Select(c => c.Index);

        int GetNextEmptyCellIndex(int cellIndex) {
            if (cellIndex is >= CELL_COUNT or < 0) {
                return -1;
            }

            var nextCellIndex = GetNextCellIndex(cellIndex);
            if (nextCellIndex < 0) {
                return -1;
            }

            var nextCell = Cells[nextCellIndex];
            while (nextCell.value != 0) {
                nextCellIndex = GetNextCellIndex(nextCellIndex);
                if (nextCellIndex < 0) {
                    return -1;
                }

                nextCell = Cells[nextCellIndex];
            }

            return nextCellIndex;
        }

        bool IsCellValid(int cellIndex, int value) {
            if (cellIndex is < 0 or >= CELL_COUNT) {
                return false;
            }

            var cellData = Cells[cellIndex];
            cellData.value = value;
            return IsCellValid(cellData);
        }

        public bool IsCellValid(Cell cell) {
            if (cell.value == 0) {
                return false;
            }

            var row = cell.Position.Row;
            var col = cell.Position.Column;
            var num = cell.value;
            for (var i = 0; i < BOARD_SIZE; i++) {
                if (i != col && GetCell(row, i).value == num) {
                    return false;
                }

                if (i != row && GetCell(i, col).value == num) {
                    return false;
                }
            }

            var subRow = row / SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            var subCol = col / SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            for (var i = subRow; i < subRow + SUB_BOARD_SIZE; i++) {
                for (var j = subCol; j < subCol + SUB_BOARD_SIZE; j++) {
                    if ((i != row || j != col) && GetCell(i, j).value == num) {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsBoardValid() {
            var valid = Cells?.All(IsCellValid) == true;
            if (!valid) Debug.LogWarning("Board is not valid\n" + this);
            return valid;
        }

        public static int GetDifficultyHoleAmount(Difficulty difficulty) => difficulty switch {
            Difficulty.Easy   => CELL_COUNT - MIN_HINT_AMOUNT - 15,
            Difficulty.Medium => CELL_COUNT - MIN_HINT_AMOUNT - 10,
            Difficulty.Hard   => CELL_COUNT - MIN_HINT_AMOUNT - 5,
            Difficulty.Expert => CELL_COUNT - MIN_HINT_AMOUNT,
            _                 => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };

        public int GetBoardHoleAmount() => Cells.Count(c => c.value == 0);

        public Cell GetCell(int row, int column) => Cells[row * BOARD_SIZE + column];

        public List<Cell> GetBlockCells(int block) {
            if (block is < 0 or > BOARD_SIZE - 1) {
                throw new ArgumentOutOfRangeException(nameof(block), block, "Block must be between 0 and 8");
            }

            var cells = new List<Cell>();
            var row   = block / SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            var col   = block % SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            for (var i = row; i < row + SUB_BOARD_SIZE; i++) {
                for (var j = col; j < col + SUB_BOARD_SIZE; j++) {
                    cells.Add(GetCell(i, j));
                }
            }

            return cells;
        }

        public override string ToString() {
            StringBuilder sb = new();
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var column = 0; column < BOARD_SIZE; column++) {
                    sb.Append(GetCell(row, column).value);
                    sb.Append(" ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}