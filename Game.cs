using System.Collections.Generic;
using System.Diagnostics;

namespace GameOfLife;

class Game
{
    private static Board board;
	private static readonly Stopwatch stopwatch = new();
	private static int generation;
	private static double timeSum;

    public delegate void BoardChangedEvent(List<Patch> patches);
    public static BoardChangedEvent BoardChanged;
	public delegate void GenerationCompletedEvent(int generation, double averageTime);
	public static GenerationCompletedEvent GenerationCompleted;

	public static void InitBoard(int rows, int columns, int aliveChance)
	{
        generation = 0;
        timeSum = 0.0;

		board = new Board(rows, columns);
		board.RandomInitialization(aliveChance);

        // Generate initial patches
        List<Patch> patches = new();
        for (int row = 0; row < board.Rows; ++row)
        {
            for (int column = 0; column < board.Columns; ++column)
            {
                if (!board[row, column])
                {
                    continue;
                }

                patches.Add(new()
                {
                    Row = (byte)row,
                    Column = (byte)column,
                    NewValue = board[row, column]
                });
            }
        }

		BoardChanged(patches);
		GenerationCompleted(generation, timeSum);
	}

	public static void DoGeneration()
	{
        stopwatch.Start();
		List<Patch> patches = new();
		for (int row = 0; row < board.Rows; ++row)
		{
			for (int column = 0; column < board.Columns; ++column)
			{
				bool currentValue = board[row, column];
				bool newValue = CheckAdjacentTiles(row, column, currentValue);

                if (currentValue != newValue)
                {
                    patches.Add(new()
                    {
                        Row = (byte)row,
                        Column = (byte)column,
                        NewValue = newValue
                    });
                }
            }
        }

		// Use the patches to update the board
		foreach (Patch patch in patches)
		{
			board[patch.Row, patch.Column] = patch.NewValue;
		}
        stopwatch.Stop();

		// Signal the GUI to update the board.
		BoardChanged(patches);

		// Calculate the average compute time
		double computeTime = stopwatch.Elapsed.TotalMilliseconds;
		stopwatch.Reset();
		timeSum += computeTime;
		++generation;

		// Signal the GUI
		GenerationCompleted(generation, timeSum / generation);
	}

    private static bool CheckAdjacentTiles(int row, int column, bool alive)
    {
        int adjacentAlive = 0;
        for (int checkRow = -1; checkRow <= 1; ++checkRow)
        {
            for (int checkColumn = -1; checkColumn <= 1; ++checkColumn)
            {
                // We don't want to check ourself
                if (checkRow == 0 && checkColumn == 0)
                {
                    continue;
                }

                if (board[row + checkRow, column + checkColumn])
                {
                    ++adjacentAlive;
                }

                // Any live cell with more than 3 alive neighbors dies.
                if (alive && adjacentAlive > 3)
                {
                    return false;
                }
            }
        }

        // Any live cell...
        if (alive)
        {
            // ...with less than two live neighbors dies.
            if (adjacentAlive < 2)
            {
                return false;
            }

            // ...with two or three live neighbors lives on.
            else
            {
                return true;
            }
        }

        // Any dead cell with exactly three live neighbors becomes alive.
        else if (adjacentAlive == 3)
        {
            return true;
        }

        return false;
    }
}
