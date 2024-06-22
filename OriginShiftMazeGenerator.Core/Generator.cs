namespace OriginShiftMazeGenerator.Core;

using System.Runtime.CompilerServices;
using PrimaryParameter.SG;

// Generator made by @CaptainLuma (https://www.youtube.com/watch?v=zbXKcDVV4G0)

public partial class Generator<TCell>
    (
        [Field(Type = typeof(ICellGeneration[,]), AssignFormat = "System.Runtime.CompilerServices.Unsafe.As<ICellGeneration[,]>({0})")]
        [Property(AssignFormat = "{0}.GetLength(0)", Name = nameof(Generator<TCell>.Width), Type = typeof(int))]
        [Property(AssignFormat = "{0}.GetLength(1)", Name = nameof(Generator<TCell>.Height), Type = typeof(int))]
        TCell[,] cells
    )
where TCell : ICell
{
    private static readonly (int x, int y)[] _offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    public TCell[,] Cells => Unsafe.As<TCell[,]>(_cells);

    public TCell Origin { get; private set; } = default!;

    public void Setup()
    {
        GenerateRows(_cells);
        GenerateFirstColumn(_cells);
        GenerateNeighbours(_cells);
        Origin = (TCell)_cells[0, 0];

        void GenerateRows(ICellGenerationPhase[,] cells)
        {
            for (var y = 0; y < Height; y++)
                for (var x = 1; x < Width; x++)
                    cells[x, y].PointTo = (TCell)cells[x - 1, y];
        }

        void GenerateFirstColumn(ICellGenerationPhase[,] cells)
        {
            for (var y = 1; y < Height; y++)
                cells[0, y].PointTo = (TCell)cells[0, y - 1];
        }

        void GenerateNeighbours(ICellGenerationPhase[,] cells)
        {
            var positions = cells.WithIndices()
                .Select(static pos => (pos.item, _offsets.Select(offset => (pos.x + offset.x, pos.y + offset.y))));
            foreach (var (cell, neighboursPos) in positions)
                cell.Neighbours = neighboursPos
                    .Where(pos => ((uint)pos.Item1 - Width, (uint)pos.Item2 - Height) is (< 0, < 0))
                    .GetIn((ICell[,])_cells);
        }
    }

    public void MoveOrigin()
    {
        var next = Random.Shared.Pickitem(Origin.Neighbours);
        ((ICellGenerationPhase)Origin).PointTo = next;
        Origin = (TCell)next;
        ((ICellGenerationPhase)next).PointTo = null;
    }
}

file static class Ext
{
    public static IEnumerable<(T item, int x, int y)> WithIndices<T>(this T[,] array)
    {
        var (width, height) = (array.GetLength(0), array.GetLength(1));
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                yield return (array[x, y], x, y);
    }

    public static IEnumerable<T> GetIn<T>(this IEnumerable<(int x, int y)> pos, T[,] array)
    {
        foreach (var (x, y) in pos)
            yield return array[x, y];
    }

    public static T Pickitem<T>(this Random rng, IEnumerable<T> values)
    {
        var count = values.TryGetNonEnumeratedCount(out var c) ? c : values.Count();
        return values.ElementAt(rng.Next(count));
    }
}
