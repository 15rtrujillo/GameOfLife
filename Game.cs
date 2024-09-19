using System.Collections.Generic;
using System.Diagnostics;

namespace GameOfLife;

class Game
{
	private static Board board;

	private static bool initializedTables = false;
	private static readonly byte[] twoNeighborsTable = new byte[28];
	private static readonly byte[] threeNeighborsTable = new byte[56];

	public delegate void BoardChangedEvent(List<Patch> patches);
	public static BoardChangedEvent BoardChanged;

	public static void InitBoard(int rows, int columns, int aliveChance)
	{
		// Lookup table init
		if (!initializedTables)
		{
			ComputeTwoNeighbors();
			ComputeThreeNeighbors();
			initializedTables = true;
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
		int neighborFlags = 0;
		int bitIndex = 0;
		for (int checkedRow = -1; checkedRow <= 1; ++checkedRow)
		{
			for (int checkedColumn = -1; checkedColumn <= 1; ++checkedColumn)
			{
				// Don't check the current tile
				if (checkedRow == 0 && checkedColumn == 0)
				{
					continue;
				}

				if (board[row + checkedRow, column + checkedColumn])
				{
					neighborFlags |= 1 << bitIndex;
				}
				++bitIndex;
			}
		}
		
		// Any alive cell...
		if (alive)
		{
			// ...with two or three live neighbors lives on
			if (IsInTable((byte)neighborFlags, twoNeighborsTable) || IsInTable((byte)neighborFlags, threeNeighborsTable))
			{
				return true;
			}

			// ...with less than two or more than three live neighbors dies.
			else
			{
				return false;
			}
		}

		else
		{
			// Any dead cell with exactly three live neighbors becomes alive.
			if (IsInTable((byte)neighborFlags, threeNeighborsTable))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsInTable(byte neighbors, byte[] table)
	{
		foreach (byte perm in table)
		{
			if (perm == neighbors)
			{
				return true;
			}
		}

		return false;
	}

	private static void ComputeTwoNeighbors()
	{
		int index = 0;
		for (int i = 0; i < 7; ++i)
		{
			int bitmask = 1 << i;
			for (int j = i + 1; j < 8; ++j)
			{
				bitmask |= 1 << j;
				twoNeighborsTable[index++] = (byte)bitmask;

				// Unset the bit so we can move on
				bitmask &= ~(1 << j);
			}
		}
	}

	private static void ComputeThreeNeighbors()
	{
		int index = 0;
		for (int i = 0; i < 6; ++i)
		{
			int bitmask = 1 << i;
			for (int j = i + 1; j < 7; ++j)
			{
				bitmask |= 1 << j;
				for (int k = j + 1; k < 8; ++k)
				{
					bitmask |= 1 << k;
					threeNeighborsTable[index++] = (byte)bitmask;

					// Unset the bit so we can move on
					bitmask &= ~(1 << k);
				}

				// Unset the bit so we can move on
				bitmask &= ~(1 << j);
			}
		}
	}
}
