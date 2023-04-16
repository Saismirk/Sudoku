using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public       Cell[] Cells    { get; private set; }
        public       int[]  Solution { get; private set; }
        public const int    BOARD_SIZE          = 9;
        public const int    SUB_BOARD_SIZE      = 3;
        public const int    CELL_COUNT          = BOARD_SIZE * BOARD_SIZE;
        public const int    MIN_HINT_AMOUNT     = 24;
        public const int    MAX_HINT_AMOUNT     = CELL_COUNT - 1;
        public const int    MAX_RECURSION_DEPTH = CELL_COUNT * CELL_COUNT;

        public SudokuBoard() {
            Cells = new Cell[CELL_COUNT];
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var column = 0; column < BOARD_SIZE; column++) {
                    var index = row * BOARD_SIZE + column;
                    Cells[index] = new Cell(0, new BoardPosition(row, column));
                }
            }

            PopulateBoard();
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

        bool PopulateCell(int cellIndex, ref int iterations) {
            iterations++;
            if (iterations > MAX_RECURSION_DEPTH) {
                iterations = 0;
                return false;
            }

            var numbers = new int[BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++) {
                numbers[i] = i + 1;
            }

            if (cellIndex is >= CELL_COUNT or < 0) {
                iterations = 0;
                return true;
            }

            numbers = numbers.Where(number => IsCellValid(cellIndex, number)).ToArray();
            SetCellValue(cellIndex, 0);
            Shuffle(numbers);
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

        public bool HasUniqueSolution() {
            var valueList = Cells.Select(cell => cell.value).ToList();
            if (!SolveBoard()) {
                Debug.Log("Board has no solution");
                return false;
            }

            var result = IsCurrentBoardSolution();
            SetCellValues(valueList);
            return result;
        }

        bool HasUniqueSolution(int cellIndex) {
            var valueList = Cells.Select(cell => cell.value).ToList();
            SetCellValue(cellIndex, 0);
            if (!SolveBoard()) {
                Debug.Log($"Board has no solution when removing cell {cellIndex}");
                SetCellValues(valueList);
                return false;
            }

            var result = IsCurrentBoardSolution();
            SetCellValues(valueList);
            return result;
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

        public void RemoveCells(Difficulty difficulty) {
            Debug.Log($"Removing cells for difficulty {difficulty}");
            var numberOfAttempts = 0;
            var iterations       = 0;
            while (true) {
                ApplyKnownSolution();
                var cellsToRemove = GetDifficultyHoleAmount(difficulty);
                var usedIndices   = new HashSet<int>();
                var validIndices  = Cells.Select((cell, i) => (cell, i)).Where(c => c.cell.value != 0).Select(c => c.i).ToList();
                Shuffle(validIndices);
                if (!RemoveCell(ref cellsToRemove, usedIndices, ref iterations)) {
                    Debug.LogError("Failed to remove cells");
                }
                iterations = 0;
                numberOfAttempts++;
                if (numberOfAttempts > 10) {
                    Debug.LogError("Failed to remove cells after 10 attempts");
                    break;
                }

                if (!HasUniqueSolution()) continue;
                Debug.Log($"Found unique solution after {numberOfAttempts} attempts ({difficulty})");
                break;
            }
        }

        bool RemoveCell(ref int cellsToRemove, ISet<int> usedIndices, ref int iterations) {
            iterations++;
            Debug.Assert(iterations < MAX_RECURSION_DEPTH, $"Failed to remove cells after {MAX_RECURSION_DEPTH} iterations");
            if (iterations > MAX_RECURSION_DEPTH) {
                return false;
            }

            if (cellsToRemove > CELL_COUNT - MIN_HINT_AMOUNT) {
                return false;
            }

            if (usedIndices.Count >= CELL_COUNT - MIN_HINT_AMOUNT) {
                return false;
            }

            if (cellsToRemove <= 0) {
                if (!HasUniqueSolution()) {
                    Debug.LogError("Removed last cell but board is still not solvable");
                    return false;
                }

                Debug.Log("Removed last cell and found unique solution");
                return true;
            }

            var nonEmptyCells = Cells.Where(c => c.value != 0 && !usedIndices.Contains(c.Index)).Select(c => c.Index).ToList();
            Debug.Assert(nonEmptyCells.Count > 0, "No more cells to remove, board will never be solvable.");
            if (nonEmptyCells.Count == 0) {
                return false;
            }

            Shuffle(nonEmptyCells);

            foreach (var index in nonEmptyCells) {
                if (!HasUniqueSolution(index)) {
                    usedIndices.Add(index);
                    continue;
                }

                SetCellValue(index, 0);
                cellsToRemove--;
                if (RemoveCell(ref cellsToRemove, usedIndices, ref iterations)) {
                    return true;
                }

                SetCellValue(index, Solution[index]);
                cellsToRemove++;
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

        public bool IsCellValid(Cell? cell, int value) {
            if (cell == null) {
                return false;
            }

            var cellData = (Cell)cell;
            cellData.value = value;
            return IsCellValid(cellData);
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

        public bool IsBoardValid() => Cells?.All(IsCellValid) == true;

        public static int GetDifficultyHoleAmount(Difficulty difficulty) => difficulty switch {
            Difficulty.Easy   => CELL_COUNT - MIN_HINT_AMOUNT - 30,
            Difficulty.Medium => CELL_COUNT - MIN_HINT_AMOUNT - 20,
            Difficulty.Hard   => CELL_COUNT - MIN_HINT_AMOUNT - 10,
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