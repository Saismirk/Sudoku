using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;
using Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sudoku {
    public class SudokuManager : MonoBehaviour {
        public const     int                         MAX_FAILS         = 5;
        [SerializeField] Difficulty                  difficultySetting = Difficulty.Hard;
        public static event Action                   OnBoardGenerationStarted;
        public static event Action                   OnBoardGenerationFinished;
        public static event Action<SudokuBoard>      OnBoardGenerated;
        public static event Action                   OnBoardPlayable;
        public static event Action                   OnGameStarted;
        public static event Action<NotificationData> OnNotificationMessage;
        public static event Action<bool>             OnNotificationDismissed;
        public static event Action<Cell>             OnCellSelected;
        public static event Action<bool>             OnGamePaused;

        public static CancellationToken CancellationToken   { get; private set; }
        public static SudokuTimer       Timer               { get; private set; } = new();
        public static SudokuBoard       Board               { get; private set; }
        public static Cell              CurrentSelectedCell { get; private set; }
        public static string            TimerText           { get; private set; }

        public static Observable<Difficulty> DifficultySetting { get; private set; } = Difficulty.Hard;
        public static Observable<int>        Attempts          { get; private set; } = 0;

        public static bool IsPaused => Timer?.IsPaused == true;

        void Start() {
            CancellationToken = this.GetCancellationTokenOnDestroy();
            GenerateBoard(false).Forget();
            DifficultySetting.Value = difficultySetting;
            Application.targetFrameRate = 120;
            Attempts.OnChanged += OnAttemptsChanged;
        }

        static void OnAttemptsChanged(int attempts) {
            if (attempts < MAX_FAILS) return;
            Debug.Log("Game Over");
            PushNotification(new NotificationData {
                title = "Game Over",
                message = "You have failed too many times. Try again?",
                onConfirm = () => DismissAndRestartAsync().Forget(),
                onDismiss = () => {
                    OnNotificationDismissed?.Invoke(false);
                    Application.Quit();
                }
            });
        }

        public static async UniTaskVoid DismissAndRestartAsync() {
            RestartBoard().Forget();
            await UniTask.Delay(500);
            DismissNotification(true);
        }

        public static async UniTask GenerateBoard(bool populateBoard) {
            var processTime = Time.realtimeSinceStartup;
            Board = new SudokuBoard(populateBoard);
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log($"Board generated in {processTime * 1000} ms");
            Timer?.StopTimer();
            await UniTask.Yield(PlayerLoopTiming.Update);
            DifficultySetting.Update();
            UpdateBoard();
            Debug.Log(Board);
        }

        public static async UniTask GeneratePlayableBoard() {
            if (Board == null) {
                await GenerateBoard(true);
            }

            Timer?.StopTimer();
            await UniTask.Yield(PlayerLoopTiming.Update);
            var time = Time.realtimeSinceStartup;
            if (Board != null) await Board.RemoveCells(DifficultySetting);
            time = Time.realtimeSinceStartup - time;
            Debug.Log($"Generated playable board in {time * 1000} ms ({DifficultySetting.Value})");
            Timer?.StartTimer(CancellationToken);
            UpdateBoard();
            OnBoardPlayable?.Invoke();
        }

        public static void TogglePauseTimer(bool raiseEvent = true) {
            if (Timer.IsPaused) {
                Timer.ResumeTimer();
                if (raiseEvent) OnGamePaused?.Invoke(false);
            } else {
                Timer.PauseTimer();
                if (raiseEvent) OnGamePaused?.Invoke(true);
            }
        }

        public static void SetPause(bool pause) {
            if (pause) {
                Timer.PauseTimer();
                OnGamePaused?.Invoke(true);
            } else {
                Timer.ResumeTimer();
                OnGamePaused?.Invoke(false);
            }
        }

        public static void SetDifficulty(Difficulty difficulty) => DifficultySetting.Value = difficulty;

        public static void SolveBoard() {
            var processTime = Time.realtimeSinceStartup;
            Board.SolveBoard();
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log($"Board solved in {processTime * 1000} ms");
            UpdateBoard();
        }

        public static void UpdateBoard() => OnBoardGenerated?.Invoke(Board);

        public static async UniTaskVoid RestartBoard() {
            OnBoardGenerationStarted?.Invoke();
            Attempts.Value = 0;
            await UniTask.Delay(500);
            await GenerateBoard(true);
            await GeneratePlayableBoard();
            await UniTask.Delay(200);
            OnBoardGenerationFinished?.Invoke();
        }

        public static void StartNewGame(Difficulty difficulty) {
            DifficultySetting.Value = difficulty;
            RestartBoard().Forget();
            OnBoardPlayable?.Invoke();
        }

        public static void StartGame() {
            OnGameStarted?.Invoke();
        }

        public static void PushNotification(NotificationData data)   => OnNotificationMessage?.Invoke(data);
        public static void DismissNotification(bool instant = false) => OnNotificationDismissed?.Invoke(instant);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SudokuManager))]
    public class SudokuManagerEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("difficultySetting"));
            if (SudokuManager.Board == null) return;
            for (var index = 0; index < SudokuManager.Board.Cells.Length; index++) {
                var cell = SudokuManager.Board.Cells[index];
                if (cell.Index % 9 == 0) {
                    EditorGUILayout.BeginHorizontal();
                }

                if (GUILayout.Button($"{cell.value}", GUILayout.Width(50))) {
                    cell.value = 0;
                    SudokuManager.Board.Cells[index] = cell;
                    SudokuManager.UpdateBoard();
                }

                if (cell.Index % 9 == 8) {
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Generate Board")) {
                SudokuManager.GenerateBoard(true).Forget();
            }

            if (GUILayout.Button("Solve Board")) {
                SudokuManager.SolveBoard();
            }

            if (GUILayout.Button("Generate Playable Board")) {
                SudokuManager.GeneratePlayableBoard().Forget();
            }

            if (GUILayout.Button("Check Solutions")) {
                SudokuManager.Board.HasUniqueSolution();
            }

            if (GUILayout.Button("Reset Board")) {
                SudokuManager.Board.ApplyKnownSolution();
                SudokuManager.UpdateBoard();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}