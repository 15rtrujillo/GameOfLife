namespace GameOfLife;

struct Patch
{
	public byte Row { get; set; }
	public byte Column { get; set; }
	public bool NewValue { get; set; }
}