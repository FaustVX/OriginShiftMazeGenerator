// See https://aka.ms/new-console-template for more information
using OriginShiftMazeGenerator.Core;

Console.WriteLine("Hello, World!");

var generator = new Generator<Cell>(new Cell[3, 3]
{
    { new() { I = 1 }, new() { I = 2 }, new() { I = 3 } },
    { new() { I = 4 }, new() { I = 5 }, new() { I = 6 } },
    { new() { I = 7 }, new() { I = 8 }, new() { I = 9 } },
});
generator.Setup();
generator.MoveOrigin();
;

public sealed record class Cell : ICellGeneration
{
    public required int I { get; init; }
    public ICell? PointTo { get; private set; }
    public IEnumerable<ICell> Neighbours { get; private set; } = [];

    IEnumerable<ICell> ICellGenerationPhase.Neighbours { set => Neighbours = value; }
    ICell? ICellGenerationPhase.PointTo { set => PointTo = value; }
}
