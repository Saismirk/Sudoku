using Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
    public class PauseMenuUI : PanelUI {
        Button _resumeButton;

        protected override void SetupVisualElements() {
            base.SetupVisualElements();
            SudokuManager.OnGamePaused += OnGamePaused;
            _resumeButton = Root.Q<Button>("ResumeButton");
            _resumeButton.clicked += OnResumeButtonClicked;
        }

        protected override void DisableVisualElements() {
            base.DisableVisualElements();
            SudokuManager.OnGamePaused -= OnGamePaused;
            _resumeButton.clicked -= OnResumeButtonClicked;
        }

        void OnGamePaused(bool paused) {
            if (paused) {
                ShowPanel();
                return;
            }

            HidePanel();
        }

        void OnResumeButtonClicked() {
            _resumeButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _resumeButton.schedule.Execute(() => SudokuManager.TogglePauseTimer()).StartingIn(200);
        }
    }
}