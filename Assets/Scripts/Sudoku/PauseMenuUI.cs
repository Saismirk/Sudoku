using Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuUI : MonoBehaviour {
        const string HIDDEN_CLASS  = "sudoku--hidden";
        const string REMOVED_CLASS = "sudoku--removed";

        UIDocument _menuUI;
        Button     _resumeButton;

        void Awake() {
            _menuUI = GetComponent<UIDocument>();
        }

        void OnEnable() {
            SudokuManager.OnGamePaused += OnGamePaused;
            _resumeButton = _menuUI.rootVisualElement.Q<Button>("ResumeButton");
            _resumeButton.clicked += OnResumeButtonClicked;
        }

        void OnDisable() {
            SudokuManager.OnGamePaused -= OnGamePaused;
            _resumeButton.clicked -= OnResumeButtonClicked;
        }

        void OnGamePaused(bool paused) {
            TogglePauseMenu(paused);
        }

        void OnResumeButtonClicked() {
            _resumeButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _resumeButton.schedule.Execute(() => SudokuManager.TogglePauseTimer()).StartingIn(200);
        }

        void TogglePauseMenu(bool show) {
            if (show && !_menuUI.enabled) {
                _menuUI.enabled = true;
            }

            if (show) {
                _menuUI.rootVisualElement.RemoveFromClassList(REMOVED_CLASS);
                _menuUI.rootVisualElement.AddToClassList(HIDDEN_CLASS);
                _menuUI.rootVisualElement.schedule.Execute(() => _menuUI.rootVisualElement.RemoveFromClassList(HIDDEN_CLASS)).StartingIn(200);
                return;
            }

            _menuUI.rootVisualElement.AddToClassList(HIDDEN_CLASS);
            _menuUI.rootVisualElement.schedule.Execute(() => _menuUI.rootVisualElement.AddToClassList(REMOVED_CLASS)).StartingIn(200);
        }
    }
}