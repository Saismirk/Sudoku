using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Tables;

namespace Sudoku {
    public class UILocalizationManager : MonoBehaviour {
        [SerializeField] LocalizedStringTable localizedStringTable;

        public static event Action OnLocalizationLoaded;

        static StringTable _detailedTable;
        static bool        _initialized = false;

        void Awake()                           => LoadLocalization().Forget();
        void OnEnable()                        => localizedStringTable.TableChanged += OnTableChanged;
        void OnDisable()                       => localizedStringTable.TableChanged -= OnTableChanged;
        void OnTableChanged(StringTable table) => LoadLocalization().Forget();

        async UniTask LoadLocalization() {
            _initialized = false;
            _detailedTable = await localizedStringTable.GetTableAsync();
            if (_detailedTable == null) {
                Debug.LogError("Failed to load localization table", this);
            }
            _initialized = true;
            OnLocalizationLoaded?.Invoke();
        }

        public static string GetLocalizedText(string key) {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            if (_detailedTable != null) return _detailedTable.GetEntry(key)?.GetLocalizedString() ?? string.Empty;
            Debug.LogError("Localization table is not loaded", _detailedTable);
            return key;
        }

        public static async UniTask<string> GetLocalizedTextAsync(string key) {
            while (!_initialized) {
                await UniTask.Yield();
            }
            return GetLocalizedText(key);
        }
    }
}