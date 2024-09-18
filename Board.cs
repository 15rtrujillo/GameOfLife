using System;
using System.Collections;

namespace GameOfLife;

public class Board : IEnumerable
{
    private static Random _random = new();

    private readonly bool[,] _board;

    public int Rows { get; private set; }
    public int Columns { get; private set; }

    public Board(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _board = new bool[Rows, Columns];
    }

    public bool this[int row, int column]
    {
        get
        {
            return _board[Mod(row, Rows), Mod(column, Columns)];
        }

        set
        {
            _board[Mod(row, Rows), Mod(column, Columns)] = value;
        }
    }

    public void RandomInitialization(int aliveChance)
    {
        for (int row = 0; row < Rows; ++row)
        {
            for (int column = 0; column < Columns; ++column)
            {
                _board[row, column] = _random.Next(1, 101) <= aliveChance;
            }
        }
    }

    private static int Mod(int a, int b)
    {
        return a - b * (int)Math.Floor((double)a / b);
    }

    public IEnumerator GetEnumerator()
    {
        return _board.GetEnumerator();
    }
}
