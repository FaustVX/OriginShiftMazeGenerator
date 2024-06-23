// See https://aka.ms/new-console-template for more information
using OriginShiftMazeGenerator.Core;

Console.WriteLine("Hello, World!");

var generator = new Generator<Cell>(GenerateCells(10, 10), new(0));

generator.Setup(0);

DrawLoop(generator);

static Cell[,] GenerateCells(int width, int height)
{
    var maze = new Cell[width, height];
    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maze[x, y] = new Cell() { Pos = (x, y) };
    return maze;
}

static void DrawLoop(Generator<Cell> maze)
{
    Console.CursorVisible = false;
    Console.Clear();
    maze.Regenerate();
    foreach (var cell in maze.Cells)
        cell.IsVisited = false;
    DrawMaze(maze);
    var pos = maze.Cells[0, 0];
    while (true)
    {
        pos.IsVisited = true;
        UpdatePos(maze, pos);
        if (pos == maze[^1, ^1])
            DrawLoop(maze);
        pos = Move(pos, maze);
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

    static void UpdatePos(Generator<Cell> maze, Cell position)
    {
        ReadOnlySpan<(int x, int y)> _offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];
        var origin = position.Pos;
        foreach (var offset in _offsets)
        {
            var (x, y) = (origin.x + offset.x, origin.y + offset.y);
            if (((uint)x - maze.Width, (uint)y - maze.Height) is (< 0, < 0))
                Write(x, y, maze[x, y].GetDirection(), maze[x, y].IsVisited ? ConsoleColor.Magenta : Console.ForegroundColor);
        }
        Write(origin.x, origin.y, maze[origin].GetDirection(), ConsoleColor.Blue);

        static void Write(int x, int y, char symbol, ConsoleColor foreground)
        {
            Console.SetCursorPosition(x, y);
            (var color, Console.ForegroundColor) = (Console.ForegroundColor, foreground);
            Console.Write(symbol);
            Console.ForegroundColor = color;
        }
    }

    static Cell Move(Cell cell, Generator<Cell> maze)
    {
        var move = Console.ReadKey(intercept: true).Key switch
        {
            ConsoleKey.LeftArrow => (-1, 0),
            ConsoleKey.UpArrow => (0, -1),
            ConsoleKey.RightArrow => (1, 0),
            ConsoleKey.DownArrow => (0, 1),
            _ => (0, 0),
        };

        try
        {
            var to = maze[move.Item1 + cell.Pos.x, move.Item2 + cell.Pos.y];
            if (cell.PointTo?.Pos == to.Pos || to.PointTo?.Pos == cell.Pos)
                return to;
            return Move(cell, maze);
        }
        catch (Exception e) when (e is IndexOutOfRangeException or ArgumentOutOfRangeException)
        {
            return Move(cell, maze);
        }
    }
}

public sealed record class Cell : ICellGeneration
{
    public required (int x, int y) Pos { get; init; }
    public Cell? PointTo { get; private set; }
    ICell? ICell.PointTo => PointTo;
    public IEnumerable<ICell> Neighbours { get; private set; } = [];
    public bool IsVisited { get; set; }

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
