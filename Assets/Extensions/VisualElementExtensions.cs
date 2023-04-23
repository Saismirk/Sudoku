using UnityEngine.UIElements;
using System;

namespace Extensions {
    public static class VisualElementExtensions {
        public static void AddTemporaryClass(this VisualElement element, string className, int duration) {
            if (element == null) {
                return;
            }

            element.AddToClassList(className);
            element.schedule.Execute(() => element.RemoveFromClassList(className)).StartingIn(duration);
        }
    }
}