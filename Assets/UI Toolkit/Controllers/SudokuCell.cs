using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI_Toolkit.Controllers {
    public class SudokuCell : VisualElement {
        public enum CellState {
            None,
            Selected,
            Highlighted
        }

        public new class UxmlFactory : UxmlFactory<SudokuCell, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits {
            UxmlStringAttributeDescription _cellValue = new() { name = "Value" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var target = ((SudokuCell)ve);
                target._cellValue = _cellValue.GetValueFromBag(bag, cc);
                target.UpdateCellValue();
            }
        }

        const string BASE_CLASS        = "sudoku_cell";
        const string SELECTED_CLASS    = "selected";
        const string HIGHLIGHTED_CLASS = "highlighted";

        public static event System.Action<int> OnCellClicked;

        public bool      IsSelected { get; private set; }
        public CellState State      { get; private set; }

        string _cellValue;
        bool   _initialized;

        public int CellIndex { get; private set; }

        public SudokuCell() {
            var label = new Label();
            Add(label);
            label.pickingMode = PickingMode.Ignore;
            RegisterCallback<ClickEvent>(OnClickEventListener);
        }

        public void Init() {
            UpdateCellState(CellState.None);
            if (_initialized) {
                return;
            }
            OnCellClicked += _ => UpdateCellState(CellState.None);
            _initialized = true;
        }

        void OnClickEventListener(ClickEvent evt) {
            Debug.Log($"Cell {CellIndex} clicked");
            OnCellClicked?.Invoke(CellIndex);
            UpdateCellState(CellState.Selected);
        }

        public void UpdateCellValue() {
            SetCellValue(_cellValue);
        }

        void UpdateCellState(CellState state) {
            if (State == state) {
                return;
            }

            Debug.Log($"Cell {CellIndex} state changed to {state}");
            State = state;
            switch (State) {
                case CellState.None:
                    RemoveFromClassList(SELECTED_CLASS);
                    RemoveFromClassList(HIGHLIGHTED_CLASS);
                    break;
                case CellState.Selected:
                    AddToClassList(SELECTED_CLASS);
                    RemoveFromClassList(HIGHLIGHTED_CLASS);
                    break;
                case CellState.Highlighted:
                    AddToClassList(HIGHLIGHTED_CLASS);
                    RemoveFromClassList(SELECTED_CLASS);
                    break;
            }
        }

        public void SetCellValue(string value) {
            var label = this.Q<Label>();
            label.text = value == "0" ? string.Empty : value;
        }

        public void SetCellIndex(int index) {
            CellIndex = index;
        }
    }
}