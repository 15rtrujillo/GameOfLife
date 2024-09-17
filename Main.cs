using Godot;

namespace GameOfLife;

public partial class Main : Control
{
    [Export]
    public int Rows { get; set; }
    [Export]
    public int Columns { get; set; }

    private GridContainer _grid;
    private ColorRect[,] _tiles;

    public override void _Ready()
    {
        _grid = GetNode<GridContainer>("MarginContainer/Grid");
        _grid.Columns = Columns;

        _tiles = new ColorRect[Rows, Columns];
        for (int row = 0; row < Rows; ++row)
        {
            for (int column = 0; column < Columns; ++column)
            {
                ColorRect rect = new()
                {
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    SizeFlagsVertical = SizeFlags.ExpandFill
                };
                
                _tiles[row, column] = rect;
                _grid.AddChild(rect);
            }
        }
    }
}
