using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sudoku {
    public class SudokuManager : MonoBehaviour {
        [SerializeField] Difficulty difficulty = Difficulty.Easy;
        public static event Action<SudokuBoard> OnBoardGenerated;

        public static SudokuBoard board;

        public static Cell CurrentSelectedCell { get; private set; }

        void Start() {
            GenerateBoard().Forget();
            Application.targetFrameRate = 120;
        }

        public static async UniTask GenerateBoard() {
            var processTime = Time.realtimeSinceStartup;
            board = new SudokuBoard();
            await UniTask.Yield();
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log($"Board generated in {processTime*1000} ms");
            UpdateBoard();
            Debug.Log(board);
        }

        public async UniTask GeneratePlayableBoard() {
            if (board == null) {
                await GenerateBoard();
            }
            await UniTask.Yield();
            if (board != null) await board.RemoveCells(difficulty);
            UpdateBoard();
        }

        public static void SolveBoard() {
            var processTime = Time.realtimeSinceStartup;
            board.SolveBoard();
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log($"Board solved in {processTime*1000} ms");
            UpdateBoard();
        }

        public static void UpdateBoard() => OnBoardGenerated?.Invoke(board);
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(SudokuManager))]
    public class SudokuManagerEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("difficulty"));
            if (SudokuManager.board == null) return;
            for (var index = 0; index < SudokuManager.board.Cells.Length; index++) {
                var cell = SudokuManager.board.Cells[index];
                if (cell.Index % 9 == 0) {
                    EditorGUILayout.BeginHorizontal();
                }

                if (GUILayout.Button($"{cell.value}", GUILayout.Width(50))) {
                    cell.value = 0;
                    SudokuManager.board.Cells[index] = cell;
                    SudokuManager.UpdateBoard();
                }

                if (cell.Index % 9 == 8) {
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Generate Board")) {
                SudokuManager.GenerateBoard().Forget();
            }

            if (GUILayout.Button("Solve Board")) {
                SudokuManager.SolveBoard();
            }

            if (GUILayout.Button("Generate Playable Board")) {
                ((SudokuManager) target).GeneratePlayableBoard().Forget();
            }

            if (GUILayout.Button("Check Solutions")) {
                SudokuManager.board.HasUniqueSolution();
            }

            if (GUILayout.Button("Reset Board")) {
                SudokuManager.board.ApplyKnownSolution();
                SudokuManager.UpdateBoard();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}