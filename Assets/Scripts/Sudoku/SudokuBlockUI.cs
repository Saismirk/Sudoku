using System.Collections.Generic;
using System.Linq;
using UI_Toolkit.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
    public class SudokuBlockUI {
        const  string           HIGHLIGHTED_CLASS = "highlighted";
        public int              BlockNumber   { get; private set; }
        public List<SudokuCell> Cells         { get; } = new();
        public VisualElement[]  Rows          { get; } = new VisualElement[3];
        public VisualElement[]  Columns       { get; } = new VisualElement[3];
        public bool             IsHighlighted { get; private set; }

        VisualElement _blockContainer;

        public SudokuBlockUI(VisualElement blockContainer, int index, IReadOnlyList<Cell> blockCells) {
            _blockContainer = blockContainer;
            BlockNumber = index;
            Cells = _blockContainer.Query<SudokuCell>().ToList();

            foreach (var cell in Cells.Select((cell, i) => (cell, i))) {
                cell.cell.SetCellIndex(blockCells[cell.i].Index);
            }
        }



        public void SetBlockNumber(int blockNumber) => BlockNumber = blockNumber;
    }
}