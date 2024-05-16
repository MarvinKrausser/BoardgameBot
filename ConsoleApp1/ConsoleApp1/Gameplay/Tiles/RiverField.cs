using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class RiverField : Tile
{
    public RiverField(int x, int y, directionEnum direction) : base(x, y, false)
    {
        Type = TileTypes.RIVER_FIELD;
        Direction = direction;
    }

    public directionEnum Direction { get; set; }
}