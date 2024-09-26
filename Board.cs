using System;

namespace GameOfLife;

public class Board
{
	private static readonly Random random = new();

	public int Rows { get; private set; }
	public int Columns { get; private set; }

	private readonly byte[] _board;
	private readonly int _actualColumns;

	public Board(int rows, int columns)
	{
		Rows = rows;
		Columns = columns;
        _actualColumns = (int)Math.Ceiling((double)Columns / sizeof(byte));
		_board = new byte[Rows * _actualColumns];
	}

	public bool this[int row, int column]
	{
		get
		{
            int location = BoundDimension(column / 8, _actualColumns) + (_actualColumns * BoundDimension(row, Rows));
			byte bitmask = CalculateBitmask(column, Columns);
			return (_board[location] & bitmask) != 0;
		}

		set
		{
			int location = BoundDimension(column / 8, _actualColumns) + (_actualColumns * BoundDimension(row, Rows));
			byte bitmask = CalculateBitmask(column, Columns);
            byte currentValue = _board[location];
			if (value)
			{
				_board[location] = (byte)(currentValue | bitmask);
			}

			else
			{
				_board[location] = (byte)(currentValue & ~bitmask);
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

    private static int BoundDimension(int dimension, int max)
    {
        if (dimension >= max)
        {
            return 0;
        }

        if (dimension < 0)
        {
            return max - 1;
        }

        return dimension;
    }

    private static byte CalculateBitmask(int position, int max)
    {
        if (position < 0)
        {
            position = max - 1;
        }

        else if (position >= max)
        {
            position = 0;
        }

        return (byte)(1 << (position % sizeof(byte)));
    }
}
