using System;

namespace Sudoku
{
    public struct NotificationData {
        public string           title;
        public string           message;
        public NotificationType type;
        public Action           onConfirm;
        public Action           onDismiss;

        public NotificationData(string title, string message, NotificationType type, Action onConfirm = null, Action onDismiss = null) {
            this.title = title;
            this.message = message;
            this.type = type;
            this.onConfirm = onConfirm;
            this.onDismiss = onDismiss;
        }
    }
}