using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Sudoku;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor.VersionControl;

public class SudokuTests {
    readonly IReadOnlyList<int> _fourSolutionBoardValues = new[] {
        2, 3, 0, 0, 0, 0, 4, 9, 1,
        0, 0, 8, 0, 9, 1, 0, 5, 0,
        4, 9, 1, 3, 0, 2, 7, 8, 0,
        0, 5, 0, 8, 3, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 9, 4, 0,
        3, 0, 9, 2, 0, 4, 0, 6, 5,
        0, 7, 0, 0, 0, 0, 1, 0, 8,
        9, 0, 0, 0, 2, 0, 0, 0, 0,
        0, 0, 0, 0, 8, 7, 0, 0, 0
    };

    [Test]
    public void TestNumberOfSolutions() {
        var board     = new SudokuBoard(_fourSolutionBoardValues);
        var solutions = board.GetNumberOfSolutions();
        Assert.AreEqual(4, solutions);
    }

    // Tests the population of the board and that all cells have a valid value
    [Test]
    public void TestPopulateBoard() {
        var board = new SudokuBoard();
        Assert.NotNull(board.Cells);
        Assert.AreEqual(SudokuBoard.BOARD_SIZE * SudokuBoard.BOARD_SIZE, board.Cells.Length);
        foreach (var cell in board.Cells) {
            Assert.Greater(cell.value, 0);
            Assert.LessOrEqual(cell.value, SudokuBoard.BOARD_SIZE);
            Assert.IsTrue(board.IsCellValid(cell));
        }
    }

// Tests the solving of the board
    [UnityTest]
    public IEnumerator TestSolveBoard() => UniTask.ToCoroutine(async () => {
        var board = new SudokuBoard();
        await board.RemoveCells(Difficulty.Easy);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        board.SolveBoard();
        Assert.IsTrue(board.IsBoardValid());
        Assert.IsTrue(board.IsCurrentBoardSolution());
    });

    [Test]
    public void TestIsBoardValid() {
        var board = new SudokuBoard();
        Assert.IsTrue(board.IsBoardValid());
        board.Cells[0].value = 0;
        Assert.IsFalse(board.IsBoardValid());
    }

    [Test]
    public void TestIsCurrentBoardSolution() {
        var board = new SudokuBoard();
        Assert.IsTrue(board.IsCurrentBoardSolution());
        board.Cells[0].value = 0;
        Assert.IsFalse(board.IsCurrentBoardSolution());
    }

    [Test]
    public void TestIsCellValid() {
        var board = new SudokuBoard();
        Assert.IsTrue(board.IsCellValid(board.Cells[0]));
        board.Cells[0].value = 0;
        Assert.IsFalse(board.IsCellValid(board.Cells[0]));
    }

    [UnityTest]
    public IEnumerator TestDifficultyEasy() => UniTask.ToCoroutine(async () => {
        var board = new SudokuBoard();
        await board.RemoveCells(Difficulty.Easy);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Easy), board.GetBoardHoleAmount());
    });

    [UnityTest]
    public IEnumerator TestDifficultyMedium() => UniTask.ToCoroutine(async () => {
        var board = new SudokuBoard();
        await board.RemoveCells(Difficulty.Medium);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Medium), board.GetBoardHoleAmount());
    });

    [UnityTest]
    public IEnumerator TestDifficultyHard() => UniTask.ToCoroutine(async () => {
        var board = new SudokuBoard();
        await board.RemoveCells(Difficulty.Hard);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Hard), board.GetBoardHoleAmount());
    });

    [UnityTest]
    public IEnumerator TestDifficultyExpert() => UniTask.ToCoroutine(async () => {
        var board = new SudokuBoard();
        await board.RemoveCells(Difficulty.Expert);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Expert), board.GetBoardHoleAmount());
    });

    [UnityTest]
    public IEnumerator TestRemoveCells() => UniTask.ToCoroutine(async () => {
        var board = new SudokuBoard();
        var cells = board.Cells.ToArray();
        await board.RemoveCells(Difficulty.Easy);
        Assert.AreNotEqual(cells, board.Cells);
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Easy), board.GetBoardHoleAmount());
    });

    [UnityTest]
    public IEnumerator TestHasUniqueSolution() => UniTask.ToCoroutine(async () => {
        var board = new SudokuBoard();
        Assert.IsTrue(board.HasUniqueSolution());
        var copiedSolution = board.Solution.ToArray();
        await board.RemoveCells(Sudoku.Difficulty.Easy);
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(copiedSolution, board.Solution);
    });
}