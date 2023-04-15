using System;

namespace Sudoku {
    [Serializable]
    public struct Cell {
        public int           value;
        public BoardPosition Position { get; set; }

        public int Index => Position.Row * SudokuBoard.BOARD_SIZE + Position.Column;

        public Cell(int value, BoardPosition position) {
            this.value = value;
            Position = position;
        }

        public override string ToString()                       => $"{value}";
        public override bool   Equals(object obj)               => obj is Cell cell && Equals(cell);
        public override int    GetHashCode()                    => HashCode.Combine(value, Position);
        public static   bool operator ==(Cell left, Cell right) => left.Equals(right);
        public static   bool operator !=(Cell left, Cell right) => !(left == right);

        public bool Equals(Cell other) => value == other.value && Position.Equals(other.Position);
    }
}