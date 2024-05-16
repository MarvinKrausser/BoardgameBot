using System.Collections.Concurrent;
using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Tiles;
using NUnit.Framework.Constraints;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace ConsoleApp1;

class MainClass
{
    public static WebSocket webClient;

    private static bool running = true;
    public static int difficultyMain;
    public static bool usePathFinding;

    public static string kiName;
    public static int kiDelay;

    private static int gameStatesCounter = -1;

    public static BoardConfig boardConfig;
    public static GameConfig _gameConfig;
    public static Player player;

    private static string reconnectToken = string.Empty;
    private static ReconnectStateEnum reconnectState = ReconnectStateEnum.NOTPOSSIEBLE;
    private static readonly ConcurrentQueue<(string, int)> jobQueue = new ();
    private static readonly SemaphoreSlim newJob = new(0);
    private static readonly bool useReconnect = true;
    private static bool reconnectWasAccomplished = true;
    private static CardOffer? cardOfferDepot = null;


    /// <summary>
    /// Starts the Ai Client
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        JsonManager.ChooseDirectory();

        string? port = Environment.GetEnvironmentVariable("PORT");
        string? host = Environment.GetEnvironmentVariable("HOST");
        string? name = Environment.GetEnvironmentVariable("NAME");
        string? delay = Environment.GetEnvironmentVariable("DELAY");
        string? difficulty = Environment.GetEnvironmentVariable("DIFFICULTY");
        string? pathFinding = Environment.GetEnvironmentVariable("PATHFINDING");

        if (port is null || host is null || name is null || delay is null || difficulty is null || pathFinding is null)
        {
            throw new Exception("Environment Variables are null");
        }

        /*var name = "ai1";
        var host = "134.60.155.219";
        var port = "3018";
        var delay = "1000";
        var difficulty = "5";
        var pathFinding = "true";*/

        kiName = name;
        kiDelay = int.Parse(delay);

        using (StreamReader sr = new StreamReader(JsonManager.path + "reconnect.txt"))
        {
            reconnectToken = sr.ReadToEnd();
        }

        if(!reconnectToken.IsNullOrEmpty())
        {
            reconnectState = ReconnectStateEnum.POSSIBLE;
        }

        //Difficulty: 0: random, 1: 5 cards, 2: 6 cards, 3: 7 cards, 4: 8 cards, 5: 9 cards

        difficultyMain = int.Parse(difficulty);
        usePathFinding = bool.Parse(pathFinding);

        webClient = new WebSocket("ws://" + host + ":" + port);

        webClient.OnMessage += WebClientOnOnMessage;
        webClient.OnClose += WebClientOnOnClose;
        webClient.OnError += WebClientOnOnError;
        webClient.OnOpen += WebClientOnOnOpen;
        
        connect();

        while (true)
        {
            newJob.Wait();

            if (useReconnect && reconnectWasAccomplished && cardOfferDepot is not null)
            {
                ComputeCardOfffer((CardOffer)cardOfferDepot);
                cardOfferDepot = null;
                continue;
            }
            
            if(!jobQueue.TryDequeue(out var result)) continue;

            switch (result.Item2)
            {
                case 0: //New Message
                    ComputeMessage(result.Item1);
                    break;
                case 1: //Disconnect
                    if (reconnectState == ReconnectStateEnum.TRIED) reconnectState = ReconnectStateEnum.NOTPOSSIEBLE;
                    Thread.Sleep(1000);
                    connect();
                    break;
                case 2: //Connected
                    SendConnectMessages();
                    break;
            }
        }
    }

    private static void connect()
    {
        webClient.Connect();
        Console.WriteLine("Connecting");
    }

    private static void SendConnectMessages()
    {
        if (reconnectState is ReconnectStateEnum.POSSIBLE or ReconnectStateEnum.ACCOMPLISHED && useReconnect)
        {
            reconnectState = ReconnectStateEnum.TRIED;
            
            //Ai now waits for the reconnect data to be send before answering cardoffers
            reconnectWasAccomplished = false;
            Console.WriteLine("Reconnecting");
            var reconnect = new Reconnect(new Reconnect.Data(kiName, reconnectToken));
            webClient.Send(JsonManager.ConvertToJson(reconnect));
            return;
        }

        reconnectWasAccomplished = true;
        var helloServer = new HelloServer(new HelloServer.Data(roleEnum.AI, kiName));
        webClient.Send(JsonManager.ConvertToJson(helloServer));
    }

    public static (float, cardEnum[]) ComputeAsync(cardEnum[] cards)
    {
        var finalResult = (-1f, new cardEnum[5]);
        Semaphore sem = new Semaphore(1, 10);
        
        Parallel.For(0, cards.Length, i =>
        {
            cardEnum[] playedCards = new cardEnum[5];
            playedCards[0] = cards[i];

            //New array without chosen card
            var cardsAfterRound = CloneArrayWithoutIndex(cards, i);
            var gameManager = new GameManager(boardConfig, _gameConfig);
            gameManager.players.Add(player.ClonePlayer());
            
            var result = Compute((-1, new cardEnum[5]), cardsAfterRound, 1, playedCards, player.ClonePlayer(), gameManager);

            sem.WaitOne();
            if (finalResult.Item1 < result.Item1) finalResult = (result.Item1, (cardEnum[])result.Item2.Clone());
            sem.Release();
        });

        if (finalResult.Item1 <= 1)
            finalResult.Item2 = new[] { cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY };
        
        return finalResult;
    }


    /// <summary>
    /// Computes cards by trying all possible card combinations and choosing the best
    /// </summary>
    /// <param name="results"></param>
    /// <param name="cards"></param>
    /// <param name="rounds"></param>
    /// <param name="playedCards"></param>
    /// <param name="playerClone"></param>
    /// <param name="gameManager"></param>
    /// <returns></returns>
    private static (float, cardEnum[]) Compute((float, cardEnum[]) results, cardEnum[] cards, int rounds,
        cardEnum[] playedCards, Player playerClone, GameManager gameManager)
    {
        if (rounds == 5)
        {
            //Memorizing old state
            float qualityOfMove = 0;
            var checkPointsOld = gameManager.players[0].ReachedCheckpoints;
            //Achievements further away are worth less
            var decay = 0.9f;
            var currentDecay = 1f;
            var lembasCountOld = gameManager.players[0].Charakter.Lembas;
            var oldDistanceNextCheckpoint = 0;
            if (usePathFinding)
                oldDistanceNextCheckpoint = gameManager.FindPath(gameManager.players[0].Charakter.GetPosition(),
                    gameManager.gameboard.CheckPoints[playerClone.ReachedCheckpoints]).Count;
            
            foreach (var card in playedCards)
            {
                //gameManager.moveCharakter(gameManager.players[0], new List<cardEnum>(){card});
                gameManager.MoveCharacterFast(gameManager.players[0], card); //Faster
                //gameManager.moveCharaktersOnRiver();
                gameManager.RiverEventFast(gameManager.players[0]); //Faster
                //Triggering eagle fields makes things more random
                if (gameManager.TriggerEagleEvent(gameManager.players[0]))
                    currentDecay *= decay / boardConfig.eagleFields.Length;
                //winning is very valuable
                if (gameManager.CheckCheckpoints())
                {
                    qualityOfMove += 50 * currentDecay;
                    break;
                }

                //Set next checkpoint to next checkpoint
                if (usePathFinding && gameManager.players[0].ReachedCheckpoints != checkPointsOld)
                    oldDistanceNextCheckpoint = gameManager.FindPath(gameManager.players[0].Charakter.GetPosition(),
                        gameManager.gameboard.CheckPoints[gameManager.players[0].ReachedCheckpoints]).Count;;

                //reward achieved checkpoints
                qualityOfMove += (gameManager.players[0].ReachedCheckpoints - checkPointsOld) * 10f * currentDecay;

                //Reward for getting closer to checkpoint
                var nextDistanceNextCheckpoint = 0;
                if (usePathFinding)
                {
                    var position = gameManager.players[0].Charakter.GetPosition();
                    if (gameManager.players[0].Charakter.IsDead)
                        position = gameManager.players[0].Charakter.startField.GetPosition();
                    nextDistanceNextCheckpoint = gameManager.FindPath(position,
                        gameManager.gameboard.CheckPoints[gameManager.players[0].ReachedCheckpoints]).Count;
                    qualityOfMove += (oldDistanceNextCheckpoint - nextDistanceNextCheckpoint) * currentDecay;
                }

                

                oldDistanceNextCheckpoint = nextDistanceNextCheckpoint;
                
                //lembas is good
                qualityOfMove += (gameManager.players[0].Charakter.Lembas - lembasCountOld) * 0.5f * currentDecay;

                //dying is bad
                if (gameManager.players[0].Charakter.IsDead)
                {
                    if (_gameConfig.reviveRounds >= 0) qualityOfMove -= (_gameConfig.reviveRounds + 1) * 5 * currentDecay;
                    else qualityOfMove -= 80 * currentDecay;
                    break;
                }


                checkPointsOld = gameManager.players[0].ReachedCheckpoints;
                lembasCountOld = gameManager.players[0].Charakter.Lembas;
                currentDecay *= decay;
            }

            //Reset player
            gameManager.players[0] = playerClone.ClonePlayer();

            //Choose best Cards
            if (qualityOfMove > results.Item1) return (qualityOfMove, (cardEnum[])playedCards.Clone());
            return results;
        }

        //Go through all possible cards
        for (var i = 0; i < cards.Length; i++)
        {
            playedCards[rounds] = cards[i];

            //New array without chosen card
            var cardsAfterRound = CloneArrayWithoutIndex(cards, i);
            results = Compute(results, cardsAfterRound, rounds + 1, playedCards, playerClone, gameManager);
        }

        return results;
    }

    /// <summary>
    /// Clones an array without a specific index
    /// </summary>
    /// <param name="originalArray"></param>
    /// <param name="indexToRemove"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] CloneArrayWithoutIndex<T>(T[] originalArray, int indexToRemove)
    {
        var clonedArray = new T[originalArray.Length - 1];

        Array.Copy(originalArray, 0, clonedArray, 0, indexToRemove);
        Array.Copy(originalArray, indexToRemove + 1, clonedArray, indexToRemove, clonedArray.Length - indexToRemove);

        return clonedArray;
    }

    
    /// <summary>
    /// Gets called when a connection is established. Will initialize the login
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void WebClientOnOnOpen(object? sender, EventArgs e)
    {
        jobQueue.Enqueue((string.Empty, 2));
        newJob.Release();
    }

    private static void WebClientOnOnError(object? sender, ErrorEventArgs e)
    {
        Console.WriteLine(e.Message);
    }

    private static void WebClientOnOnClose(object? sender, CloseEventArgs e)
    {
        Console.WriteLine("Disconnect");
        jobQueue.Enqueue((string.Empty, 1));
        newJob.Release();
    }

    
    /// <summary>
    /// Gets called when a message is received
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void WebClientOnOnMessage(object? sender, MessageEventArgs e)
    {
        Console.WriteLine(JsonManager.GetTypeJson(e.Data));
        jobQueue.Enqueue((e.Data, 0));
        newJob.Release();
    }

    /// <summary>
    /// Computes incoming messages
    /// </summary>
    /// <param name="message"></param>
    public static void ComputeMessage(string message)
    {
        var type = JsonManager.GetTypeJson(message);
        //Console.WriteLine(type.ToString());
        if (!JsonManager.IsValid(message, type))
        {
            reconnectState = ReconnectStateEnum.NOTPOSSIEBLE;
            webClient.Close();
            return;
        }

        switch (type)
        {
            case messageEnum.HELLO_CLIENT:
                gameStatesCounter = -1;
                var helloClient = JsonManager.DeserializeJson<HelloClient>(message);
                boardConfig = helloClient.data.boardConfig;
                _gameConfig = helloClient.data.gameConfig;

                File.WriteAllText(JsonManager.path + "reconnect.txt", helloClient.data.reconnectToken);
                reconnectToken = helloClient.data.reconnectToken;
                if(reconnectState == ReconnectStateEnum.NOTPOSSIEBLE) reconnectState = ReconnectStateEnum.POSSIBLE;
                if (reconnectState == ReconnectStateEnum.TRIED)
                {
                    reconnectState = ReconnectStateEnum.ACCOMPLISHED;
                    break;
                }

                Thread.Sleep(kiDelay);
                var playerReady = new PlayerReady(new PlayerReady.Data(true));

                webClient.Send(JsonManager.ConvertToJson(playerReady));
                break;
            case messageEnum.CHARACTER_OFFER:
                var characterChoice =
                    new CharacterChoice(new CharacterChoice.Data(JsonManager.DeserializeJson<CharacterOffer>(message)
                        .data.characters[0]));
                webClient.Send(JsonManager.ConvertToJson(characterChoice));
                break;
            case messageEnum.GAME_STATE:
                if (gameStatesCounter == -1)
                {
                    var gameState = JsonManager.DeserializeJson<GameState>(message);
                    Console.WriteLine("First GameState");
                    ComputeGameState(gameState);

                    if (reconnectState == ReconnectStateEnum.ACCOMPLISHED)
                    {
                        reconnectWasAccomplished = true;
                        newJob.Release();
                    }
                }

                if (gameStatesCounter == 4 || reconnectState == ReconnectStateEnum.ACCOMPLISHED)
                {
                    var gameState = JsonManager.DeserializeJson<GameState>(message);
                    Console.WriteLine("Last GameState");
                    ComputeGameState(gameState);
                }
                else
                {
                    gameStatesCounter++;
                }

                break;
            case messageEnum.ROUND_START:
                if (reconnectState == ReconnectStateEnum.ACCOMPLISHED) reconnectState = ReconnectStateEnum.POSSIBLE;
                gameStatesCounter = 0;
                break;
            case messageEnum.CARD_OFFER:
                var cardOffer = JsonManager.DeserializeJson<CardOffer>(message);
                ComputeCardOfffer(cardOffer);
                break;
            case messageEnum.GAME_END:
                reconnectState = ReconnectStateEnum.NOTPOSSIEBLE;
                webClient.Close();
                break;
            case messageEnum.ERROR:
                Error error = JsonManager.DeserializeJson<Error>(message);
                Console.WriteLine(error.data.reason);
                //if (error.data.errorCode is 1 or 2 or 3 or 4 or 5 or 6 or 7) reconnectState = ReconnectStateEnum.NOTPOSSIEBLE;
                break;
        }
    }

    private static void ComputeGameState(GameState gameState)
    {
        var playerState = gameState.data.playerStates.First(p => p.playerName == kiName);
        //Find direction of startField
        var myStartfieldDirection = boardConfig.startFields.First(p =>
                p.position[0] == playerState.spawnPosition[0] &&
                p.position[1] == playerState.spawnPosition[1])
            .direction;
        
        player = new Player(kiName, roleEnum.AI, playerState.reachedCheckpoints);
        player.InitializeCharakter(
            new StartField(playerState.spawnPosition[0], playerState.spawnPosition[1],
                myStartfieldDirection),
            playerState.lives, playerState.lembasCount, characterEnum.GOLLUM,
            playerState.suspended, new List<CheckPoint>());
        
        player.Charakter.X = playerState.currentPosition[0];
        player.Charakter.Y = playerState.currentPosition[1];
        player.Charakter.direction = playerState.direction;
    }


    /// <summary>
    /// Just prints some data
    /// </summary>
    private static void BigDataOutput(Player player)
    {
        Console.WriteLine("______________________________________");
        Console.WriteLine(
            $"Character Position: {player.Charakter.X}, {player.Charakter.Y}, {player.Charakter.direction}");
        Console.WriteLine(
            $"StartField: {player.Charakter.startField.X}, {player.Charakter.startField.Y}, {player.Charakter.startField.Direction}");
        Console.WriteLine(
            $"Character Stats: Checkpoints: {player.ReachedCheckpoints}, Dead: {player.Charakter.IsDead}");
    }

    private static void ComputeCardOfffer(CardOffer cardOffer)
    {
        if (!reconnectWasAccomplished)
        {
            cardOfferDepot = cardOffer;
            return;
        }

        var cards = cardOffer.data.cards;

        //if difficulty is 0, choose random cards
        if (difficultyMain == 0)
        {
            var clonedArray1 = new cardEnum[5];
            Array.Copy(cards, clonedArray1, 5);
            var cardChoice = new CardChoice(new CardChoice.Data(clonedArray1));
            webClient.Send(JsonManager.ConvertToJson(cardChoice));
            return;
        }

        foreach (var c in cards) Console.Write($"{c}  ");
        Console.WriteLine(string.Empty);

        var clonedArray = new cardEnum[(int)MathF.Min(difficultyMain + 4, cards.Length)];
        Array.Copy(cards, clonedArray, clonedArray.Length);

        BigDataOutput(player);

        var stringWriter = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        //Compute next cards
        var results = ComputeAsync(clonedArray);

        Console.SetOut(originalConsoleOut);
        stringWriter.Dispose();

        Console.WriteLine($"Quality of Move: {results.Item1}");

        Console.WriteLine("Playing cards:");
        foreach (var c in results.Item2) Console.Write($"{c}  ");
        Console.WriteLine(string.Empty);

        var cardChoice1 = new CardChoice(new CardChoice.Data(results.Item2));
        webClient.Send(JsonManager.ConvertToJson(cardChoice1));
    }

    
    /// <summary>
    /// Helps to organize the reconnect
    /// </summary>
    private enum ReconnectStateEnum
    {
        NOTPOSSIEBLE,
        POSSIBLE,
        TRIED,
        ACCOMPLISHED
    }
}