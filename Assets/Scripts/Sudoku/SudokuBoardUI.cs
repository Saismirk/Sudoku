using System.Collections.Generic;
using System.Linq;
using UI_Toolkit.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku
{
    public class SudokuBoardUI : MonoBehaviour {
        [SerializeField] UIDocument boardUI;

        VisualElement       _boardContainer;
        List<SudokuBlockUI> _blocks = new();
        List<SudokuCell>    _cells  = new();

        void Start() {
            _boardContainer = boardUI.rootVisualElement.Q<VisualElement>("BoardBase");
            Debug.Assert(_boardContainer != null, "BoardBase not found");
        }

        void OnEnable() {
            SudokuManager.OnBoardGenerated += OnBoardGenerated;
            SudokuCell.OnCellClicked += OnCellClicked;
        }

        void OnDisable() {
            SudokuManager.OnBoardGenerated -= OnBoardGenerated;
            SudokuCell.OnCellClicked -= OnCellClicked;
        }

        void OnBoardGenerated(SudokuBoard board) {
            if (boardUI == null) {
                return;
            }

            _blocks.Clear();

            _boardContainer = boardUI.rootVisualElement.Q<VisualElement>("BoardBase");

            foreach (var i in Enumerable.Range(0, SudokuBoard.BOARD_SIZE)) {
                var block = _boardContainer.Q<VisualElement>($"Block_{i}");
                Debug.Assert(block != null, $"Block {i} not found");
                _blocks.Add(new SudokuBlockUI(block, i, board.GetBlockCells(i)));
            }

            _cells = _blocks.SelectMany(block => block.Cells).ToList();
            _cells.Sort((cell1, cell2) => cell1.CellIndex > cell2.CellIndex ? 1 : -1);
            _cells.ForEach(cell => cell.Init());
            UpdateBoard(board);
        }

        public void UpdateBoard(SudokuBoard board) {
            foreach (var cell in _cells.Select((cell, i) => (cell, i))) {
                cell.cell.SetCellValue(board.Cells[cell.i].value.ToString());
            }
        }

        void OnCellClicked (int cellIndex) {
            _cells.ForEach(cell => cell.UpdateCellState(SudokuCell.CellState.None));
            var cellsToHighlight = SudokuManager.board.GetValidationCellIndices(cellIndex);
            foreach (var highlightCellIndex in cellsToHighlight) {
                _cells[highlightCellIndex].UpdateCellState(SudokuCell.CellState.Highlighted);
            }
            _cells[cellIndex].UpdateCellState(SudokuCell.CellState.Selected);
        }

    }
}