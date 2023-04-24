using System;

namespace Utilities {
    [Serializable]
    public class Observable<T> {
        public event Action<T> OnChanged;
        public T Value {
            get => _value;
            set {
                this._value = value;
                OnChanged?.Invoke(value);
            }
        }

        T _value;

        public Observable(T value) => Value = value;
        public void Update() => OnChanged?.Invoke(Value);
        public static implicit operator T(Observable<T> observable) => observable.Value;
        public static implicit operator Observable<T>(T value) => new(value);
        public override string ToString() => Value?.ToString() ?? string.Empty;
    }
}