using ConsoleApp1.DataContainers;
using NUnit.Framework;
using WebSocketSharp;

namespace ConsoleApp1.Tests;

[TestFixture]
public class ComputeMessagesTest
{
    [Test]
    public void Test()
    {
        JsonManager.ChooseDirectory();
        MainClass.webClient = new WebSocket("ws://localhost:3018/");

        var helloClient =
            new HelloClient(new HelloClient.Data(string.Empty, new BoardConfig(), new GameConfig()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(helloClient));

        var participantsInfo = new ParticipantsInfo(new ParticipantsInfo.Data(Array.Empty<string>(),
            Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(participantsInfo));

        var gameState = new GameState(new GameState.Data(Array.Empty<PlayerState>(), new BoardState(), 0));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameState));

        var cardEvent = new CardEvent(new CardEvent.Data(string.Empty, cardEnum.MOVE_1,
            Array.Empty<PlayerState[]>(), Array.Empty<BoardState>()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(cardEvent));

        var shotEvent = new ShotEvent(new ShotEvent.Data(string.Empty, string.Empty, Array.Empty<PlayerState>()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(shotEvent));

        var eagleEvent = new EagleEvent(new EagleEvent.Data(string.Empty, Array.Empty<PlayerState>()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(eagleEvent));

        var riverEvent =
            new RiverEvent(new RiverEvent.Data(string.Empty, Array.Empty<PlayerState[]>(), Array.Empty<BoardState>()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(riverEvent));

        var invalidMessage = new InvalidMessage(new InvalidMessage.Data(string.Empty));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(invalidMessage));

        var gameEnd = new GameEnd(new GameEnd.Data(Array.Empty<PlayerState>(), string.Empty));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(gameEnd));

        var error = new Error(new Error.Data(string.Empty, 1));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(error));

        var paused = new Paused(new Paused.Data(true, string.Empty));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(paused));

        var roundStart = new RoundStart(new RoundStart.Data(Array.Empty<PlayerState>()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(roundStart));

        var cardOffer = new CardOffer(new CardOffer.Data(Array.Empty<cardEnum>()));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(cardOffer));

        var characterOffer =
            new CharacterOffer(new CharacterOffer.Data(new[] { characterEnum.LEGOLAS, characterEnum.GOLLUM }));

        MainClass.ComputeMessage(JsonManager.ConvertToJson(characterOffer));
    }
}