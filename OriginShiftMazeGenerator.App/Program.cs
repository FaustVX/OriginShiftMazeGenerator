// See https://aka.ms/new-console-template for more information
using OriginShiftMazeGenerator.Core;

Console.WriteLine("Hello, World!");

var generator = new Generator<Cell>(GenerateCells(150, 30));

generator.Setup(0);

await DrawLoop(generator, TimeSpan.FromMilliseconds(0));

static Cell[,] GenerateCells(int width, int height)
{
    var maze = new Cell[width, height];
    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maze[x, y] = new Cell() { Pos = (x, y) };
    return maze;
}

static async Task DrawLoop(Generator<Cell> maze, TimeSpan duration)
{
    Console.CursorVisible = false;
    Console.Clear();
    DrawMaze(maze);
    while (true)
    {
        if (duration.TotalMilliseconds > 0)
            await Task.Delay(duration);
        maze.MoveOrigin();
        UpdateMaze(maze);
    }

    static void DrawMaze(Generator<Cell> maze)
    {
        Console.SetCursorPosition(0, 0);
        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
                Console.Write(maze.Cells[x, y].GetDirection());
            Console.WriteLine();
        }
    }

    static void UpdateMaze(Generator<Cell> maze)
    {
        ReadOnlySpan<(int x, int y)> _offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];
        var origin = maze.Origin.Pos;
        foreach (var offset in _offsets)
        {
            var (x, y) = (origin.x + offset.x, origin.y + offset.y);
            if (((uint)x - maze.Width, (uint)y - maze.Height) is (< 0, < 0))
            {
                Console.SetCursorPosition(x, y);
                Console.Write(maze.Cells[x, y].GetDirection());
            }
        }
    }
}

public sealed record class Cell : ICellGeneration
{
    public required (int x, int y) Pos { get; init; }
    public Cell? PointTo { get; private set; }
    ICell? ICell.PointTo => PointTo;
    public IEnumerable<ICell> Neighbours { get; private set; } = [];

    IEnumerable<ICell> ICellGenerationPhase.Neighbours { set => Neighbours = value; }
    ICell? ICellGenerationPhase.PointTo { set => PointTo = (Cell?)value; }

    public char GetDirection()
    {
        if (PointTo is null)
            return '.';
        return (PointTo.Pos.x - Pos.x, PointTo.Pos.y - Pos.y) switch
        {
            (+1, +0) => '>',
            (-1, +0) => '<',
            (+0, -1) => '^',
            (+0, +1) => 'V',
            _ => '$',
        };
    }
}
