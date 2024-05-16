using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Tiles;
using ConsoleApp1.Gameplay.Utility;

namespace ConsoleApp1.Gameplay;

public class Player
{
    private List<cardEnum> aviableCards;

    public bool hasReceivedError = false;
    public List<cardEnum> playedCards;

    public Player(string name, roleEnum role, int reachedCheckpoints)
    {
        PlayerConnected = true;
        Name = name;
        Role = role;
        ReachedCheckpoints = reachedCheckpoints;
        isReady = false;
        playedCards = new List<cardEnum>();
        aviableCards = new List<cardEnum>();
    }

    public bool isReady { get; set; }
    public string Name { get; set; }
    public int ReachedCheckpoints { get; set; }
    public roleEnum Role { get; set; }
    private int suspended { get; set; }

    public bool PlayerConnected { get; set; }
    public bool IsPlayerBanned { get; set; }

    public Charakter Charakter { get; set; }

    //wird von GameManager aufgerufen um charakter zu instanziieren
    public void InitializeCharakter(StartField startField, int lifes, int lembas, characterEnum charakterEnum,
        int deadRoundsLeft, List<CheckPoint> leftCheckPoints)
    {
        Charakter = new Charakter(startField, lifes, lembas, charakterEnum, leftCheckPoints, deadRoundsLeft);
    }


    public Player ClonePlayer()
    {
        var player = new Player(Name, Role, ReachedCheckpoints);
        player.Charakter =
            new Charakter(
                new StartField(Charakter.startField.X, Charakter.startField.Y, Charakter.startField.Direction),
                Charakter.Lifes, Charakter.Lembas, Charakter.Character, new List<CheckPoint>(),
                Charakter.deadRoundsLeft);
        player.Charakter.X = Charakter.X;
        player.Charakter.Y = Charakter.Y;
        player.Charakter.direction = Charakter.direction;
        return player;
    }
}