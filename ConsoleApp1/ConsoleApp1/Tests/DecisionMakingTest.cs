using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Tiles;
using NUnit.Framework;

namespace ConsoleApp1.Tests;

[TestFixture]
public class DecisionMakingTest
{
    [Test]
    [Repeat(10)]
    public void Test()
    {
        MainClass.usePathFinding = true;

        cardEnum[] cards =
        {
            cardEnum.MOVE_3, cardEnum.MOVE_2, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.LEFT_TURN,
            cardEnum.LEFT_TURN, cardEnum.RIGHT_TURN, cardEnum.RIGHT_TURN, cardEnum.MOVE_3
        };

        cardEnum[] expectedResult =
            { cardEnum.MOVE_3, cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.MOVE_3 };
        Do("gameConfig", "boardConfigTest1", cards, expectedResult, new StartField(2, 4, directionEnum.SOUTH));

        cards = new[]
        {
            cardEnum.MOVE_3, cardEnum.MOVE_2, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.LEFT_TURN,
            cardEnum.LEFT_TURN, cardEnum.MOVE_3, cardEnum.RIGHT_TURN, cardEnum.RIGHT_TURN
        };

        expectedResult = new[]
            { cardEnum.MOVE_3, cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.MOVE_3 };
        Do("gameConfig", "boardConfigTest2", cards, expectedResult, new StartField(2, 4, directionEnum.SOUTH));

        cards = new[]
        {
            cardEnum.MOVE_3, cardEnum.LEMBAS, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.LEFT_TURN,
            cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.RIGHT_TURN, cardEnum.RIGHT_TURN
        };

        expectedResult = new[]
            { cardEnum.MOVE_3, cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.MOVE_1 };
        Do("gameConfig", "boardConfigTest3", cards, expectedResult, new StartField(2, 4, directionEnum.SOUTH));

        cards = new[]
        {
            cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.U_TURN, cardEnum.LEFT_TURN, cardEnum.LEFT_TURN,
            cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.RIGHT_TURN, cardEnum.U_TURN
        };

        expectedResult = new[]
            { cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.U_TURN, cardEnum.MOVE_1, cardEnum.MOVE_1 };
        Do("gameConfig", "boardConfigTest4", cards, expectedResult, new StartField(2, 4, directionEnum.WEST));

        cards = new[]
        {
            cardEnum.MOVE_2, cardEnum.MOVE_1, cardEnum.MOVE_2, cardEnum.LEFT_TURN, cardEnum.LEFT_TURN,
            cardEnum.LEFT_TURN, cardEnum.MOVE_3, cardEnum.RIGHT_TURN, cardEnum.LEMBAS
        };

        expectedResult = new[]
            { cardEnum.MOVE_2, cardEnum.LEFT_TURN, cardEnum.MOVE_2, cardEnum.LEFT_TURN, cardEnum.MOVE_3 };
        Do("gameConfig", "boardConfigTest5", cards, expectedResult, new StartField(2, 4, directionEnum.SOUTH));

        cards = new[]
        {
            cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.U_TURN, cardEnum.LEFT_TURN, cardEnum.LEFT_TURN,
            cardEnum.LEFT_TURN, cardEnum.U_TURN, cardEnum.RIGHT_TURN, cardEnum.LEMBAS
        };

        expectedResult = new[]
            { cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.U_TURN, cardEnum.MOVE_1, cardEnum.LEMBAS };
        Do("gameConfig", "boardConfigTest6", cards, expectedResult, new StartField(2, 4, directionEnum.WEST));

        cards = new[]
        {
            cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1,
            cardEnum.RIGHT_TURN, cardEnum.RIGHT_TURN, cardEnum.RIGHT_TURN, cardEnum.MOVE_1
        };

        expectedResult = new[]
            { cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.RIGHT_TURN };
        Do("gameConfig", "boardConfigTest7", cards, expectedResult, new StartField(3, 0, directionEnum.SOUTH));

        cards = new[]
        {
            cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1,
            cardEnum.RIGHT_TURN, cardEnum.LEFT_TURN, cardEnum.RIGHT_TURN, cardEnum.MOVE_1
        };

        expectedResult = new[]
            { cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.MOVE_1 };
        Do("gameConfig", "boardConfigTest8", cards, expectedResult, new StartField(2, 4, directionEnum.SOUTH));

        cards = new[]
        {
            cardEnum.MOVE_3, cardEnum.MOVE_2, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.LEFT_TURN,
            cardEnum.LEFT_TURN, cardEnum.MOVE_3, cardEnum.RIGHT_TURN, cardEnum.RIGHT_TURN
        };

        expectedResult = new[]
            { cardEnum.MOVE_3, cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.MOVE_3 };
        Do("gameConfig", "boardConfigTest9", cards, expectedResult, new StartField(2, 4, directionEnum.SOUTH));

        cards = new[]
        {
            cardEnum.MOVE_3, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.LEFT_TURN, cardEnum.LEMBAS,
            cardEnum.U_TURN, cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.RIGHT_TURN
        };

        expectedResult = new[]
            { cardEnum.MOVE_1, cardEnum.MOVE_1, cardEnum.RIGHT_TURN, cardEnum.MOVE_1, cardEnum.MOVE_1 };
        Do("gameConfig", "boardConfigTest9", cards, expectedResult, new StartField(3, 4, directionEnum.NORTH));
    }

    public void Do(string gamePath, string boardPath, cardEnum[] cards, cardEnum[] expectedResult,
        StartField startField)
    {
        var stringWriter = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        var gameConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\" + gamePath + ".json"))
        {
            gameConfigJson = sr.ReadToEnd();
        }

        var boardConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\" + boardPath + ".json"))
        {
            boardConfigJson = sr.ReadToEnd();
        }

        var gameConfig = JsonManager.DeserializeJson<GameConfig>(gameConfigJson);
        var boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigJson);

        MainClass.boardConfig = boardConfig;
        MainClass._gameConfig = gameConfig;

        MainClass.player = new Player("aa", roleEnum.PLAYER, 0);
        MainClass.player.InitializeCharakter(startField, 3, 0, characterEnum.GOLLUM, 0,
            new List<CheckPoint>());

        var results = MainClass.ComputeAsync(cards);
        
        Console.SetOut(originalConsoleOut);
        stringWriter.Dispose();

        Console.WriteLine(boardPath + ":");

        foreach (var card in results.Item2) TestContext.Write(card + "  ");
        TestContext.WriteLine(string.Empty);
        TestContext.WriteLine(results.Item1);

        for (var i = 0; i < expectedResult.Length; i++)
            if (expectedResult[i] != results.Item2[i])
                Assert.Fail("Wrong cards");
    }

    [Test]
    public void CloneArray()
    {
        cardEnum[] cards =
            { cardEnum.MOVE_1, cardEnum.MOVE_2, cardEnum.MOVE_3, cardEnum.MOVE_BACK, cardEnum.LEFT_TURN };
        var newArray = MainClass.CloneArrayWithoutIndex(cards, 2);

        var expected = new[] { cardEnum.MOVE_1, cardEnum.MOVE_2, cardEnum.MOVE_BACK, cardEnum.LEFT_TURN };

        for (var i = 0; i < expected.Length; i++)
            if (expected[i] != newArray[i])
                Assert.Fail();
    }

    [Test]
    public void WalkTest()
    {
        var gameConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\gameconfig.json"))
        {
            gameConfigJson = sr.ReadToEnd();
        }

        var boardConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\boardconfigTest1.json"))
        {
            boardConfigJson = sr.ReadToEnd();
        }

        var gameConfig = JsonManager.DeserializeJson<GameConfig>(gameConfigJson);
        var boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigJson);

        var gameManager = new GameManager(boardConfig, gameConfig);

        var player = new Player("aa", roleEnum.PLAYER, 0);
        player.InitializeCharakter(new StartField(1, 4, directionEnum.WEST), 3, 0, characterEnum.GOLLUM, 0,
            gameManager.gameboard.CheckPoints);

        gameManager.players.Add(player);

        gameManager.Walk(gameManager.players[0], directionEnum.WEST);
        gameManager.Walk(gameManager.players[0], directionEnum.EAST);
        gameManager.Walk(gameManager.players[0], directionEnum.SOUTH);
        gameManager.Walk(gameManager.players[0], directionEnum.NORTH);


        if (gameManager.players[0].Charakter.X != 1 && gameManager.players[0].Charakter.Y != 4)
            Assert.Fail("Wrong Coordinates");
        if (gameManager.players[0].Charakter.IsDead) Assert.Fail("Character is dead");
    }
}