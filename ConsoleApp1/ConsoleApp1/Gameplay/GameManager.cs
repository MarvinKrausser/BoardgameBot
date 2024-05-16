using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Tiles;

namespace ConsoleApp1.Gameplay;

public class GameManager
{
    private bool _isRunning = true;
    private bool _receivingCardChoices;
    private readonly BoardConfig boardConfig;
    public Gameboard gameboard;
    private readonly GameConfig gameConfig;


    //methode um moves auf charakter anzuwenden
    //parameter müssen evtl noch angepasst werden (Karte statt List von Moves?)
    //es fehlen noch ein paar moves wie U-Turn usw.

    public GameManager(BoardConfig boardconfig, GameConfig gameconfig)
    {
        gameboard = new Gameboard(boardconfig);
        players = new List<Player>();
        viewers = new List<Player>();

        boardConfig = boardconfig;
        gameConfig = gameconfig;
    }
    //hier wird das ganze spiel verwaltet
    //Spieler und Charaktere initialisiert 
    //Gameboard initialisiert und ausgewertet
    //Nachrichten empfangen und triggert dazugehörige Events

    public List<Player> players { set; get; }
    public List<Player> viewers { get; set; }

    public Eye eye => gameboard.eye;

    public void MoveCharacterFast(Player player, cardEnum card)
    {
        if (card == cardEnum.AGAIN) card = player.Charakter.lastPlayedCard;

        var steps = 0;

        switch (card)
        {
            case cardEnum.MOVE_1:
                steps = 1;
                break;
            case cardEnum.MOVE_2:
                steps = 2;
                break;
            case cardEnum.MOVE_3:
                steps = 3;
                break;
            case cardEnum.MOVE_BACK:
                Walk(player, InvertDirection(player.Charakter.direction));
                return;
            case cardEnum.LEMBAS:
                player.Charakter.Lembas++;
                return;
            case cardEnum.U_TURN:
                player.Charakter.direction = InvertDirection(player.Charakter.direction);
                return;
            case cardEnum.RIGHT_TURN:
                player.Charakter.direction = RotateDirectionRight(player.Charakter.direction);
                return;
            case cardEnum.LEFT_TURN:
                player.Charakter.direction = RotateDirectionLeft(player.Charakter.direction);
                return;
        }

        while (steps > 0)
        {
            if (Walk(player, player.Charakter.direction)) return;
            steps -= 1;
        }
    }

    public bool Walk(Player player, directionEnum direction)
    {
        if (!gameboard.isWalkable(player.Charakter.X, player.Charakter.Y, direction)) return true;


        var newCoordinates = GiveCoordinateInDirection((player.Charakter.X, player.Charakter.Y), direction);

        if (newCoordinates.Item1 < 0 || newCoordinates.Item1 >= boardConfig.width || newCoordinates.Item2 < 0 ||
            newCoordinates.Item2 >= boardConfig.height)
        {
            player.Charakter.killCharakter();
            return true;
        }

        player.Charakter.X = newCoordinates.Item1;
        player.Charakter.Y = newCoordinates.Item2;
        return checkHoleandLembasTile(player);
    }

    private (int, int) GiveCoordinateInDirection((int, int) coordinates, directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.NORTH:
                coordinates.Item2 += -1;
                break;
            case directionEnum.SOUTH:
                coordinates.Item2 += 1;
                break;
            case directionEnum.WEST:
                coordinates.Item1 += -1;
                break;
            case directionEnum.EAST:
                coordinates.Item1 += 1;
                break;
        }

        return coordinates;
    }

    /// <summary>
    /// Alle standard bewegungen der Karten werden über diese Methode ausgefürt. Es wird ein Spieler, sowie dessen Bewegungen die
    /// ausgeführt werden sollen übergeben.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="moves"></param>
    /*public void moveCharakter(Player player, List<cardEnum> moves)
    {
        cardEnum currentCard;

        foreach (var move in moves)
        {
            if (move == cardEnum.AGAIN)
            {
                currentCard = player.Charakter.lastPlayedCard;
            }
            else currentCard = move;

            Console.WriteLine("Stats of {0}: {1} {2} {3}", player.Name, player.Charakter.X, player.Charakter.Y,
                player.Charakter.direction);
            switch (currentCard)
            {
                //vor und rückwärtsbewegung des charakters. Es wird geschaut ob der move ausführbar ist sonst wird gewartet
                case cardEnum.MOVE_1:
                    movementHelper(player, player.Charakter.direction);
                    break;

                case cardEnum.MOVE_2:
                    movementHelper(player, player.Charakter.direction);
                    movementHelper(player, player.Charakter.direction);
                    break;

                case cardEnum.MOVE_3:
                    movementHelper(player, player.Charakter.direction);
                    movementHelper(player, player.Charakter.direction);
                    movementHelper(player, player.Charakter.direction);
                    break;

                case cardEnum.MOVE_BACK:
                    directionEnum dir = directionEnum.EAST;

                    switch (player.Charakter.direction)
                    {
                        case directionEnum.NORTH:
                            dir = directionEnum.SOUTH;
                            break;
                        case directionEnum.SOUTH:
                            dir = directionEnum.NORTH;
                            break;
                        case directionEnum.WEST:
                            dir = directionEnum.EAST;
                            break;
                        case directionEnum.EAST:
                            dir = directionEnum.WEST;
                            break;
                    }

                    movementHelper(player, dir);
                    break;
                //bei einer drehung wird eine Helfermethode aufgerufen die aufgrund der aktuellen position den spieler dreht
                case cardEnum.LEFT_TURN:
                case cardEnum.RIGHT_TURN:
                    player.Charakter.direction = turnCharakterHelper(currentCard, player);
                    break;

                case cardEnum.U_TURN:
                    player.Charakter.direction =
                        turnCharakterHelper(cardEnum.LEFT_TURN,
                            player); //zweimal in eine richtung drehen ist ein U-Turn
                    player.Charakter.direction = turnCharakterHelper(cardEnum.LEFT_TURN, player);
                    break;

                case cardEnum.LEMBAS:
                    player.Charakter.Lembas += 1;
                    break;
            }
            
            player.Charakter.lastPlayedCard = currentCard;
        }
    }*/

    /// <summary>
    ///     Lässt alle Spieler schießen.
    ///     Überprüft auf Richtung und Hindernisse. Jeder Spieler kann nur einmal schießen und es wird nur ein Ziel getroffen
    /// </summary>
    /*public void shoot()
    {
        List<Player> shootOrder = new List<Player>();
        //jeder Spieler wird mit jedem verglichen, Abhänig von den Koordinaten und der Richtung schauen ob Ziel getroffen werden könnte
        //wenn ja, überprüfung auf Hindernisse dann wird Ziel getroffen
        foreach (var player in players)
        {
            Console.WriteLine("Player {0} looks for target", player.Name);
            //wenn spieler nicht genügen Lembas wird abgebrochen => Optimierung
            if (player.Charakter.Lembas < gameConfig.shotLembas)
            {
                Console.WriteLine("player has not enought lembas");
                continue;
            }

            if (player.Charakter.IsDead) continue;
            

            switch (player.Charakter.direction)
            {
                case directionEnum.NORTH:
                    shootOrder = players.OrderByDescending(p => p.Charakter.Y).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.X == otherPlayer.Charakter.X &&
                            player.Charakter.Y > otherPlayer.Charakter.Y) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.Y; i > otherPlayer.Charakter.Y; i--)
                            {
                                Console.WriteLine("Y coordinate getting checked is: " + i);
                                if (!gameboard.isWalkable(player.Charakter.X, i, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 1 \n");
                                
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;
                                
                                goto
                                    nextPlayerLabel; //verhindert dass ein spieler mehrere Ziele treffen kann. Not best practice but this will be fine :)
                            }
                            //todo: kurzer test ob das so klappt; GameConfig muss och intitialisiert werden
                        }
                    }

                    break;
                case directionEnum.SOUTH:
                    shootOrder = players.OrderBy(p => p.Charakter.Y).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.X == otherPlayer.Charakter.X &&
                            player.Charakter.Y < otherPlayer.Charakter.Y) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.Y; i < otherPlayer.Charakter.Y; i++)
                            {
                                if (!gameboard.isWalkable(player.Charakter.X, i, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 2 \n");
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;
                                
                                goto
                                    nextPlayerLabel; //verhindert dass ein spieler mehrere Ziele treffen kann. Not best practice but this will be fine :)
                            }
                        }
                    }

                    break;
                case directionEnum.WEST:
                    shootOrder = players.OrderByDescending(p => p.Charakter.X).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.Y == otherPlayer.Charakter.Y &&
                            player.Charakter.X > otherPlayer.Charakter.X) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.X; i > otherPlayer.Charakter.X; i--)
                            {
                                if (!gameboard.isWalkable(i, player.Charakter.Y, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 3 \n");
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;
                                
                                goto
                                    nextPlayerLabel; //verhindert dass ein spieler mehrere Ziele treffen kann. Not best practice but this will be fine :)
                            }
                        }
                    }

                    break;
                case directionEnum.EAST:
                    shootOrder = players.OrderBy(p => p.Charakter.X).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.Y == otherPlayer.Charakter.Y &&
                            player.Charakter.X < otherPlayer.Charakter.X) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.X; i < otherPlayer.Charakter.X; i++)
                            {
                                if (!gameboard.isWalkable(i, player.Charakter.Y, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 4 \n");
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;
                                
                                goto nextPlayerLabel;
                            }
                        }
                    }

                    break;
            }

            nextPlayerLabel: ;
        }
    }*/
    public void RiverEventFast(Player player)
    {
        var posX = player.Charakter.X;
        var posY = player.Charakter.Y;
        for (var i = 0; i < gameConfig.riverMoveCount; i++)
        {
            //hier beginnt der spaß
            var currTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];

            if (currTile is RiverField currRiverField)
            {
                //check next Field
                var nextTile = getNextRiverTile(currRiverField);

                if (nextTile == null)
                {
                    Console.WriteLine("{0} fell from board", player.Name);
                    player.Charakter.killCharakter();
                    break;
                }

                if (Walk(player, currRiverField.Direction)) return;

                if (nextTile is RiverField nextRiverField) RotatePlayerRiver(currRiverField, nextRiverField, player);

                player.Charakter.lastRiverField = currRiverField;
            }
        }

        player.Charakter.lastRiverField = null;
    }

    /// <summary>
    ///     schaut für alle Spieler ob sie auf einem Fluss Feld stehen, wenn ja werden alle 2 tiles weiter verschobe
    ///     SONDERFALL: Ende des flusses wird auf die Zugreihenfolge geachtet
    /// </summary>
    /*public void moveCharaktersOnRiver()
    {
        foreach (var player in players)
        {
            moveCharakterOnRiverHelper(player, gameConfig.riverMoveCount);
        }
    }*/
    /*public void moveCharakterOnRiverHelper(Player player, int riverMoveCount)
    {
        Console.WriteLine("{0} is being moved", player.Name);

        var posX = player.Charakter.X;
        var posY = player.Charakter.Y;
        for (int i = 0; i < riverMoveCount; i++)
        {

            //hier beginnt der spaß
            var currTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];

            if (currTile is RiverField currRiverField)
            {
                //check next Field
                var nextTile = getNextRiverTile(currRiverField);

                if (nextTile == null)
                {
                    Console.WriteLine("{0} fell from board", player.Name);
                    player.Charakter.killCharakter();
                    break;
                }
                //sollte kein anderer Spieler mehr vor einem sein so wird bewegt
                movementHelper(player, currRiverField.Direction);
                    
                if(nextTile is RiverField nextRiverField)
                {
                    RotatePlayerRiver(currRiverField, nextRiverField, player);
                }
                    
                player.Charakter.lastRiverField = currRiverField;
            }
        }
        player.Charakter.lastRiverField = null;
    }*/
    private void RotatePlayerRiver(RiverField currrentTile, RiverField nextTile, Player player)
    {
        if (InvertDirection(currrentTile.Direction).Equals(nextTile.Direction))
            player.Charakter.direction = InvertDirection(player.Charakter.direction);
        if (RotateDirectionLeft(currrentTile.Direction).Equals(nextTile.Direction))
            player.Charakter.direction = RotateDirectionLeft(player.Charakter.direction);
        if (RotateDirectionRight(currrentTile.Direction).Equals(nextTile.Direction))
            player.Charakter.direction = RotateDirectionRight(player.Charakter.direction);
    }

    private directionEnum InvertDirection(directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.EAST:
                return directionEnum.WEST;
            case directionEnum.WEST:
                return directionEnum.EAST;
            case directionEnum.SOUTH:
                return directionEnum.NORTH;
            case directionEnum.NORTH:
                return directionEnum.SOUTH;
        }

        return directionEnum.SOUTH;
    }

    private directionEnum RotateDirectionRight(directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.NORTH:
                return directionEnum.EAST;
            case directionEnum.SOUTH:
                return directionEnum.WEST;
            case directionEnum.EAST:
                return directionEnum.SOUTH;
            case directionEnum.WEST:
                return directionEnum.NORTH;
        }

        return directionEnum.SOUTH;
    }

    private directionEnum RotateDirectionLeft(directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.NORTH:
                return directionEnum.WEST;
            case directionEnum.SOUTH:
                return directionEnum.EAST;
            case directionEnum.EAST:
                return directionEnum.NORTH;
            case directionEnum.WEST:
                return directionEnum.SOUTH;
        }

        return directionEnum.SOUTH;
    }


    private Tile getNextRiverTile(RiverField currentTile)
    {
        var x = currentTile.X;
        var y = currentTile.Y;

        switch (currentTile.Direction)
        {
            case directionEnum.NORTH:
                //wenn auserhalb des spielfeldes wird null returned
                if (y - 1 >= 0) return gameboard.tiles[x, y - 1];

                break;
            case directionEnum.SOUTH:
                if (y + 1 < gameboard.Height) return gameboard.tiles[x, y + 1];

                break;
            case directionEnum.WEST:
                if (x - 1 >= 0) return gameboard.tiles[x - 1, y];

                break;
            case directionEnum.EAST:
                if (x + 1 < gameboard.Width) return gameboard.tiles[x + 1, y];

                break;
        }

        return null;
    }

    //schaut sich die aktuelle orientation des Charakters and und ändert die Richtung entsprechend 
    /*private directionEnum turnCharakterHelper(cardEnum move, Player player)
    {
        directionEnum currentDirection = player.Charakter.direction;

        switch (currentDirection)
        {
            case directionEnum.NORTH:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.WEST;
                }

                return directionEnum.EAST;

            case directionEnum.EAST:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.NORTH;
                }

                return directionEnum.SOUTH;

            case directionEnum.SOUTH:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.EAST;
                }

                return directionEnum.WEST;

            case directionEnum.WEST:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.SOUTH;
                }

                return directionEnum.NORTH;
        }

        throw new Exception("Internal Error in Player direction movement");
    }*/


    //findet alle Nachbarn eines Charakteres von einem Spieler, abhänig davon in welche richtung er schaut

    /*private void movementHelper(Player Initialplayer, directionEnum direction)
    {
        Console.WriteLine("Moving " + Initialplayer.Name);
        List<Player> pushedNeigbours = findPushingNeighbours(Initialplayer, direction);
        pushedNeigbours.Add(Initialplayer);

        foreach (var player in pushedNeigbours)
        {
            if (player.Charakter.IsDead)
            {
                break;
            }

            if (gameboard.isWalkable(player.Charakter.X, player.Charakter.Y, direction))
            {
                switch (direction)
                {
                    case directionEnum.NORTH:
                        if (player.Charakter.Y - 1 < 0)
                        {
                            Console.WriteLine("{0} fell from the board", player.Name);
                            player.Charakter.killCharakter();
                            break;
                        }

                        player.Charakter.Y += -1;
                        break;
                    case directionEnum.SOUTH:
                        if (player.Charakter.Y + 1 >= gameboard.Height)
                        {
                            Console.WriteLine("{0} fell from the board", player.Name);
                            player.Charakter.killCharakter();
                            break;
                        }

                        player.Charakter.Y += 1;
                        break;
                    case directionEnum.WEST:
                        if (player.Charakter.X - 1 < 0)
                        {
                            Console.WriteLine("{0} fell from the board", player.Name);
                            player.Charakter.killCharakter();
                            break;
                        }

                        player.Charakter.X += -1;
                        break;
                    case directionEnum.EAST:
                        if (player.Charakter.X + 1 >= gameboard.Width)
                        {
                            Console.WriteLine("{0} fell from the board", player.Name);
                            player.Charakter.killCharakter();
                            break;
                        }

                        player.Charakter.X += 1;
                        break;
                }
            }
            else
            {
                Console.WriteLine("Tile to {0} from {1} {2} not walkable", direction, player.Charakter.X,
                    player.Charakter.Y);
                break;
            }

            checkHoleandLembasTile(player);
        }


        //wenn lemabs muss von Objekt abgezogen werden
    }

    public List<Player> findPushingNeighbours(Player player, directionEnum direction)
    {
        List<Player> neighbours = new List<Player>();
        findPushingNeigboursHelper(player, direction, neighbours);
        
        return neighbours;
    }

    //ruft nebenliegende Nachbarn rekursiv auf um zu sehen ob diese auch nachbarn haben
    private void findPushingNeigboursHelper(Player player, directionEnum direction, List<Player> neiList)
    {
        int x = player.Charakter.X;
        int y = player.Charakter.Y;

        if (!gameboard.isWalkable(player.Charakter.X, player.Charakter.Y, direction))
        {
            return; //Maybe
        }

        switch (direction)
        {
            case directionEnum.NORTH:
                y += -1;
                break;
            case directionEnum.SOUTH:
                y += 1;
                break;
            case directionEnum.WEST:
                x += -1;
                break;
            case directionEnum.EAST:
                x += 1;
                break;
        }

        foreach (var otherPlayer in players)
        {
            if (otherPlayer.Charakter.X == x && otherPlayer.Charakter.Y == y && !otherPlayer.Charakter.IsDead)
            {
                neiList.Add(otherPlayer);
                findPushingNeigboursHelper(otherPlayer, direction, neiList);
            }
        }
    }*/

    /// <summary>
    /// Findet für jeden Spieler den kürzesten Pfad zum Auge und sortiert zuerst nach Uhrzeigersinn und dann nach der Pfadlänge
    /// Methode gib sowohl eine sortierte Liste zurück, setzt aber gleichzeitig die "players" Liste
    /// </summary>
    /// <returns></returns>
    /*public List<Player> getTurnOrder()
    {
        List<Player> orderedPlayers = new List<Player>();
        foreach (var player in players)
        {
            player.Charakter.pathToEye = FindPath(player.Charakter.GetPosition(), gameboard.eye);
            orderedPlayers.Add(player);
        }


        var tempList = orderedPlayers.OrderBy(player =>
        {
            var xDist = player.Charakter.X - eye.X;
            var yDist = player.Charakter.Y - eye.Y;

            var angle = (Math.Atan2(yDist, xDist)) * 180 / Math.PI;

            if (angle < 0)
            {
                angle += 360; // Winkelbereich von [-180,180] auf [0,360] 
            }

            //todo: direction testen 
            // Um für jede Richtung des Auges um n x 90 Grad versetzen wegen Uhrzeigersinn
            switch (eye.Direction)
            {
                case directionEnum.NORTH:
                    angle += 90;
                    break;
                case directionEnum.EAST:
                    break;
                case directionEnum.SOUTH:
                    angle += 270;
                    break;
                case directionEnum.WEST:
                    angle += 180;
                    break;
            }

            if (angle >= 360)
            {
                angle -= 360;
            }

            return angle;
        }).ToList();
        tempList = tempList.OrderBy(player => player.Charakter.pathToEye.Count).ToList();

        int i = 1;
        foreach (var player in tempList)
        {
            player.Charakter.turnOrder = i;
            i++;
        }

        players = tempList;
        return tempList;
    }*/

    /// <summary>
    ///     A* Pathfinding algorithmus findet den kürzesten Weg von Spieler zum Auge und gibt diesen als Liste zurück.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public List<Tile> FindPath((int, int) player, Tile target)
    {
        var start = target;
        var end = gameboard.tiles[player.Item1, player.Item2];

        var openList = new List<Tile> { start };
        var closedList = new List<Tile>();

        for (var i = 0; i < gameboard.Width; i++)
        for (var j = 0; j < gameboard.Height; j++)
        {
            var node = gameboard.tiles[i, j];
            node.GCost = int.MaxValue;
            node.Parent = null;
        }

        start.GCost = 0;
        start.HCost = calculateDistanceCost(start, end);

        while (openList.Count > 0)
        {
            var currNode = getLowestFCostNode(openList);
            if (currNode == end)
                //Das Ziel wurde erreicht
                return calculatePath(end);

            openList.Remove(currNode);
            closedList.Add(currNode);

            var neighbors = gameboard.GetNeighbors(currNode);

            foreach (var neighbor in neighbors)
            {
                if (closedList.Contains(neighbor)) continue;
                if (neighbor.IsOccupied && neighbor != end)
                {
                    closedList.Add(neighbor);
                    continue;
                }

                var tempGCost = currNode.GCost + calculateDistanceCost(currNode, neighbor);
                if (tempGCost < neighbor.GCost)
                {
                    //wenn der neue Pfad schneller als der alte ist, werden alle stats akutalisiert
                    neighbor.Parent = currNode;
                    neighbor.GCost = tempGCost;
                    neighbor.HCost = calculateDistanceCost(neighbor, end);

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                }
            }
        }

        return new List<Tile>();
    }

    private int calculateDistanceCost(Tile a, Tile b)
    {
        var xDistance = Math.Abs(a.X - b.X);
        var yDistance = Math.Abs(a.Y - b.Y);
        return (xDistance + yDistance) * 10; //MOVE_COST ist konstant
    }

    private Tile getLowestFCostNode(List<Tile> nodeList)
    {
        var lowestFCostNode = nodeList[0];
        for (var i = 0; i < nodeList.Count; i++)
            if (nodeList[i].FCost < lowestFCostNode.FCost)
                lowestFCostNode = nodeList[i];

        return lowestFCostNode;
    }

    private List<Tile> calculatePath(Tile end)
    {
        var path = new List<Tile>();
        //path.Add(end);
        var currNode = end;
        path.Add(end);
        while (currNode.Parent != null) //ist es wirklich immer true ? ich glaube nicht
        {
            path.Add(currNode.Parent);
            currNode = currNode.Parent;
        }

        path.Remove(currNode);
        path.Reverse();
        return path;
    }


    /// <summary>
    /// this method should be called, when a character needs to be revived 
    /// //TODO: Diese Methode testen. Außerdem: geht das vlt effizienter?
    /// </summary>
    /*public void respawnAllDeadCharakters()
    {
        List<StartField> notOccupiedStartFields = new List<StartField>();
        List<Charakter> allDeadCharakters = new List<Charakter>();

        for (int i = 0; i < players.Count; i++)
        {
            if (!isRespawnPointOccupied(players[i].Charakter))
            {
                notOccupiedStartFields.Add(players[i].Charakter.startField);
            }
        }


        for (int m = 0; m < players.Count; m++)
        {
            if (players[m].Charakter.deadRoundsLeft != 0)
            {
                players[m].Charakter.deadRoundsLeft--;
                continue;
            }

            if (players[m].Charakter.IsDead)
            {
                Charakter charakter = players[m].Charakter;
                allDeadCharakters.Add(charakter);

                bool respawnPointOccupied = isRespawnPointOccupied(charakter);

                if (!respawnPointOccupied)
                {
                    if (notOccupiedStartFields.Contains(charakter
                            .startField)) //check if the respawn point is in "notOccupiedStartFields" and remove it and it's character
                    {
                        //this 2. if case should be in every iteration true
                        Console.WriteLine("Wichtig");
                        charakter.respawn(charakter.startField);
                        notOccupiedStartFields.Remove(charakter.startField);
                        allDeadCharakters.Remove(charakter);
                    }
                }
            }
        }

        int numberOfAllDeadCharakter = allDeadCharakters.Count;
        for (int i = 0; i < numberOfAllDeadCharakter; i++) //assign every dead charakter to a not occupied respawn point
        {
            Random rng = new Random();
            int randomNr = rng.Next(notOccupiedStartFields.Count);
            Console.WriteLine($"{randomNr}  {notOccupiedStartFields.Count}");

            allDeadCharakters[0].respawn(notOccupiedStartFields[randomNr]);

            allDeadCharakters.RemoveAt(0);
            notOccupiedStartFields.RemoveAt(randomNr);
        }
    }*/

    /// <summary>
    ///     this is a helper method for the method respawnAllDeadCharakters(). It returns "false" when the respawn Point is
    ///     not occupied and returns "true" when it is occupied.
    /// </summary>
    /// <param name="charakter"></param>
    /// <returns></returns>
    /*private bool isRespawnPointOccupied(Charakter charakter)
    {
        for (int i = 0; i < players.Count; i++) //test if respawn point is occupied from an other player
        {
            Charakter charakterToTest = players[i].Charakter;


            if (!charakterToTest.Equals(charakter) && !charakterToTest.IsDead)
            {
                if (charakterToTest.X.Equals(charakter.startField.X) &&
                    charakterToTest.Y.Equals(charakter.startField.Y))
                {
                    break;
                }
            }

            if (i == players.Count - 1)
            {
                return false;
            }
        }

        return true;
    }*/
    public bool TriggerEagleEvent(Player player)
    {
        var unoccupiedEagleField = gameboard.eagleTiles.Where(p => !p.IsOccupied).ToArray();
        if (!unoccupiedEagleField.Any()) return false;

        var currTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];
        if (currTile is not EagleTile) return false;
        var random = new Random();
        var randomIndex = random.Next(unoccupiedEagleField.Length);
        var newTile = unoccupiedEagleField[randomIndex];
        newTile.IsOccupied = true;
        currTile.IsOccupied = false;
        player.Charakter.X = newTile.X;
        player.Charakter.Y = newTile.Y;
        return true;
    }


    private bool checkHoleandLembasTile(Player player)
    {
        var currentTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];

        if (currentTile is Hole)
        {
            player.Charakter.killCharakter();
            return true;
        }

        if (currentTile is LembasFieldTile lembasFieldTile)
        {
            if (lembasFieldTile.amount <= 0) return false;
            player.Charakter.Lembas += 1;
            lembasFieldTile.amount -= 1;
        }

        return false;
    }

    public bool CheckCheckpoints()
    {
        foreach (var player in players)
            if (gameboard.tiles[player.Charakter.X, player.Charakter.Y] is CheckPoint checkPoint)
                if (player.ReachedCheckpoints + 1 == checkPoint.order)
                {
                    player.ReachedCheckpoints++;
                    if (player.ReachedCheckpoints == gameboard.CheckPoints.Count) return true;
                }

        return false;
    }
}