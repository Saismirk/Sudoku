using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UI_Toolkit.Controllers;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Sudoku {
    public class SudokuBoardUI : MonoBehaviour {
        [SerializeField, Range(0, 80)] int        cellUpdateBatchSize = 3;
        [SerializeField]               UIDocument boardUI;

        readonly List<SudokuBlockUI>     _blocks  = new();
        readonly Dictionary<int, Button> _buttons = new();
        VisualElement                    _boardContainer;
        VisualElement                    _inputContainer;
        Label                            _timer;
        List<SudokuCell>                 _cells = new();
        int                              _selectedCellIndex;

        void Start() {
            InitializeVisualElements();
        }

        void OnEnable() {
            SudokuManager.OnBoardGenerated += OnBoardGenerated;
            SudokuManager.Timer.OnTimerUpdated += UpdateTimer;
            SudokuCell.OnCellClicked += OnCellClicked;
        }

        void OnDisable() {
            SudokuManager.OnBoardGenerated -= OnBoardGenerated;
            SudokuManager.Timer.OnTimerUpdated -= UpdateTimer;
            SudokuCell.OnCellClicked -= OnCellClicked;
        }

        void OnBoardGenerated(SudokuBoard board) {
            _blocks.Clear();
            foreach (var i in Enumerable.Range(0, SudokuBoard.BOARD_SIZE)) {
                var block = _boardContainer.Q<VisualElement>($"Block_{i}");
                Debug.Assert(block != null, $"Block {i} not found");
                _blocks.Add(new SudokuBlockUI(block, i, board.GetBlockCells(i)));
            }

            _cells = _blocks.SelectMany(block => block.Cells).ToList();
            _cells.Sort((cell1, cell2) => cell1.CellIndex > cell2.CellIndex ? 1 : -1);
            _cells.ForEach(cell => cell.Init());
            UpdateBoard(board);
            UpdateButtonAvailability();
        }

        void UpdateTimer(float time) {
            if (_timer == null) return;
            _timer.text = $"{time / 60:00}:{time % 60:00}";
        }

        void InitializeVisualElements() {
            if (boardUI == null) {
                return;
            }

            _boardContainer = boardUI.rootVisualElement.Q<VisualElement>("BoardBase");
            Debug.Assert(_boardContainer != null, "BoardBase not found");
            _boardContainer = boardUI.rootVisualElement.Q<VisualElement>("BoardBase");
            Debug.Assert(_boardContainer != null, "BoardBase not found");
            _inputContainer = boardUI.rootVisualElement.Q<VisualElement>("Inputs");
            Debug.Assert(_inputContainer != null, "Inputs not found");
            _timer = boardUI.rootVisualElement.Q<Label>("TimeValue");
            Debug.Assert(_timer != null, "TimeValue not found");

            InitializeInputButtons();
        }

        public void UpdateBoard(SudokuBoard board) {
            foreach (var cell in _cells.Select((cell, i) => (cell, i))) {
                cell.cell.SetCellValue(board.Cells[cell.i].value.ToString());
            }
        }

        void OnCellClicked(int cellIndex) {
            var cells = SudokuManager.Board.GetValidationCellIndices(cellIndex);
            _selectedCellIndex = cellIndex;
            HighlightValidationCells(cellIndex).Forget();
        }

        async UniTask HighlightValidationCells(int cellIndex) {
            var cell = SudokuManager.Board.Cells[cellIndex];
            await UniTask.Yield();
            await SelectCells(cell);
            await UniTask.Yield();
            await HighlightCells();
        }

        async UniTask SelectCells(Cell cell, bool deselect = true) {
            var batch = 0;
            for (var index = 0; index < _cells.Count; index++) {
                if (cell.value != 0 && cell.value == SudokuManager.Board.Cells[index].value) {
                    _cells[index].UpdateCellState(SudokuCell.CellState.Selected);
                    await UniTask.Yield();
                    continue;
                }

                if (deselect && _cells[index].UpdateCellState(SudokuCell.CellState.None)) {
                    if (batch++ % cellUpdateBatchSize == 0) await UniTask.Yield();
                }

                batch++;
            }

            _cells[cell.Index].UpdateCellState(SudokuCell.CellState.Selected);
        }

        void InitializeInputButtons() {
            var buttons = _inputContainer.Query<Button>().ToList();
            foreach (var button in buttons) {
                var inputValue = int.Parse(button.name);
                _buttons.Add(inputValue, button);
                button.clickable.clicked += () => OnInputButtonPressed(inputValue);
            }
        }

        void OnInputButtonPressed(int value) {
            Debug.Log($"Button {value} pressed");
            if (SudokuManager.Board.GetCellValue(_selectedCellIndex) != 0) return;
            if (SudokuManager.Board.TrySetCorrectCellValue(_selectedCellIndex, value)) {
                Debug.Log($"Correct value was set ({value} on Cell {_selectedCellIndex})");
                UpdateBoard(SudokuManager.Board);
                SudokuManager.Board.UpdateValueCount(value);
                UpdateButtonAvailability();
                SelectCells(SudokuManager.Board.Cells[_selectedCellIndex], false).Forget();
                return;
            }

            Debug.Log($"Incorrect value was set ({value} on Cell {_selectedCellIndex})");
        }

        void UpdateButtonAvailability() {
            foreach (var button in _buttons) {
                button.Value.SetEnabled(SudokuManager.Board.ValueCounts[button.Key - 1] < SudokuBoard.BOARD_SIZE);
            }
        }

        async UniTask HighlightCells() {
            var batch = 0;
            foreach (var highlightedCellIndex in SudokuManager.Board.HighlightedCellIndices) {
                _cells[highlightedCellIndex].UpdateCellState(SudokuCell.CellState.Highlighted);
                if (batch++ % cellUpdateBatchSize == 0) await UniTask.Yield();
                batch++;
            }
        }
    }
}