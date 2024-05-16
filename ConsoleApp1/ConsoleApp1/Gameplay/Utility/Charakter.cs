using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Tiles;

namespace ConsoleApp1.Gameplay.Utility;

public class Charakter
{
    public characterEnum Character;

    public cardEnum lastPlayedCard;

    //for finding turn order
    public List<Tile> pathToEye;

    public List<cardEnum> playedCards = new();

    public Charakter(StartField startField, int Lifes, int Lembas, characterEnum characterEnum,
        List<CheckPoint> checkPoints, int deadRoundsesLeft)
    {
        //position of charakter
        X = startField.X;
        Y = startField.Y;
        direction = startField.Direction;

        //safe startfield of charakter
        this.startField = startField;

        cards = new Cards();

        //andere eigenschaften
        this.Lifes = Lifes;
        LifesFinal = Lifes;
        this.Lembas = Lembas;
        LembasFinal = Lembas;
        Character = characterEnum;
        IsDead = false;
        deadRoundsLeftMax = deadRoundsesLeft;

        lastPlayedCard = cardEnum.EMPTY;
    }

    public int Lifes { get; set; }
    private int LifesFinal { get; }
    public int Lembas { get; set; }
    private int LembasFinal { get; }
    public int X { get; set; }
    public int Y { get; set; }

    public StartField startField { get; set; }

    public Cards cards { get; set; }

    //riverEvent 
    public RiverField? lastRiverField { get; set; }
    public int turnOrder { get; set; }
    public directionEnum direction { get; set; }
    public bool IsDead { get; set; }
    public int deadRoundsLeft { get; set; }
    public int deadRoundsLeftMax { get; }

    //wird aufgerufen wenn spieler getroffen wird
    /*public void isHitEvent()
    {
        Lifes -= 1;
        if (Lifes <= 0)
        {
            killCharakter();
        }
    }*/

    //wird getriggert sobald spieler tot ist und die neue runde beginnt
    /*public void respawn(StartField respawnPoint)
    {
        IsDead = false;
        this.X = respawnPoint.X;
        this.Y = respawnPoint.Y;
        this.direction = respawnPoint.Direction;
        this.Lifes = this.LifesFinal;
        this.Lembas = this.LembasFinal;
    }*/

    //wird getriggert wenn spieler ins loch oder vom Spielfeld fällt
    public void killCharakter()
    {
        IsDead = true;
        deadRoundsLeft = deadRoundsLeftMax;
        Lifes = 0;
        cards.playerDied();
    }

    public (int, int) GetPosition()
    {
        return (X, Y);
    }
}