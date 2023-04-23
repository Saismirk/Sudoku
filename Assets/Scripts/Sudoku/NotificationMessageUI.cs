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

    [RequireComponent(typeof(UIDocument))]
    public class NotificationMessageUI : MonoBehaviour {
        const string HIDDEN_CLASS  = "sudoku--hidden";
        const string REMOVED_CLASS = "sudoku--removed";

        VisualElement Root => _messageUI?.rootVisualElement;

        UIDocument _messageUI;

        Label  _titleLabel;
        Label  _messageLabel;
        Button _acceptButton;
        Button _dismissButton;
        VisualElement _basePanel;

        Action _onAccept;
        Action _onDismiss;

        void Awake() {
            _messageUI = GetComponent<UIDocument>();
            _messageUI.enabled = true;
        }

        void OnEnable() {
            SudokuManager.OnNotificationMessage += OnNotificationMessage;
            SudokuManager.OnNotificationDismissed += HideNotificationMessage;
            _dismissButton = _messageUI.rootVisualElement.Q<Button>("DismissButton");
            _dismissButton.clicked += OnDismissButtonClicked;
            _titleLabel = Root.Q<Label>("TitleLabel");
            _basePanel = Root.Q<VisualElement>("Background");
            _basePanel.AddToClassList(REMOVED_CLASS);
            _messageLabel = Root.Q<Label>("MessageLabel");
            _acceptButton = Root.Q<Button>("AcceptButton");
            _dismissButton = Root.Q<Button>("DismissButton");
            _acceptButton.clicked += OnAcceptButtonClicked;
        }

        void OnAcceptButtonClicked() {
            _acceptButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _acceptButton.schedule.Execute(() => _onAccept?.Invoke()).StartingIn(200);
        }

        void OnDismissButtonClicked() {
            _dismissButton.AddTemporaryClass("sudoku-button--pressed", 100);
            _dismissButton.schedule.Execute(() => _onDismiss?.Invoke()).StartingIn(200);
        }

        public void HideNotificationMessage() {
            _basePanel.AddToClassList(HIDDEN_CLASS);
            _basePanel.schedule.Execute(() => _basePanel.AddToClassList(REMOVED_CLASS)).StartingIn(200);
        }

        public void ShowNotificationMessage() {
            _basePanel.RemoveFromClassList(REMOVED_CLASS);
            _basePanel.schedule.Execute(() => _basePanel.RemoveFromClassList(HIDDEN_CLASS)).StartingIn(100);
        }

        void OnDisable() {
            SudokuManager.OnNotificationMessage -= OnNotificationMessage;
            SudokuManager.OnNotificationDismissed -= HideNotificationMessage;
            _dismissButton.clicked -= OnDismissButtonClicked;
            _acceptButton.clicked -= OnAcceptButtonClicked;
        }

        void OnNotificationMessage(NotificationData message) {
            _onAccept = message.onConfirm;
            _onDismiss = message.onDismiss;
            _titleLabel.text = message.title;
            _messageLabel.text = message.message;
            ShowNotificationMessage();
        }

    }
}