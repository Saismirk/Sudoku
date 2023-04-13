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

        public void PopulateBoard() => Cells = Cells.Select(PopulateCell)
                                                    .ToList();

        Cell PopulateCell(Cell cell) {
            var numbers = new int[BOARD_SIZE];
            for (var i = 0; i < BOARD_SIZE; i++) {
                numbers[i] = i + 1;
            }

            Shuffle(numbers);

            foreach (var num in numbers) {
                cell.Value = num;
                if (IsCellValid(cell)) {
                    break;
                }
                cell.Value = 0;
            }
            return cell;
        }

        static void Shuffle<T>(T[] array) {
            var rand = new Random();
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
                if (GetCell(row, i).Value == num || GetCell(i, col).Value == num) {
                    return false;
                }
            }

            var subRow = row / SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            var subCol = col / SUB_BOARD_SIZE * SUB_BOARD_SIZE;
            for (var i = subRow; i < subRow + SUB_BOARD_SIZE; i++) {
                for (var j = subCol; j < subCol + SUB_BOARD_SIZE; j++) {
                    if (GetCell(i, j).Value == num) {
                        return false;
                    }
                }
            }

            return true;
        }

        public Cell GetCell(int row, int column)            => Cells[row * BOARD_SIZE + column];
        public void SetCell(int row, int column, int value) => Cells[row * BOARD_SIZE + column].Value = value;
    }

    public class Cell {
        public int           Value         { get; set; }
        public BoardPosition Position      { get; set; }
        public bool          IsEditable    { get; private set; }
        public bool          IsSelected    { get; set; }
        public bool          IsError       { get; set; }
        public bool          IsHighlighted { get; set; }
        public bool          IsHint        { get; set; }
        public bool          IsSolved      { get; set; }

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