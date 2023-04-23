using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Extensions;
using UI_Toolkit.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
    [RequireComponent(typeof(UIDocument))]
    public class SudokuBoardUI : MonoBehaviour {
        const string HIGHLIGHTED_CLASS                = "highlighted";
        const string SELECTED_CLASS                   = "selected";
        const string PAUSE_TIMER_CLASS                = "sudoku-pause--paused";
        const string BUTTON_PRESSED_REACTION_CLASS    = "sudoku-button--pressed";
        const int    BUTTON_PRESSED_REACTION_DURATION = 100;

        [SerializeField, Range(0, 80)] int cellUpdateBatchSize = 3;
        UIDocument                         _boardUI;

        readonly List<SudokuBlockUI>     _blocks       = new();
        readonly Dictionary<int, Button> _inputButtons = new();
        Button                           _pauseButton;
        Button                           _restartButton;
        VisualElement                    _boardContainer;
        VisualElement                    _inputContainer;
        Label                            _timer;
        List<SudokuCell>                 _cells = new();
        int                              _selectedCellIndex;

        void Awake() {
            _boardUI = GetComponent<UIDocument>();
        }

        void OnEnable() {
            InitializeVisualElements();
            SudokuManager.OnBoardGenerated += OnBoardGenerated;
            SudokuManager.OnGamePaused += OnGamePaused;
            SudokuManager.Timer.OnTimerUpdated += UpdateTimer;
            SudokuCell.OnCellClicked += OnCellClicked;
        }

        void OnDisable() {
            SudokuManager.OnBoardGenerated -= OnBoardGenerated;
            SudokuManager.OnGamePaused -= OnGamePaused;
            SudokuManager.Timer.OnTimerUpdated -= UpdateTimer;
            SudokuCell.OnCellClicked -= OnCellClicked;
        }

        void OnBoardGenerated(SudokuBoard board) {
            _blocks.Clear();
            foreach (var i in Enumerable.Range(0, SudokuBoard.BOARD_SIZE)) {
                var block = _boardContainer?.Q<VisualElement>($"Block_{i}");
                Debug.Assert(block != null, $"Block {i} not found");
                _blocks.Add(new SudokuBlockUI(block, i, board.GetBlockCells(i)));
            }

            _cells = _blocks.SelectMany(block => block.Cells).ToList();
            _cells.Sort((cell1, cell2) => cell1.CellIndex > cell2.CellIndex ? 1 : -1);
            _cells.ForEach(cell => cell.Init());
            UpdateBoard(board);
            UpdateButtonAvailability();
            _restartButton.SetEnabled(true);
        }

        void UpdateTimer(float time) {
            if (_timer == null) return;
            _timer.text = $"{time / 60:00}:{time % 60:00}";
        }

        void InitializeVisualElements() {
            if (_boardUI == null) {
                Debug.LogError("Board UI is null");
                return;
            }

            GetVisualElement(ref _boardContainer, _boardUI.rootVisualElement, "BoardBase");
            GetVisualElement(ref _inputContainer, _boardUI.rootVisualElement, "Inputs");
            GetVisualElement(ref _timer, _boardUI.rootVisualElement, "TimeValue");
            GetVisualElement(ref _pauseButton, _boardUI.rootVisualElement, "PauseToggle");
            GetVisualElement(ref _restartButton, _boardUI.rootVisualElement, "RestartButton");

            InitializeInputButtons();
        }

        void GetVisualElement<T>(ref T visualElement, VisualElement source, string elementName) where T : VisualElement {
            visualElement = source?.Q<T>(elementName);
            Debug.Assert(visualElement != null, $"{elementName} not found in {source?.name}");
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
                _inputButtons.Add(inputValue, button);
                button.clickable.clicked += () => OnInputButtonPressed(inputValue);
            }

            if (_pauseButton != null) _pauseButton.clickable.clicked += TogglePause;
            if (_restartButton != null) _restartButton.clickable.clicked += OnRestartButtonPressed;
        }

        void OnRestartButtonPressed() {
            SudokuManager.PushNotification(new NotificationData(title: "Restart Game",
                                                                message: "Are you sure you want to restart the game?",
                                                                type: NotificationType.Confirmation,
                                                                onConfirm: UniTask.Action(async () => {
                                                                    SudokuManager.TogglePauseTimer(false);
                                                                    await SudokuManager.GenerateBoard(true);
                                                                    await SudokuManager.GeneratePlayableBoard();
                                                                    SudokuManager.DismissNotification();
                                                                }),
                                                                onDismiss: SudokuManager.DismissNotification));
        }

        void OnInputButtonPressed(int value) {
            Debug.Log($"Button {value} pressed");
            if (SudokuManager.Board.GetCellValue(_selectedCellIndex) != 0) return;
            if (SudokuManager.Board.TrySetCorrectCellValue(_selectedCellIndex, value)) {
                Debug.Log($"Correct value was set ({value} on Cell {_selectedCellIndex})");
                UpdateBoard(SudokuManager.Board);
                _inputButtons[value].AddTemporaryClass(BUTTON_PRESSED_REACTION_CLASS, BUTTON_PRESSED_REACTION_DURATION);
                SudokuManager.Board.UpdateValueCount(value);
                UpdateButtonAvailability();
                SelectCells(SudokuManager.Board.Cells[_selectedCellIndex], false).Forget();
                return;
            }

            Debug.Log($"Incorrect value was set ({value} on Cell {_selectedCellIndex})");
        }

        public void TogglePause() => SudokuManager.TogglePauseTimer();

        void OnGamePaused(bool paused) {
            _pauseButton?.Q<VisualElement>("PauseIcon")?.ToggleInClassList(PAUSE_TIMER_CLASS);
            _pauseButton.AddTemporaryClass(BUTTON_PRESSED_REACTION_CLASS, BUTTON_PRESSED_REACTION_DURATION);
        }

        void UpdateButtonAvailability() {
            foreach (var button in _inputButtons) {
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