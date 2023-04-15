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
        public       Cell[] Cells { get; private set; }
        public int[] Solution { get; private set; }
        public const int        BOARD_SIZE     = 9;
        public const int        SUB_BOARD_SIZE = 3;

        public SudokuBoard() {
            Cells = new Cell[BOARD_SIZE * BOARD_SIZE];
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var column = 0; column < BOARD_SIZE; column++) {
                    var index = row * BOARD_SIZE + column;
                    Cells[index] = new Cell(0, new BoardPosition(row, column));
                }
            }

            PopulateBoard();
            Solution = Cells.Select(c => c.value).ToArray();
        }

        public void PopulateBoard() {
            if (!PopulateCell(0, false)) {
                Debug.LogError("Failed to generate board");
            }
        }

        public bool SolveBoard() {
            var firstCell = GetNextEmptyCellIndex(0);
            if (firstCell < 0) {
                Debug.Log("Board is already solved");
                return true;
            }

            if (!PopulateCell(firstCell, true)) {
                return false;
            }

            return true;
        }

        bool PopulateCell(int cellIndex, bool solve) {
            var numbers = new int[BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++) {
                numbers[i] = i + 1;
            }

            if (cellIndex is >= BOARD_SIZE * BOARD_SIZE or < 0) {
                return true;
            }

            numbers = numbers.Where(number => IsCellValid(cellIndex, number)).ToArray();
            SetCellValue(cellIndex, 0);
            Shuffle(numbers);
            var nextCell = solve ? GetNextEmptyCellIndex(cellIndex) : GetNextCellIndex(cellIndex);
            if (nextCell < -1) {
                SetCellValue(cellIndex, numbers.FirstOrDefault());
                return true;
            }

            foreach (var num in numbers) {
                SetCellValue(cellIndex, num);
                if (PopulateCell(nextCell, solve)) {
                    return true;
                }
            }

            return false;
        }

        void SetCellValue(int cellIndex, int value) {
            var c = Cells[cellIndex];
            c.value = value;
            Cells[cellIndex] = c;
        }

        bool IsCurrentBoardSolution() => Cells.Select((cell, i) => (cell, i)).All(c => c.cell.value == Solution[c.i]);

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
                return false;
            }

            var result = IsCurrentBoardSolution();
            SetCellValues(valueList);
            Debug.Log($"Board is solution: {result}");
            return result;
        }

        public bool HasUniqueSolution(int cellIndex) {
            var valueList = Cells.Select(cell => cell.value).ToList();
            SetCellValue(cellIndex, 0);
            if (!SolveBoard()) {
                Debug.Log($"Board has no solution when removing cell {cellIndex}");
                SetCellValues(valueList);
                return false;
            }

            var result = IsCurrentBoardSolution();
            SetCellValues(valueList);
            Debug.Log($"Board is solution: {result}");
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

        public void RemoveCells(Difficulty difficulty) {
            var cellsToRemove = difficulty switch {
                Difficulty.Easy   => 40,
                Difficulty.Medium => 50,
                Difficulty.Hard   => 60,
                Difficulty.Expert => 70,
                _                 => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };

            var rand = new Random();
            rand.InitState((uint)Time.frameCount);
            var steps = 0;
            var validIndices = Cells.Select((cell, i) => (cell, i)).Where(c => c.cell.value != 0).Select(c => c.i).ToList();
            Shuffle(validIndices);
            if (!RemoveCell(ref cellsToRemove)) {
                Debug.LogError("Failed to remove cells");
            }
        }

        bool RemoveCell(ref int cellsToRemove) {
            if (cellsToRemove <= 0) {
                return true;
            }

            var nonEmptyCells = Cells.Where(c => c.value != 0).Select(c => c.Index).ToList();
            Shuffle(nonEmptyCells);

            foreach (var index in nonEmptyCells) {
                if (!HasUniqueSolution(index)) {
                    continue;
                }

                SetCellValue(index, 0);
                cellsToRemove--;
                if (RemoveCell(ref cellsToRemove)) {
                    return true;
                }
                SetCellValue(index, Solution[index]);
                cellsToRemove++;
            }

            return false;
        }

        int GetNextCellIndex(int cellIndex) {
            if (cellIndex is >= BOARD_SIZE * BOARD_SIZE or < 0) {
                return -1;
            }

            var nextIndex = cellIndex + 1;
            return nextIndex >= BOARD_SIZE * BOARD_SIZE ? -1 : nextIndex;
        }

        int GetNextEmptyCellIndex(int cellIndex) {
            if (cellIndex is >= BOARD_SIZE * BOARD_SIZE or < 0) {
                return -1;
            }

            var nextCellIndex = GetNextCellIndex(cellIndex);
            if (nextCellIndex < 0) {
                return -1;
            }
            var nextCell      = Cells[nextCellIndex];
            while (nextCell.value != 0) {
                nextCellIndex = GetNextCellIndex(nextCellIndex);
                if (nextCellIndex < 0) {
                    return -1;
                }
                nextCell = Cells[nextCellIndex];
            }

            return nextCellIndex;
        }

        bool IsCellValid(Cell? cell, int value) {
            if (cell == null) {
                return false;
            }

            var cellData = (Cell)cell;
            cellData.value = value;
            return IsCellValid(cellData);
        }

        bool IsCellValid(int cellIndex, int value) {
            if (cellIndex is < 0 or >= BOARD_SIZE * BOARD_SIZE) {
                return false;
            }

            var cellData = Cells[cellIndex];
            cellData.value = value;
            return IsCellValid(cellData);
        }

        bool IsCellValid(Cell cell) {
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