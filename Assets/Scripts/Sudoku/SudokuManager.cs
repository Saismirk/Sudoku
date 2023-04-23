using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sudoku {
    public class SudokuManager : MonoBehaviour {
        [SerializeField] Difficulty                  difficulty = Difficulty.Easy;
        public static event Action<SudokuBoard>      OnBoardGenerated;
        public static event Action                   OnBoardPlayable;
        public static event Action<NotificationData> OnNotificationMessage;
        public static event Action                   OnNotificationDismissed;
        public static event Action<Cell>             OnCellSelected;
        public static event Action<bool>             OnGamePaused;

        public static CancellationToken CancellationToken   { get; private set; }
        public static SudokuTimer       Timer               { get; private set; } = new();
        public static SudokuBoard       Board               { get; private set; }
        public static Cell              CurrentSelectedCell { get; private set; }
        public static string            TimerText           { get; private set; }

        public static Difficulty Difficulty { get; private set; }

        public static bool IsPaused => Timer?.IsPaused == true;

        void Start() {
            CancellationToken = this.GetCancellationTokenOnDestroy();
            GenerateBoard(false).Forget();
            Application.targetFrameRate = 120;
        }

        public static async UniTask GenerateBoard(bool populateBoard) {
            var processTime = Time.realtimeSinceStartup;
            Board = new SudokuBoard(populateBoard);
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log($"Board generated in {processTime * 1000} ms");
            Timer?.StopTimer();
            await UniTask.Yield(PlayerLoopTiming.Update);
            UpdateBoard();
            Debug.Log(Board);
        }

        public static async UniTask GeneratePlayableBoard() {
            if (Board == null) {
                await GenerateBoard(true);
            }

            Timer?.StopTimer();
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (Board != null) await Board.RemoveCells(Difficulty);
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

        public static void SolveBoard() {
            var processTime = Time.realtimeSinceStartup;
            Board.SolveBoard();
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log($"Board solved in {processTime * 1000} ms");
            UpdateBoard();
        }

        public static void UpdateBoard() => OnBoardGenerated?.Invoke(Board);

        public static async UniTask RestartBoard() {
            await GenerateBoard(true);
            await GeneratePlayableBoard();
        }

        public static void PushNotification(NotificationData data) => OnNotificationMessage?.Invoke(data);
        public static void DismissNotification() => OnNotificationDismissed?.Invoke();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SudokuManager))]
    public class SudokuManagerEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("difficulty"));
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