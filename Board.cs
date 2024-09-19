using System;
using System.Collections.Generic;

namespace GameOfLife;

public class Board
{
	private static readonly Random random = new();

	public int Rows { get; private set; }
	public int Columns { get; private set; }

	private static readonly Dictionary<int, int> actualRowLookup = new();
	private static readonly Dictionary<int, int> actualColumnLookup = new();
	private static readonly Dictionary<int, long> columnBitmaskLookup = new();

	private readonly long[,] _board;
	private readonly int _actualColumns;

	public Board(int rows, int columns)
	{
		Rows = rows;
		Columns = columns;
		_actualColumns = columns / 64 + 1;
		_board = new long[rows, _actualColumns];
	}

	public bool this[int row, int column]
	{
		get
		{
			int actualColumn = LookupActualColumn(column, _actualColumns);
			long bitmask = LookupBitmask(column);
			return (_board[LookupActualRow(row, Rows), actualColumn] & bitmask) != 0;
		}

		set
		{
			int actualRow = LookupActualRow(row, Rows);
			int actualColumn = LookupActualColumn(column, _actualColumns);
			long bitmask = LookupBitmask(column);
			if (value)
			{
				_board[actualRow, actualColumn] = _board[actualRow, actualColumn] | bitmask;
			}

			else
			{
				_board[actualRow, actualColumn] = _board[actualRow, actualColumn] & ~bitmask;
			}
		}
	}

	public void RandomInitialization(int aliveChance)
	{
		for (int row = 0; row < Rows; ++row)
		{
			for (int column = 0; column < Columns; ++column)
			{
				this[row, column] = random.Next(1, 101) <= aliveChance;
			}
		}
	}

	private static int Mod(int a, int b)
	{
		return a - b * (int)Math.Floor((double)a / b);
	}

	private static int LookupActualRow(int row, int rows)
	{
		if (!actualRowLookup.TryGetValue(row, out int actualRow))
		{
			actualRow = Mod(row, rows);
			actualRowLookup[row] = actualRow;
		}

		return actualRow;
	}

	private static int LookupActualColumn(int column, int columns)
	{
		if (!actualColumnLookup.TryGetValue(column, out int actualColumn))
		{
			actualColumn = Mod((int)Math.Floor(column / 64.0), columns);
			actualColumnLookup[column] = actualColumn;
		}

		return actualColumn;
	}

	private static long LookupBitmask(int column)
	{
		if (!columnBitmaskLookup.TryGetValue(column, out long bitmask))
		{
			bitmask = 1 << (63 - Mod(column, 64));
			columnBitmaskLookup[column] = bitmask;
		}

		return bitmask;
	}
}
