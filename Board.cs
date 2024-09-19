using System;
using System.Collections.Generic;

namespace GameOfLife;

public class Board
{
	private static readonly Random random = new();

	public int Rows { get; private set; }
	public int Columns { get; private set; }

	private readonly Dictionary<int, int> _actualRowLookup = new();
	private readonly Dictionary<int, int> _actualColumnLookup = new();
	private readonly Dictionary<int, long> _columnBitmaskLookup = new();

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
			int actualColumn = LookupActualColumn(column);
			long bitmask = LookupBitmask(column);
			return (_board[LookupActualRow(row), actualColumn] & bitmask) != 0;
		}

		set
		{
			int actualRow = LookupActualRow(row);
			int actualColumn = LookupActualColumn(column);
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

	private int LookupActualRow(int row)
	{
		if (!_actualRowLookup.TryGetValue(row, out int actualRow))
		{
			if (row < 0)
			{
				if (row < -Rows)
				{
					actualRow = Rows + row / Rows;
				}

				else
				{
					actualRow = Rows + row;
				}
			}

			else if (row > 0)
			{
				actualRow = row % Rows;
			}

			else
			{
				actualRow = 0;
			}

			_actualRowLookup[row] = actualRow;
		}

		return actualRow;
	}

	private int LookupActualColumn(int column)
	{
		if (!_actualColumnLookup.TryGetValue(column, out int actualColumn))
		{
			if (column < 0)
			{
				actualColumn = _actualColumns + (int)Math.Floor(column / 64.0);
			}

			else if (column > 0)
			{
				actualColumn = (int)Math.Floor(column / 64.0) % _actualColumns;
			}
			
			else
			{
				actualColumn = 0;
			}

			_actualColumnLookup[column] = actualColumn;
		}

		return actualColumn;
	}

	private long LookupBitmask(int column)
	{
		if (!_columnBitmaskLookup.TryGetValue(column, out long bitmask))
		{
			if (column < 0)
			{
				// This will break on negative numbers that are too high, but I don't care right now.
				bitmask = 1L << (-column - 1);
			}

			else if (column > 0)
			{
				bitmask = 1L << (63 - column % 64);
			}

			else
			{
				bitmask = 1L << 63;
			}

			_columnBitmaskLookup[column] = bitmask;
		}

		return bitmask;
	}
}
