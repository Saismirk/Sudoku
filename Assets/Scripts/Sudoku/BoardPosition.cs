using System;

namespace Sudoku {
    [Serializable]
    public struct BoardPosition {
        public int Row                 { get; private set; }
        public int BlockRelativeRow    { get; private set; }
        public int Column              { get; private set; }
        public int BlockRelativeColumn { get; private set; }
        public int Block               { get; private set; }

        public BoardPosition(int row, int column, int block) {
            Row = row;
            BlockRelativeRow = row % 3;
            Column = column;
            BlockRelativeColumn = column % 3;
            Block = block;
        }
    }
}