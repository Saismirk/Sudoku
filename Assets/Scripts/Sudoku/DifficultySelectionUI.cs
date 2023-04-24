using Extensions;
using UnityEngine.UIElements;

namespace Sudoku
{
    public class DifficultySelectionUI : PanelUI {
        Button _easyButton;
        Button _mediumButton;
        Button _hardButton;

        protected override void SetupVisualElements() {
            base.SetupVisualElements();
            _easyButton = Root.Q<Button>("EasyButton");
            _mediumButton = Root.Q<Button>("MediumButton");
            _hardButton = Root.Q<Button>("HardButton");
            _easyButton.clicked += OnEasyButtonClicked;
            _mediumButton.clicked += OnMediumButtonClicked;
            _hardButton.clicked += OnHardButtonClicked;
            SudokuManager.OnBoardGenerationStarted += HidePanel;
        }

        protected override void DisableVisualElements() {
            base.DisableVisualElements();
            _easyButton.clicked -= OnEasyButtonClicked;
            _mediumButton.clicked -= OnMediumButtonClicked;
            _hardButton.clicked -= OnHardButtonClicked;
            SudokuManager.OnBoardGenerationStarted -= HidePanel;
        }

        void OnEasyButtonClicked() {
            _easyButton.schedule.Execute(() => SudokuManager.StartNewGame(Difficulty.Easy))
                       .StartingIn(200);
        }

        void OnMediumButtonClicked() {
            _mediumButton.schedule.Execute(() => SudokuManager.StartNewGame(Difficulty.Medium)).StartingIn(200);
        }

        void OnHardButtonClicked() {
            _hardButton.schedule.Execute(() => SudokuManager.StartNewGame(Difficulty.Hard)).StartingIn(200);
        }
    }
}