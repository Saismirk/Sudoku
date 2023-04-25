using UnityEngine.UIElements;

namespace Sudoku
{
    public class LoadingUI : PanelUI {
        VisualElement _loadingIcon;
        int           _rotation;
        IVisualElementScheduledItem _rotateSchedule;
        protected override void SetupVisualElements() {
            base.SetupVisualElements();
            SudokuManager.OnBoardGenerationStarted += ShowPanel;
            SudokuManager.OnBoardGenerationFinished += HidePanel;
        }

        protected override void DisableVisualElements() {
            base.DisableVisualElements();
            SudokuManager.OnBoardGenerationStarted -= ShowPanel;
            SudokuManager.OnBoardGenerationFinished -= HidePanel;
        }

        //Animate loading icon by rotating it indefinitely.
        public override void ShowPanel() {
            base.ShowPanel();
            _loadingIcon = Root.Q<VisualElement>("LoadingIcon");
            _loadingIcon.style.rotate = new Rotate(0);
            StartLoadingAnimation();
        }

        public override void HidePanel() {
            base.HidePanel();
            StopLoadingAnimation();
        }

        void StartLoadingAnimation() {
            _rotation = 0;
            _rotateSchedule?.Pause();
            _rotateSchedule = _loadingIcon.schedule.Execute(RotateLoadingIcon).Every(0);
        }

        void StopLoadingAnimation() => _rotateSchedule?.Pause();

        void RotateLoadingIcon(TimerState timer) {
            _rotation += 5;
            _loadingIcon.style.rotate = new Rotate(_rotation);
        }
    }
}