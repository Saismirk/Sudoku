using System;

namespace Sudoku
{
    [Serializable]
    public struct BoardPosition {
        public int Row    { get; set; }
        public int Column { get; set; }
        public int Block  { get; set; }

        public BoardPosition(int row, int column, int block) {
            Row = row;
            Column = column;
            Block = block;
        }
    }
}