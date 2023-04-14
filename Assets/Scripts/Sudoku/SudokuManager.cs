using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Serialization;

namespace Sudoku {
    public class SudokuManager : MonoBehaviour {
        public static event Action<SudokuBoard> OnBoardGenerated;

        public static SudokuBoard board;

        void Start() {
            GenerateBoard();
        }

        public static void GenerateBoard() {
            board = new SudokuBoard();
            UpdateBoard();
            Debug.Log(board);
        }

        public static void SolveBoard() {
            board.PopulateBoard(true);
            UpdateBoard();
        }

        public static void UpdateBoard() => OnBoardGenerated?.Invoke(board);
    }

    [CustomEditor(typeof(SudokuManager))]
    public class SudokuManagerEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            if (SudokuManager.board == null) return;
            foreach (var cell in SudokuManager.board.Cells) {
                if (cell.Index % 9 == 0) {
                    EditorGUILayout.BeginHorizontal();
                }
                if (GUILayout.Button($"{cell.Value}", GUILayout.Width(50))) {
                    cell.Value = 0;
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

            if (GUILayout.Button("Check Solutions")) {
                SudokuManager.board.HasUniqueSolution();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}