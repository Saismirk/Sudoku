using Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku
{
    public class TitleScreen : PanelUI {
        Button _playButton;
        Button _quitButton;

        protected override void SetupVisualElements() {
            base.SetupVisualElements();
            _playButton = Root.Q<Button>("StartButton");
            _quitButton = Root.Q<Button>("QuitButton");
            _playButton.clicked += OnPlayButtonClicked;
            SudokuManager.OnGameStarted += HidePanel;
        }

        protected override void DisableVisualElements() {
            base.DisableVisualElements();
            _playButton.clicked -= OnPlayButtonClicked;
            SudokuManager.OnGameStarted -= HidePanel;
        }

        void OnPlayButtonClicked() {
            _playButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _playButton.schedule.Execute(() => SudokuManager.StartGame()).StartingIn(200);
        }

    }
}