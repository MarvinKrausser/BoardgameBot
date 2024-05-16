using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class LembasFieldTile : Tile
{
    public LembasFieldTile(int x, int y, int amount) : base(x, y, false)
    {
        Type = TileTypes.LEMBAS_FIELD;
        this.amount = amount;
    }

    public int amount { get; set; }
}