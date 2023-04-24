namespace Sudoku
{
    public class LoadingUI : PanelUI {
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
    }
}