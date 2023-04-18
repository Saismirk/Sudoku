using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UI_Toolkit.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
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

        void OnCellClicked(int cellIndex) {
            var cells = SudokuManager.board.GetValidationCellIndices(cellIndex);
            HighlightValidationCells(cellIndex).Forget();
        }

        async UniTask HighlightValidationCells(int cellIndex) {
            var cell = SudokuManager.board.Cells[cellIndex];
            await UniTask.Yield();
            await SelectCells(cell);
            await UniTask.Yield();
            await HighlightCells();
        }

        async UniTask SelectCells(Cell cell) {
            var batch = 0;
            for (var index = 0; index < _cells.Count; index++) {
                if (cell.value == SudokuManager.board.Cells[index].value) {
                    _cells[index].UpdateCellState(SudokuCell.CellState.Selected);
                    await UniTask.Yield();
                    continue;
                }

                if (_cells[index].UpdateCellState(SudokuCell.CellState.None)) {
                    if (batch++ % 3 == 0) await UniTask.Yield();
                }

                batch++;
            }
        }

        async UniTask HighlightCells() {
            var batch = 0;
            foreach (var highlightedCellIndex in SudokuManager.board.HighlightedCellIndices) {
                _cells[highlightedCellIndex].UpdateCellState(SudokuCell.CellState.Highlighted);
                if (batch++ % 3 == 0) await UniTask.Yield();
                batch++;
            }
        }
    }
}