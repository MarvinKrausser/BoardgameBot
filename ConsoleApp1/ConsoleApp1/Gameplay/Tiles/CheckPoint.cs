using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

/// <summary>
///     Repräsentiert ein Checkpoint
/// </summary>
public class CheckPoint : Tile
{
    public CheckPoint(int x, int y, int order) : base(x, y, false)
    {
        Type = TileTypes.CHECKPOINT;
        this.order = order;
    }

    public int order { get; set; }
}