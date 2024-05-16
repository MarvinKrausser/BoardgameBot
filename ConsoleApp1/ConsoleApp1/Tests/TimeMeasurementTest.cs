using System.Diagnostics;
using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Tiles;
using NUnit.Framework;

namespace ConsoleApp1.Tests;

[TestFixture]
public class TimeMeasurementTest
{
    [Test]
    public void Test()
    {
        MainClass.difficultyMain = 5;
        MainClass.usePathFinding = true;

        var stringWriter = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        var gameConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\gameconfig.json"))
        {
            gameConfigJson = sr.ReadToEnd();
        }

        var boardConfigJson = string.Empty;

        using (var sr =
               new StreamReader(@"..\\..\\..\\AdditionalExamples\\configs\\boardconfig.json"))
        {
            boardConfigJson = sr.ReadToEnd();
        }

        var gameConfig = JsonManager.DeserializeJson<GameConfig>(gameConfigJson);
        var boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigJson);

        MainClass.boardConfig = boardConfig;
        MainClass._gameConfig = gameConfig;

        MainClass.player = new Player("aa", roleEnum.PLAYER, 0);
        MainClass.player.InitializeCharakter(new StartField(3, 9, directionEnum.NORTH), 3, 0, characterEnum.GOLLUM, 0,
            new List<CheckPoint>());

        var values = Enum.GetValues(typeof(cardEnum));
        var random = new Random();


        var cards = new cardEnum[9];
        for (var i = 0; i < cards.Length; i++) cards[i] = (cardEnum)values.GetValue(random.Next(values.Length));

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        if (MainClass.difficultyMain == 0)
        {
            var clonedArray1 = new cardEnum[5];
            Array.Copy(cards, clonedArray1, 5);
            var cardChoice = new CardChoice(new CardChoice.Data(clonedArray1));
        }
        else
        {
            var clonedArray = new cardEnum[MainClass.difficultyMain + 4];
            Array.Copy(cards, clonedArray, MainClass.difficultyMain + 4);

            var results = MainClass.ComputeAsync(clonedArray);
        }

        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        var ts = stopWatch.Elapsed;

        // Format and display the TimeSpan value.
        var elapsedTime = $"Time: {ts.TotalMilliseconds} Milliseconds";

        Console.SetOut(originalConsoleOut);
        stringWriter.Dispose();
        TestContext.WriteLine(elapsedTime);
    }
}