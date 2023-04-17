using System.Collections.Generic;
using System.Linq;
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
            HighlightValidationCells(cellIndex).Forget();
        }

        async UniTask HighlightValidationCells(int cellIndex) {
            var cell = SudokuManager.board.Cells[cellIndex];
            foreach (var block in _blocks) {
                block.HighlightBlock(false);
                await UniTask.Yield();
            }

            await UniTask.Yield();
            _blocks[cell.Position.Block].HighlightBlock(true);
            await UniTask.Yield();
            HighlightBlockRows(cell).Forget();
            HighlightBlockColumns(cell).Forget();
            HighlightCell(cell).Forget();
        }

        async UniTask HighlightCell(Cell cell) {
            _cells[cell.Index].UpdateCellState(SudokuCell.CellState.Selected);
            var batch = 0;
            for (var i = 0; i < SudokuBoard.CELL_COUNT; i++) {
                if (cell.value == SudokuManager.board.Cells[i].value) {
                    if (_cells[i].UpdateCellState(SudokuCell.CellState.Selected)) await UniTask.Yield();
                    continue;
                }

                if (_cells[i].UpdateCellState(SudokuCell.CellState.None) && batch > 20) {
                    batch = 0;
                    await UniTask.Yield();
                }

                batch++;
            }
        }

        async UniTask HighlightBlockRows(Cell cell) {
            var blockRowIndexStart = cell.Position.Block - cell.Position.Block % SudokuBoard.SUB_BOARD_SIZE;
            for (var i = blockRowIndexStart; i < blockRowIndexStart + SudokuBoard.SUB_BOARD_SIZE; i++) {
                if (i == cell.Position.Block) {
                    await UniTask.Yield();
                    continue;
                }

                _blocks[i].HighlightRow(cell.Position.BlockRelativeRow);
            }
        }

        async UniTask HighlightBlockColumns(Cell cell) {
            var blockColumnIndexStart = cell.Position.Block % SudokuBoard.SUB_BOARD_SIZE;
            for (var i = blockColumnIndexStart; i < SudokuBoard.SUB_BOARD_SIZE * SudokuBoard.SUB_BOARD_SIZE; i += SudokuBoard.SUB_BOARD_SIZE) {
                if (i == cell.Position.Block) {
                    await UniTask.Yield();
                    continue;
                }

                _blocks[i].HighlightColumn(cell.Position.BlockRelativeColumn);
            }
        }
    }
}