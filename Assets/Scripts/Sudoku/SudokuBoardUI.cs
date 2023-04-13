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
        }

        void OnEnable() {
            SudokuManager.OnBoardGenerated += OnBoardGenerated;
        }

        void OnDisable() {
            SudokuManager.OnBoardGenerated -= OnBoardGenerated;
        }

        void OnBoardGenerated(SudokuBoard board) {
            if (boardUI == null) {
                return;
            }

            _boardContainer = boardUI.rootVisualElement.Q<VisualElement>("BoardBase");

            foreach (var i in Enumerable.Range(0, SudokuBoard.BOARD_SIZE)) {
                var block = _boardContainer.Q<VisualElement>($"Block_{i}");
                Debug.Assert(block != null, $"Block {i} not found");
                _blocks.Add(new SudokuBlockUI(block, i, board.GetBlockCells(i)));
            }

            _cells = _blocks.SelectMany(block => block.Cells).ToList();
            UpdateBoard(board);
        }

        public void UpdateBoard(SudokuBoard board) {
            foreach (var cell in _cells.Select((cell, i) => (cell, i))) {
                cell.cell.SetCellValue(board.Cells[cell.i].Value.ToString());
            }
        }

    }
}