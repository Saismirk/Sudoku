using Cysharp.Threading.Tasks;
using Sudoku;
using UnityEngine.UIElements;
using UnityEngine;

namespace SudokuUI.Controllers {
    public class LocalizedLabel : Label {
        public new class UxmlFactory : UxmlFactory<LocalizedLabel, UxmlTraits> { }

        public new class UxmlTraits : Label.UxmlTraits {
            readonly UxmlStringAttributeDescription _keyAttr = new() { name = "key" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var target = ((LocalizedLabel)ve);
                target.Key = _keyAttr.GetValueFromBag(bag, cc);
                if (Application.isPlaying) {
                    UILocalizationManager.OnLocalizationLoaded += target.UpdateText;
                }
            }
        }

        public string Key { get; set; }

        public void UpdateText() => UpdateTextAsync().Forget();

        public async UniTask UpdateTextAsync() => text = await UILocalizationManager.GetLocalizedTextAsync(Key);
    }
}