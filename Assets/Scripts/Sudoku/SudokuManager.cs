using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Serialization;

namespace Sudoku {
    public class SudokuManager : MonoBehaviour {
        [SerializeField] Difficulty difficulty = Difficulty.Easy;
        public static event Action<SudokuBoard> OnBoardGenerated;

        public static SudokuBoard board;

        void Start() {
            GenerateBoard();
        }

        public static void GenerateBoard() {
            var processTime = Time.realtimeSinceStartup;
            board = new SudokuBoard();
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log($"Board generated in {processTime*1000} ms");
            UpdateBoard();
            Debug.Log(board);
        }

        public void GeneratePlayableBoard() {
            if (board == null) {
                GenerateBoard();
            }

            board?.RemoveCells(difficulty);
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
                SudokuManager.GenerateBoard();
            }

            if (GUILayout.Button("Solve Board")) {
                SudokuManager.SolveBoard();
            }

            if (GUILayout.Button("Generate Playable Board")) {
                ((SudokuManager) target).GeneratePlayableBoard();
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
}