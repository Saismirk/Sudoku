using Extensions;
using UnityEngine.UIElements;

namespace Sudoku
{
    public class DifficultySelectionUI : PanelUI {
        Button _easyButton;
        Button _mediumButton;
        Button _hardButton;
        Button _expertButton;

        protected override void SetupVisualElements() {
            base.SetupVisualElements();
            _easyButton = Root.Q<Button>("EasyButton");
            _mediumButton = Root.Q<Button>("MediumButton");
            _hardButton = Root.Q<Button>("HardButton");
            _expertButton = Root.Q<Button>("ExpertButton");
            _easyButton.clicked += OnEasyButtonClicked;
            _mediumButton.clicked += OnMediumButtonClicked;
            _hardButton.clicked += OnHardButtonClicked;
            _expertButton.clicked += OnExpertButtonClicked;
            SudokuManager.OnBoardGenerationStarted += HidePanel;
            SudokuManager.OnGameStarted += ShowPanel;
        }

        protected override void DisableVisualElements() {
            base.DisableVisualElements();
            _easyButton.clicked -= OnEasyButtonClicked;
            _mediumButton.clicked -= OnMediumButtonClicked;
            _hardButton.clicked -= OnHardButtonClicked;
            _expertButton.clicked -= OnExpertButtonClicked;
            SudokuManager.OnBoardGenerationStarted -= HidePanel;
            SudokuManager.OnGameStarted -= ShowPanel;
        }

        void OnEasyButtonClicked() {
            _easyButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _easyButton.schedule.Execute(() => SudokuManager.StartNewGame(Difficulty.Easy))
                       .StartingIn(200);
        }

        void OnMediumButtonClicked() {
            _mediumButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _mediumButton.schedule.Execute(() => SudokuManager.StartNewGame(Difficulty.Medium)).StartingIn(200);
        }

        void OnHardButtonClicked() {
            _hardButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _hardButton.schedule.Execute(() => SudokuManager.StartNewGame(Difficulty.Hard)).StartingIn(200);
        }

        void OnExpertButtonClicked() {
            _expertButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _expertButton.schedule.Execute(() => SudokuManager.StartNewGame(Difficulty.Expert)).StartingIn(200);
        }
    }
}