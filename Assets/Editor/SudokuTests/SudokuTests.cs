using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Sudoku;
using System.Linq;
using UnityEditor.VersionControl;

public class SudokuTests
{
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
    [Test]
    public void TestSolveBoard() {
        var board = new SudokuBoard();
        board.RemoveCells(Difficulty.Easy);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        board.SolveBoard();
        Assert.IsTrue(board.IsBoardValid());
        Assert.IsTrue(board.IsCurrentBoardSolution());
    }

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

    [Test]
    public void TestDifficultyEasy() {
        var board = new SudokuBoard();
        board.RemoveCells(Difficulty.Easy);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Easy), board.GetBoardHoleAmount());
        board.SolveBoard();
        Assert.IsTrue(board.IsBoardValid());
        Assert.IsTrue(board.IsCurrentBoardSolution());
    }

    [Test]
    public void TestDifficultyMedium() {
        var board = new SudokuBoard();
        board.RemoveCells(Difficulty.Medium);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Medium), board.GetBoardHoleAmount());
        board.SolveBoard();
        Assert.IsTrue(board.IsBoardValid());
        Assert.IsTrue(board.IsCurrentBoardSolution());
    }

    [Test]
    public void TestDifficultyHard() {
        var board = new SudokuBoard();
        board.RemoveCells(Difficulty.Hard);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Hard), board.GetBoardHoleAmount());
        board.SolveBoard();
        Assert.IsTrue(board.IsBoardValid());
        Assert.IsTrue(board.IsCurrentBoardSolution());
    }

    [Test]
    public void TestDifficultyExpert() {
        var board = new SudokuBoard();
        board.RemoveCells(Difficulty.Expert);
        Assert.IsFalse(board.IsCurrentBoardSolution());
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Expert), board.GetBoardHoleAmount());
        board.SolveBoard();
        Assert.IsTrue(board.IsBoardValid());
        Assert.IsTrue(board.IsCurrentBoardSolution());
    }

    [Test]
    public void TestRemoveCells() {
        var board = new SudokuBoard();
        var cells = board.Cells.ToArray();
        board.RemoveCells(Difficulty.Easy);
        Assert.AreNotEqual(cells, board.Cells);
        Assert.AreEqual(SudokuBoard.GetDifficultyHoleAmount(Difficulty.Easy), board.GetBoardHoleAmount());
    }

    [Test]
    public void TestHasUniqueSolution() {
        var board = new SudokuBoard();
        Assert.IsTrue(board.HasUniqueSolution());
        var copiedSolution = board.Solution.ToArray();
        board.RemoveCells(Sudoku.Difficulty.Easy);
        Assert.IsTrue(board.HasUniqueSolution());
        Assert.AreEqual(copiedSolution, board.Solution);
    }
}
