using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
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

        public void PopulateBoard() => Cells.ForEach(cell => PopulateCell(cell));

        Cell PopulateCell(Cell cell) {
            var numbers = new int[BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++) {
                numbers[i] = i + 1;
            }

            Shuffle(numbers);
            var validCellFound = false;
            foreach (var num in numbers) {
                cell.Value = num;
                if (!IsCellValid(cell)) continue;
                validCellFound = true;
                break;
            }
            if (!validCellFound) {
                cell.Value = 0;
            }
            return cell;
        }

        static void Shuffle<T>(T[] array) {
            var rand = new Random();
            rand.InitState();
            for (var i = array.Length - 1; i > 0; i--) {
                var j = rand.NextInt(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        bool IsCellValid(Cell cell) {
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
                    if (i != row && j != col && GetCell(i, j).Value == num) {
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