using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Sudoku {
    public enum Difficulty {
        Easy,
        Medium,
        Hard,
        Expert
    }

    public class SudokuBoard {
        public       List<Cell> Cells { get; private set; }
        public const int        BOARD_SIZE     = 9;
        public const int        SUB_BOARD_SIZE = 3;

        public SudokuBoard() {
            Cells = new();
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var column = 0; column < BOARD_SIZE; column++) {
                    Cells.Add(new Cell(0, new BoardPosition(row, column), true));
                }
            }

            PopulateBoard();
        }

        public void PopulateBoard(bool solve = false) {
            if (!PopulateCell(Cells[0], solve)) {
                Debug.LogError("Failed to generate board");
            }
        }

        bool PopulateCell(Cell cell, bool solve) {
            var numbers = new int[BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++) {
                numbers[i] = i + 1;
            }

            numbers = numbers.Where(number => IsCellValid(cell, number)).ToArray();
            cell.Value = 0;
            Shuffle(numbers);
            var nextCell = solve ? GetNextEmptyCell(cell) : GetNextCell(cell);
            if (nextCell == null) {
                cell.Value = numbers.FirstOrDefault();
                return true;
            }

            foreach (var num in numbers) {
                cell.Value = num;
                if (PopulateCell(nextCell, solve)) {
                    return true;
                }
            }

            return false;
        }

        void SetCellValues(IReadOnlyList<int> values) {
            for (var i = 0; i < values.Count; i++) {
                Cells[i].Value = values[i];
            }
        }

        int FindSolutions(Cell cell, ref int solutions, IReadOnlyList<int> values) {
            if (cell == null) {
                return solutions;
            }

            var numbers = new int[BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++) {
                numbers[i] = i + 1;
            }

            numbers = numbers.Where(number => IsCellValid(cell, number)).ToArray();
            cell.Value = 0;
            Shuffle(numbers);
            var nextCell = GetNextEmptyCell(cell);
            if (nextCell == null) {
                SetCellValues(values);
                solutions++;
                return solutions;
            }

            foreach (var num in numbers) {
                cell.Value = num;
                if (FindSolutions(nextCell, ref solutions, values) > 2) {
                    return solutions;
                }
            }

            return solutions;
        }

        public bool HasUniqueSolution() {
            var solutions = 0;
            var firstCell = GetNextEmptyCell(Cells[0]);
            if (firstCell == null) {
                Debug.Log("Board is already solved");
                return true;
            }
            var valueList = Cells.Select(cell => cell.Value).ToList();
            FindSolutions(firstCell, ref solutions, valueList);
            Debug.Log($"Found {solutions} solutions");
            Debug.Log($"{this}");
            SetCellValues(valueList);
            return solutions == 1;
        }

        static void Shuffle<T>(T[] array) {
            var rand = new Random();
            rand.InitState((uint)Time.frameCount);
            for (var i = array.Length - 1; i > 0; i--) {
                var j = rand.NextInt(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        Cell GetNextCell(Cell cell) {
            if (cell == null) {
                return null;
            }

            var nextIndex = cell.Index + 1;
            return nextIndex >= BOARD_SIZE * BOARD_SIZE ? null : Cells[nextIndex];
        }

        Cell GetNextEmptyCell(Cell cell) {
            if (cell == null) {
                return null;
            }

            var nextCell = GetNextCell(cell);
            while (nextCell != null && nextCell.Value != 0) {
                nextCell = GetNextCell(nextCell);
            }

            return nextCell;
        }

        bool IsCellValid(Cell cell, int value) {
            if (cell == null) {
                return false;
            }

            cell.Value = value;
            return IsCellValid(cell);
        }

        bool IsCellValid(Cell cell) {
            if (cell.Value == 0) {
                return false;
            }

            var row = cell.Position.Row;
            var col = cell.Position.Column;
            var num = cell.Value;
            for (var i = 0; i < BOARD_SIZE; i++) {
                if (i != col && GetCell(row, i).Value == num) {
                    return false;
                }

                if (i != row && GetCell(i, col).Value == num) {
                    return false;
                }
            }

            var subRow = row / SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            var subCol = col / SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            for (var i = subRow; i < subRow + SUB_BOARD_SIZE; i++) {
                for (var j = subCol; j < subCol + SUB_BOARD_SIZE; j++) {
                    if ((i != row || j != col) && GetCell(i, j).Value == num) {
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

        public void SetCell(int row, int column, int value) => Cells[row * BOARD_SIZE + column].Value = value;

        public override string ToString() {
            StringBuilder sb = new();
            for (var row = 0; row < BOARD_SIZE; row++) {
                for (var column = 0; column < BOARD_SIZE; column++) {
                    sb.Append(GetCell(row, column).Value);
                    sb.Append(" ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public class Cell {
        public int           Value         { get; set; }
        public BoardPosition Position      { get; set; }
        public int           Block         { get; set; }
        public bool          IsEditable    { get; private set; }
        public bool          IsSelected    { get; set; }
        public bool          IsError       { get; set; }
        public bool          IsHighlighted { get; set; }
        public bool          IsHint        { get; set; }
        public bool          IsSolved      { get; set; }

        public int Index => Position.Row * SudokuBoard.BOARD_SIZE + Position.Column;

        public Cell(int value, BoardPosition position, bool isEditable) {
            Value = value;
            Position = position;
            IsEditable = isEditable;
        }
    }

    [Serializable]
    public struct BoardPosition {
        public int Row    { get; set; }
        public int Column { get; set; }

        public BoardPosition(int row, int column) {
            Row = row;
            Column = column;
        }
    }
}