using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Sudoku {
    public class SudokuManager : MonoBehaviour {
        public static event Action<SudokuBoard> OnBoardGenerated;

        SudokuBoard _board;

        void Start() {
            _board = new SudokuBoard();
            OnBoardGenerated?.Invoke(_board);
            Debug.Log(_board);
        }
    }
}