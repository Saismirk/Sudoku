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
        VisualElement _background;

        public SudokuBlockUI(VisualElement blockContainer, int index, IReadOnlyList<Cell> blockCells) {
            _blockContainer = blockContainer;
            BlockNumber = index;
            Cells = _blockContainer.Query<SudokuCell>().ToList();
            _background = _blockContainer.Q<VisualElement>("BG");

            foreach (var cell in Cells.Select((cell, i) => (cell, i))) {
                cell.cell.SetCellIndex(blockCells[cell.i].Index);
            }

            for (var i = 0; i < 3; i++) {
                Rows[i] = _blockContainer.Q<VisualElement>($"Row_{i}");
                Debug.Assert(Rows[i] != null, $"Row_{i} is null");
                Columns[i] = _blockContainer.Q<VisualElement>($"Col_{i}");
                Debug.Assert(Columns[i] != null, $"Col_{i} is null");
            }
        }

        public void HighlightBlock(bool highlight) {
            if (highlight) {
                _background.AddToClassList(HIGHLIGHTED_CLASS);
                return;
            }

            _background.RemoveFromClassList(HIGHLIGHTED_CLASS);
            HighlightRow(-1);
            HighlightColumn(-1);
        }

        public void HighlightRow(int rowIndex) {
            foreach (var row in Rows) {
                row.RemoveFromClassList(HIGHLIGHTED_CLASS);
            }

            if (rowIndex < 0) {
                return;
            }

            Rows[rowIndex].AddToClassList(HIGHLIGHTED_CLASS);
        }

        public void HighlightColumn(int columnIndex) {
            foreach (var column in Columns) {
                column.RemoveFromClassList(HIGHLIGHTED_CLASS);
            }

            if (columnIndex < 0) {
                return;
            }

            Columns[columnIndex].AddToClassList(HIGHLIGHTED_CLASS);
        }

        public void SetBlockNumber(int blockNumber) => BlockNumber = blockNumber;
    }
}