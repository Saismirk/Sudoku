using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
    [RequireComponent(typeof(UIDocument))]
    [DisallowMultipleComponent]
    public abstract class PanelUI : MonoBehaviour {
        protected const string HIDDEN_CLASS  = "sudoku--hidden";
        protected const string REMOVED_CLASS = "sudoku--removed";

        [SerializeField] protected int  panelFadeTime = 200;
        [SerializeField] protected bool startDisabled;

        UIDocument              _uiDocument;
        protected VisualElement Root => _uiDocument?.rootVisualElement;
        protected VisualElement _basePanel;

        IVisualElementScheduledItem _hidePanelSchedule;
        IVisualElementScheduledItem _showPanelSchedule;

        void Awake() => Initialize();

        void OnEnable() => SetupVisualElements();

        void OnDisable() => DisableVisualElements();

        protected virtual void Initialize() {
            _uiDocument = GetComponent<UIDocument>();
            _uiDocument.enabled = true;
        }

        protected virtual void SetupVisualElements() {
            _basePanel = Root.Q<VisualElement>("Background");
            Debug.Assert(_basePanel != null, "Base panel is null");
            if (startDisabled) _basePanel?.AddToClassList(REMOVED_CLASS);
        }

        protected virtual void DisableVisualElements() {
            _basePanel = null;
        }

        public virtual void HidePanel() {
            _basePanel.AddToClassList(HIDDEN_CLASS);
            _showPanelSchedule?.Pause();
            _hidePanelSchedule = _basePanel.schedule.Execute(() => _basePanel.AddToClassList(REMOVED_CLASS))
                                           .StartingIn(panelFadeTime);
        }

        public virtual void HidePanelInstant() {
            _basePanel.AddToClassList(HIDDEN_CLASS);
            _hidePanelSchedule = null;
            _basePanel.AddToClassList(REMOVED_CLASS);
        }

        public virtual void ShowPanel() {
            _basePanel.RemoveFromClassList(REMOVED_CLASS);
            _hidePanelSchedule?.Pause();
            _basePanel.RemoveFromClassList(HIDDEN_CLASS);
        }
    }
}