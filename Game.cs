using System.Collections.Generic;
using System.Diagnostics;

namespace GameOfLife;

class Game
{
	private static Board board;
	private static readonly Stopwatch stopwatch = new();
	private static int generation;
	private static double timeSum;

	private static bool initializedTables = false;
	private static readonly byte[] twoNeighborsTable = new byte[28];
	private static readonly byte[] threeNeighborsTable = new byte[56];

	public delegate void BoardChangedEvent(List<Patch> patches);
	public static BoardChangedEvent BoardChanged;
	public delegate void GenerationCompletedEvent(int generation, double averageTime);
	public static GenerationCompletedEvent GenerationCompleted;

	public static void InitBoard(int rows, int columns, int aliveChance)
	{
		// Lookup table init
		if (!initializedTables)
		{
			ComputeTwoNeighbors();
			ComputeThreeNeighbors();
			initializedTables = true;
		}

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
		List<Patch> patches = new();
		stopwatch.Start();
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
		
		// Any cell with three neighbors lives
		if (IsInTable((byte)neighborFlags, threeNeighborsTable))
		{
			return true;
		}

		// Any alive cell with two neighbors lives
		if (alive && IsInTable((byte)neighborFlags, twoNeighborsTable))
		{
			return true;
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
