using System.Collections.Generic;
using System.Diagnostics;

namespace GameOfLife;

class Game
{
	private static Board board;

	private static byte[] twoNeighbors = new byte[28];
	private static byte[] threeNeighbors = new byte[56];

	public delegate void BoardChangedEvent(List<Patch> patches);
	public static BoardChangedEvent BoardChanged;

	public static void InitBoard(int rows, int columns, int aliveChance)
	{
		// Lookup table init
		if (twoNeighbors.Length == 0)
		{
			ComputeTwoNeighbors();
			ComputeThreeNeighbors();
		}

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
	}

	public static void DoGeneration()
	{
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

		// Signal the GUI to update the board.
		BoardChanged(patches);
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

	private static void ComputeTwoNeighbors()
	{
		int index = 0;
		for (int i = 0; i < 7; ++i)
		{
			int bitmask = 1 << i;
			for (int j = i + 1; i < 8; ++j)
			{
				bitmask |= 1 << j;
				twoNeighbors[index++] = (byte)bitmask;
			}
		}
	}

	private static void ComputeThreeNeighbors()
	{
		int index = 0;
		for (int i = 0; i < 6; ++i)
		{
			int bitmask = 1 << i;
			for (int j = i + 1; i < 7; ++j)
			{
				bitmask |= 1 << j;
				for (int k = j + 1; k < 8; ++k)
				{
					bitmask |= 1 << k;
					threeNeighbors[index++] = (byte) bitmask;
				}
			}
		}
	}
}
