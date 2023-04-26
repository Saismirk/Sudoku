using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Extensions;
using UI_Toolkit.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
    [RequireComponent(typeof(UIDocument))]
    public class SudokuBoardUI : PanelUI {
        const string PAUSE_TIMER_CLASS                = "sudoku-pause--paused";
        const string BUTTON_PRESSED_SUCCESS_CLASS     = "sudoku-button--pressed";
        const string BUTTON_PRESSED_FAIL_CLASS        = "sudoku-button--press_fail";
        const string BUTTON_PRESSED_FAIL_LABEL_CLASS  = "sudoku-label--fail";
        const int    BUTTON_PRESSED_REACTION_DURATION = 100;

        [SerializeField, Range(0, 80)] int cellUpdateBatchSize = 3;

        readonly List<SudokuBlockUI>     _blocks       = new();
        readonly Dictionary<int, Button> _inputButtons = new();
        Button                           _pauseButton;
        Button                           _restartButton;
        VisualElement                    _boardContainer;
        VisualElement                    _inputContainer;
        Label                            _timer;
        Label                            _difficultyLabel;
        Label                            _attemptsLabel;
        List<SudokuCell>                 _cells = new();
        int                              _selectedCellIndex;

        protected override void SetupVisualElements() {
            base.SetupVisualElements();
            InitializeVisualElements();
            SudokuManager.OnBoardGenerationStarted += ShowPanel;
            SudokuManager.OnBoardGenerated += OnBoardGenerated;
            SudokuManager.DifficultySetting.OnChanged += UpdateDifficultyLabel;
            SudokuManager.Attempts.OnChanged += UpdateAttemptsLabel;
            SudokuManager.OnGamePaused += OnGamePaused;
            SudokuManager.Timer.OnTimerUpdated += UpdateTimer;
            SudokuCell.OnCellClicked += OnCellClicked;
        }

        protected override void DisableVisualElements() {
            base.DisableVisualElements();
            SudokuManager.OnBoardGenerated -= OnBoardGenerated;
            SudokuManager.OnGamePaused -= OnGamePaused;
            SudokuManager.DifficultySetting.OnChanged -= UpdateDifficultyLabel;
            SudokuManager.Attempts.OnChanged -= UpdateAttemptsLabel;
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
            if (Root == null) {
                Debug.LogError("Board UI is null");
                return;
            }

            GetVisualElement(ref _boardContainer, Root, "BoardBase");
            GetVisualElement(ref _inputContainer, Root, "Inputs");
            GetVisualElement(ref _timer, Root, "TimeValue");
            GetVisualElement(ref _pauseButton, Root, "PauseToggle");
            GetVisualElement(ref _restartButton, Root, "RestartButton");
            GetVisualElement(ref _difficultyLabel, Root, "DifficultyLabel");
            GetVisualElement(ref _attemptsLabel, Root, "AttemptCounter");

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

        async UniTaskVoid HighlightValidationCells(int cellIndex) {
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
            SudokuManager.PushNotification(new NotificationData(title: UILocalizationManager.GetLocalizedText("not_reset_board_title"),
                                                                message: UILocalizationManager.GetLocalizedText("not_reset_board_msg"),
                                                                type: NotificationType.Confirmation,
                                                                onConfirm: () => SudokuManager.DismissAndRestartAsync().Forget(),
                                                                onDismiss: () => SudokuManager.DismissNotification()));
        }

        void OnInputButtonPressed(int value) {
            Debug.Log($"Button {value} pressed");
            if (SudokuManager.Board.GetCellValue(_selectedCellIndex) != 0) return;
            if (SudokuManager.Board.TrySetCorrectCellValue(_selectedCellIndex, value)) {
                Debug.Log($"Correct value was set ({value} on Cell {_selectedCellIndex})");
                UpdateBoard(SudokuManager.Board);
                _inputButtons[value].AddTemporaryClass(BUTTON_PRESSED_SUCCESS_CLASS, BUTTON_PRESSED_REACTION_DURATION);
                SudokuManager.Board.UpdateValueCount(value);
                UpdateButtonAvailability();
                SelectCells(SudokuManager.Board.Cells[_selectedCellIndex], false).Forget();
                return;
            }

            Debug.Log($"Incorrect value was set ({value} on Cell {_selectedCellIndex})");
            SudokuManager.Attempts.Value++;
            _inputButtons[value].AddTemporaryClass(BUTTON_PRESSED_FAIL_CLASS, BUTTON_PRESSED_REACTION_DURATION);
            _attemptsLabel.AddTemporaryClass(BUTTON_PRESSED_FAIL_LABEL_CLASS, BUTTON_PRESSED_REACTION_DURATION);
        }

        public void TogglePause() => SudokuManager.TogglePauseTimer();

        void OnGamePaused(bool paused) {
            _pauseButton?.Q<VisualElement>("PauseIcon")?.ToggleInClassList(PAUSE_TIMER_CLASS);
            _pauseButton.AddTemporaryClass(BUTTON_PRESSED_SUCCESS_CLASS, BUTTON_PRESSED_REACTION_DURATION);
        }

        void UpdateButtonAvailability() {
            foreach (var button in _inputButtons) {
                button.Value.SetEnabled(SudokuManager.Board.ValueCounts[button.Key - 1] < SudokuBoard.BOARD_SIZE);
            }
        }

        void UpdateDifficultyLabel(Difficulty difficulty) {
            UpdateDifficultyLabelAsync(difficulty).Forget();
        }

        void UpdateAttemptsLabel(int attempts) {
            if (_attemptsLabel == null) return;
            _attemptsLabel.text = attempts.ToString();
        }

        async UniTask UpdateDifficultyLabelAsync(Difficulty difficulty) {
            if (_difficultyLabel == null) return;
            _difficultyLabel.text = await UILocalizationManager.GetLocalizedTextAsync(difficulty.ToString());
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