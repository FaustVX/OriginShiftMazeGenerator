namespace OriginShiftMazeGenerator.Core;

public interface ICell
{
    public IEnumerable<ICell> Neighbours { get; }
    public ICell? PointTo { get; }
}

public interface ICellGenerationPhase
{
    public IEnumerable<ICell> Neighbours { set; }
    public ICell PointTo { set; }
}

public interface ICellGeneration : ICell, ICellGenerationPhase
{
    
}
