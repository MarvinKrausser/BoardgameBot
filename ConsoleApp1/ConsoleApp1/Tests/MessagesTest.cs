using ConsoleApp1.DataContainers;
using NUnit.Framework;
using WebSocketSharp;

namespace ConsoleApp1.Tests;

[TestFixture]
public class MessagesTest
{
    [Test]
    public void Test()
    {
        MainClass.kiName = "ai1";
        MainClass.webClient = new WebSocket("ws://localhost:3018");
        JsonManager.ChooseDirectory();
        MainClass.kiDelay = 1;

        var gameConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\gameConfig.json"))
        {
            gameConfigJson = sr.ReadToEnd();
        }

        var boardConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\boardConfigTest1.json"))
        {
            boardConfigJson = sr.ReadToEnd();
        }

        var gameConfig = JsonManager.DeserializeJson<GameConfig>(gameConfigJson);
        var boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigJson);

        var helloClient = new HelloClient(new HelloClient.Data("www", boardConfig, gameConfig));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(helloClient));

        var playerState1 = new PlayerState("ai1", new[] { 2, 4 }, new[] { 2, 4 }, directionEnum.WEST,
            characterEnum.GOLLUM, 3, 3, 0, 1, new[] { cardEnum.MOVE_1 }, 1);

        var playerState2 = new PlayerState("ai4", new[] { 9, 0 }, new[] { 9, 0 }, directionEnum.EAST,
            characterEnum.GOLLUM, 2, 2, 0, 0, new[] { cardEnum.MOVE_1 }, 1);

        var gameState =
            new GameState(new GameState.Data(new[] { playerState2, playerState1 },
                new BoardState(Array.Empty<LembasField>()), 4));
        //-1
        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameState));


        playerState1 = new PlayerState("ai1", new[] { 5, 6 }, new[] { 2, 4 }, directionEnum.WEST,
            characterEnum.GOLLUM, 3, 3, 0, 1, new[] { cardEnum.MOVE_1 }, 1);

        playerState2 = new PlayerState("ai4", new[] { 2, 8 }, new[] { 9, 0 }, directionEnum.EAST,
            characterEnum.GOLLUM, 2, 2, 0, 0, new[] { cardEnum.MOVE_1 }, 1);

        gameState =
            new GameState(new GameState.Data(new[] { playerState2, playerState1 },
                new BoardState(Array.Empty<LembasField>()), 4));
        //0
        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameState));

        playerState1 = new PlayerState("ai1", new[] { 5, 6 }, new[] { 2, 4 }, directionEnum.WEST,
            characterEnum.GOLLUM, 3, 3, 0, 1, new[] { cardEnum.MOVE_1 }, 1);

        playerState2 = new PlayerState("ai4", new[] { 2, 8 }, new[] { 9, 0 }, directionEnum.EAST,
            characterEnum.GOLLUM, 2, 2, 0, 0, new[] { cardEnum.MOVE_1 }, 1);

        gameState =
            new GameState(new GameState.Data(new[] { playerState2, playerState1 },
                new BoardState(Array.Empty<LembasField>()), 4));
        //1
        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameState));

        playerState1 = new PlayerState("ai1", new[] { 5, 6 }, new[] { 2, 4 }, directionEnum.WEST,
            characterEnum.GOLLUM, 3, 3, 0, 1, new[] { cardEnum.MOVE_1 }, 1);

        playerState2 = new PlayerState("ai4", new[] { 2, 8 }, new[] { 9, 0 }, directionEnum.EAST,
            characterEnum.GOLLUM, 2, 2, 0, 0, new[] { cardEnum.MOVE_1 }, 1);

        gameState =
            new GameState(new GameState.Data(new[] { playerState2, playerState1 },
                new BoardState(Array.Empty<LembasField>()), 4));
        //2
        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameState));

        playerState1 = new PlayerState("ai1", new[] { 5, 6 }, new[] { 2, 4 }, directionEnum.WEST,
            characterEnum.GOLLUM, 3, 3, 0, 1, new[] { cardEnum.MOVE_1 }, 1);

        playerState2 = new PlayerState("ai4", new[] { 2, 8 }, new[] { 9, 0 }, directionEnum.EAST,
            characterEnum.GOLLUM, 2, 2, 0, 0, new[] { cardEnum.MOVE_1 }, 1);

        gameState =
            new GameState(new GameState.Data(new[] { playerState2, playerState1 },
                new BoardState(Array.Empty<LembasField>()), 4));
        //3
        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameState));

        playerState1 = new PlayerState("ai1", new[] { 3, 6 }, new[] { 2, 4 }, directionEnum.WEST,
            characterEnum.GOLLUM, 3, 3, 0, 1, new[] { cardEnum.MOVE_1 }, 1);

        playerState2 = new PlayerState("ai4", new[] { 2, 8 }, new[] { 9, 0 }, directionEnum.EAST,
            characterEnum.GOLLUM, 2, 2, 0, 0, new[] { cardEnum.MOVE_1 }, 1);

        gameState =
            new GameState(new GameState.Data(new[] { playerState2, playerState1 },
                new BoardState(Array.Empty<LembasField>()), 4));
        //4
        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameState));


        var player = MainClass.player;
        if (player.Charakter.X != 3 || player.Charakter.Y != 6) Assert.Fail("Wrong Coordinates");
        if (player.ReachedCheckpoints != 1) Assert.Fail("Wrong Checkpoint count");
        if (player.Charakter.direction != directionEnum.WEST) Assert.Fail("Wrong direction");
    }
}