using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI_Toolkit.Controllers {
    public class SudokuCell : VisualElement {
        public new class UxmlFactory : UxmlFactory<SudokuCell, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits {
            UxmlStringAttributeDescription _cellValue = new() { name = "Value" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var target = ((SudokuCell) ve);
                target._cellValue = _cellValue.GetValueFromBag(bag, cc);
                target.UpdateCellValue();
            }
        }

        string _cellValue;

        public int CellIndex { get; private set; }
        public SudokuCell() {
            var label = new Label();
            Add(label);
            label.pickingMode = PickingMode.Ignore;
            RegisterCallback<ClickEvent>(OnClickEventListener);
        }

        void OnClickEventListener(ClickEvent evt) {
            Debug.Log($"Cell {CellIndex} clicked");
        }

        public void UpdateCellValue() {
            SetCellValue(_cellValue);
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