// See https://aka.ms/new-console-template for more information
using OriginShiftMazeGenerator.Core;

Console.WriteLine("Hello, World!");

var generator = new Generator<Cell>(GenerateCells(5, 5));

generator.Setup();

await DrawLoop(generator, TimeSpan.FromMilliseconds(500));

static Cell[,] GenerateCells(int width, int height)
{
    var maze = new Cell[width, height];
    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maze[x, y] = new Cell() { I = (x, y) };
    return maze;
}

static async Task DrawLoop(Generator<Cell> maze, TimeSpan duration)
{
    Console.Clear();
    DrawMaze(maze);
    while (true)
    {
        await Task.Delay(duration);
        maze.MoveOrigin();
        DrawMaze(maze);
    }

    static void DrawMaze(Generator<Cell> maze)
    {
        Console.SetCursorPosition(0, 0);
        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
                Console.Write(maze.Cells[x, y].GetDirection(maze));
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}

public sealed record class Cell : ICellGeneration
{
    public required (int x, int y) I { get; init; }
    public Cell? PointTo { get; private set; }
    ICell? ICell.PointTo => PointTo;
    public IEnumerable<ICell> Neighbours { get; private set; } = [];

    IEnumerable<ICell> ICellGenerationPhase.Neighbours { set => Neighbours = value; }
    ICell? ICellGenerationPhase.PointTo { set => PointTo = (Cell?)value; }

    public char GetDirection(Generator<Cell> maze)
    {
        if (PointTo is null)
            return '.';
        return (PointTo.I.x - I.x, PointTo.I.y - I.y) switch
        {
            (+1, +0) => '>',
            (-1, +0) => '<',
            (+0, -1) => '^',
            (+0, +1) => 'V',
            _ => '$',
        };
    }
}
