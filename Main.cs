using System;
using System.Collections.Generic;
using Godot;

namespace  GameOfLife;

public partial class Main : Control
{
	private enum State
	{
		Stopped,
		Running
	}

	[ExportCategory("Game Configuration")]
	[Export]
	public int InitialAliveChance { get; set; } = 50;
	[Export]
	public double GenerationInterval { get; set; } = 1.0;
	[Export]
	public int Rows { get; set; } = 10;
	[Export]
	public int Columns { get; set; } = 10;
	[Export]
	public Color AliveColor { get; set; } = Colors.White;
	[Export]
	public Color DeadColor { get; set; } = Colors.Black;

	[ExportCategory("Export Nodes")]
	[Export]
	private Label _labelTitle;
	[Export]
	private GridContainer _grid;
	[Export]
	private Label _labelGeneration;
	[Export]
	private SpinBox _spinBoxInitialAlive;
	[Export]
	private SpinBox _spinBoxInterval;
	[Export]
	private SpinBox _spinBoxRows;
	[Export]
	private SpinBox _spinBoxColumns;
	[Export]
	private Button _buttonRestart;
	[Export]
	private Button _buttonPlayPause;

	private State _state = State.Stopped;
	private ColorRect[,] _tiles;
	private Timer _timer;
	private int _generation;

	public override void _Ready()
	{
		Game.BoardChanged += UpdateBoard;

		_spinBoxInitialAlive.Value = InitialAliveChance;
		_spinBoxInterval.Value = GenerationInterval * 1000;
		_spinBoxRows.Value = Rows;
		_spinBoxColumns.Value = Columns;        

		_buttonRestart.Pressed += RestartButtonPressed;
		_buttonPlayPause.Pressed += PlayPauseButtonPressed;

		Setup();
	}

	private void Setup()
	{
		// Setup GUI
		_buttonPlayPause.Text = "Start";
		UpdateGenerationLabel();

		_grid.Columns = Columns;

		_tiles = new ColorRect[Rows, Columns];
		for (int row = 0; row < Rows; ++row)
		{
			for (int column = 0; column < Columns; ++column)
			{
				ColorRect rect = new()
				{
					Color = DeadColor,
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					SizeFlagsVertical = SizeFlags.ExpandFill
				};
				
				_tiles[row, column] = rect;
				_grid.AddChild(rect);
			}
		}

		// Setup timer
		_timer = new()
		{
			WaitTime = GenerationInterval
		};
		_timer.Timeout += TimerTimeout;
		AddChild(_timer);

		// Setup data
		_generation = 0;
		Game.InitBoard(Rows, Columns, InitialAliveChance);
	}

	private void UpdateBoard(List<Patch> patches)
	{
		foreach (Patch patch in patches)
		{
			_tiles[patch.Row, patch.Column].Color = patch.NewValue ? AliveColor : DeadColor;
		}
	}

	private void RestartButtonPressed()
	{
		Pause();

		InitialAliveChance = (int)_spinBoxInitialAlive.Value;
		GenerationInterval = _spinBoxInterval.Value / 1000.0;
		Rows = (int)_spinBoxRows.Value;
		Columns = (int)_spinBoxColumns.Value;

		_timer.Timeout -= TimerTimeout;
		_timer.QueueFree();
		_timer = null;

		foreach (ColorRect rect in _tiles)
		{
			rect.QueueFree();
		}
		_tiles = null;

		Setup();
	}

	private void PlayPauseButtonPressed()
	{
		if (_state == State.Running)
		{
			Pause();
		}

		else
		{
			Resume();
		}
	}

	private void TimerTimeout()
	{
		++_generation;
		UpdateGenerationLabel();

		Game.DoGeneration();
	}

	private void Pause()
	{
		_timer.Stop();
		_buttonPlayPause.Text = "Play";
		_state = State.Stopped;
	}

	private void Resume()
	{
		_timer.Start();
		_buttonPlayPause.Text = "Pause";
		_state = State.Running;
	}

	public void UpdateGenerationLabel()
	{
		_labelGeneration.Text = $"Generation: {_generation}";
	}
}
