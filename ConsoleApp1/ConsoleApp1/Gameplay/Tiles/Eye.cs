using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class Eye : Tile
{
    public Eye(int x, int y, directionEnum direction) : base(x, y, true)
    {
        Type = TileTypes.EYE;
        Direction = direction;
    }

    public directionEnum Direction { get; set; }
}