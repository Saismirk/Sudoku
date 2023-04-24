///

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class UILocalization : MonoBehaviour {
    [SerializeField] LocalizedStringTable _table = null;

    UIDocument  _uiDocument;
    StringTable _detailedTable;

    void OnEnable() {
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
        _table.TableChanged += OnTableChanged;
    }

    void OnDisable() {
        _table.TableChanged -= OnTableChanged;
    }

    void OnTableChanged(StringTable table) {
        var op = _table.GetTableAsync();
        if (op.IsDone) {
            OnTableLoaded(op);
        } else {
            op.Completed -= OnTableLoaded;
            op.Completed += OnTableLoaded;
        }
    }

    void OnTableLoaded(AsyncOperationHandle<StringTable> op) {
        _detailedTable = op.Result;
        UpdateLocalization();
    }

    public void UpdateLocalization() {
        LocalizeChildrenRecursively(_uiDocument.rootVisualElement, _detailedTable);
        _uiDocument.rootVisualElement.MarkDirtyRepaint();
    }

    void LocalizeChildrenRecursively(VisualElement element, StringTable table) {
        var elementHierarchy = element.hierarchy;
        var numChildren      = elementHierarchy.childCount;
        for (var i = 0; i < numChildren; i++) {
            var child = elementHierarchy.ElementAt(i);
            Localize(child, table);
        }

        for (var i = 0; i < numChildren; i++) {
            var child            = elementHierarchy.ElementAt(i);
            var childHierarchy   = child.hierarchy;
            var numGrandChildren = childHierarchy.childCount;
            if (numGrandChildren != 0)
                LocalizeChildrenRecursively(child, table);
        }
    }

    static void Localize(VisualElement next, StringTable table) {
        if (next is not TextElement textElement) return;
        var key = textElement.text;
        if (string.IsNullOrEmpty(key) || key[0] != '#') return;
        key = key.TrimStart('#');
        var entry = table[key];
        if (entry != null)
            textElement.text = entry.LocalizedValue;
        else
            Debug.LogWarning($"No {table.LocaleIdentifier.Code} translation for key: '{key}'");
    }
}