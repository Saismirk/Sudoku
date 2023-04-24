using System;
using Cysharp.Threading.Tasks;
using Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sudoku {
    public enum NotificationType {
        Info,
        Confirmation,
        Warning,
        Error,
    }

    public class NotificationMessageUI : PanelUI {
        Label         _titleLabel;
        Label         _messageLabel;
        Button        _acceptButton;
        Button        _dismissButton;
        VisualElement _basePanel;

        Action _onAccept;
        Action _onDismiss;

        protected override void SetupVisualElements() {
            base.SetupVisualElements();
            SudokuManager.OnNotificationMessage += OnNotificationMessage;
            SudokuManager.OnNotificationDismissed += HideNotification;
            _dismissButton = Root.Q<Button>("DismissButton");
            _dismissButton.clicked += OnDismissButtonClicked;
            _titleLabel = Root.Q<Label>("TitleLabel");
            _messageLabel = Root.Q<Label>("MessageLabel");
            _acceptButton = Root.Q<Button>("AcceptButton");
            _dismissButton = Root.Q<Button>("DismissButton");
            _acceptButton.clicked += OnAcceptButtonClicked;
        }

        protected override void DisableVisualElements() {
            SudokuManager.OnNotificationMessage -= OnNotificationMessage;
            SudokuManager.OnNotificationDismissed -= HideNotification;
            _dismissButton.clicked -= OnDismissButtonClicked;
            _acceptButton.clicked -= OnAcceptButtonClicked;
        }

        void OnAcceptButtonClicked() {
            _acceptButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _acceptButton.schedule.Execute(() => _onAccept?.Invoke()).StartingIn(200);
        }

        void OnDismissButtonClicked() {
            _dismissButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _dismissButton.schedule.Execute(() => _onDismiss?.Invoke()).StartingIn(200);
        }

        public void HideNotification(bool instant) {
            SudokuManager.SetPause(false);
            if (instant) {
                HidePanelInstant();
                return;
            }
            HidePanel();
        }


        void OnNotificationMessage(NotificationData message) {
            SudokuManager.SetPause(true);
            _onAccept = message.onConfirm;
            _onDismiss = message.onDismiss;
            _titleLabel.text = message.title;
            _messageLabel.text = message.message;
            ShowPanel();
        }
    }
}