using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurroundGameWPF.Extensions;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.Model
{


    public class GameModel
    {
        #region Nested classes

        #region Node class
        private class Node
        {
            public int Row;
            public int Column;

            public Node(int row, int col)
            {
                Row = row;
                Column = col;
            }
        }


        #endregion

        #region BrickNode class
        private class BrickNode
        {
            public Direction stepDirection;
            public int stepCount;
            public int Row { get; set; }
            public int Column { get; set; }
            public bool active = true;
            public int turnCount = 0;
            //       public int StepCount { get { return stepCount; } set { stepCount = value; } }
            public BrickNode(int row, int col, Direction initDirection = Direction.None, int initStep = 0)
            {
                Row = row;
                Column = col;
                stepDirection = initDirection;
                stepCount = initStep;
            }

            public BrickNode(BrickNode bn, bool copyDirection = false, bool copyStep = false) : this(bn.Row, bn.Column, copyDirection ? bn.stepDirection : Direction.None)
            {
                stepCount = copyStep ? bn.stepCount : 0;
            }

            public virtual bool Equals(BrickNode rhs)
            {
                return this.Row == rhs.Row && this.Column == rhs.Column;
            }

            public void SetTo(BrickNode rhs)
            {
                this.Row = rhs.Row;
                this.Column = rhs.Column;
                this.stepCount = rhs.stepCount;
                this.stepDirection = rhs.stepDirection;
            }
        }
        #endregion

        #region Player class
        public class Player
        {
            public Players Name { get; }
            public TileState Wall { get; }
            public TileState Field { get; }
            public int Points = 0;
            public bool IsBrickHorizontal;

            public Player(Players name, TileState wall, TileState field)
            {
                Name = name;
                Wall = wall;
                Field = field;
                IsBrickHorizontal = true;
            }
        }
        #endregion

        #region StepGenerator classes

        private abstract class StepGenerator
        {
            protected GameModel model;
            protected SurroundGameTable innerGameField;
            public int Row { get; }
            public int Column { get; }
            public int CycleCount { get; protected set; }
            public bool isBrickHorizontal { get; }
            public bool IsOutputGenerated { get; protected set; }
            public Players currentPlayer { get; protected set; }

            public Dictionary<Players, int> PointDifference;

            public StepGenerator(GameModel m, int row, int col)
            {
                model = m;
                Row = row;
                Column = col;
                isBrickHorizontal = model.activePlayer.Value.IsBrickHorizontal;
                IsOutputGenerated = false;
                currentPlayer = model.activePlayer.Value.Name;
                PointDifference = new Dictionary<Players, int>();
                var Values = Enum.GetValues(typeof(Players));
                foreach (Players player in Values)
                {
                    if (player != Players.None)
                        PointDifference.Add(player, 0);
                }
            }

            public virtual void InitializeStatic() { }

            public abstract bool Generate();


            public SurroundGameTable GetGameField()
            {
                return innerGameField;
            }

        }      

        private class DirectionalStepGenerator : StepGenerator
        {
            /*
            GameModel model;
            public int Row { get; }
            public int Column { get; }
            public int CycleCount { get; private set; }
            public bool isBrickHorizontal { get; }
            public bool IsOutputGenerated { get; private set; }
            public Players currentPlayer { get; private set; }
            public Dictionary<Players, int> PointDifference;
            */

            public DirectionalStepGenerator(GameModel m, int row, int col) : base(m, row, col) { }
  /*          public DirectionalStepGenerator(GameModel m, int row, int col)
            {
                model = m;
                Row = row;
                Column = col;
                isBrickHorizontal = model.activePlayer.Value.IsBrickHorizontal;
                IsOutputGenerated = false;
                currentPlayer = model.activePlayer.Value.Name;
                PointDifference = new Dictionary<Players, int>();
                var Values = Enum.GetValues(typeof(Players));
                foreach (Players player in Values)
                {
                    if (player != Players.None)
                        PointDifference.Add(player, 0);
                }
            }*/

            LinkedList<BrickNode> intersections = new LinkedList<BrickNode>();

            
            public override bool Generate()
            {
                foreach (KeyValuePair<Players, int> Playerpoint in PointDifference.ToList())
                {
                    PointDifference[Playerpoint.Key] = 0;
                }
                CycleCount = 0;
                innerGameField = new SurroundGameTable(model.gameField);
                int Row2 = isBrickHorizontal ? Row : Row + 1,
                    Column2 = isBrickHorizontal ? Column + 1 : Column;
                bool isValid = innerGameField.CheckTile(Row, Column, TileState.Unoccupied) && innerGameField.CheckTile(Row2, Column2, TileState.Unoccupied);
                IsOutputGenerated = isValid;
                if (!isValid) return false;

                PointDifference[currentPlayer] += 2;
                BrickNode firstBrickNode = new BrickNode(Row, Column);
                BrickNode secondBrickNode = new BrickNode(Row2, Column2);
                BrickNode[] startingPoints = GetStartingPoints(firstBrickNode, secondBrickNode);
                if (startingPoints.Length < 2) isValid = false;

                innerGameField[Row, Column] = model.activePlayer.Value.Wall;
                innerGameField[Row2, Column2] = model.activePlayer.Value.Wall;

                while (CountActive(startingPoints) > 0 && isValid)
                {
                    intersections.Clear();

                    BrickNode startingPoint = GetFirstActive(startingPoints);
                    startingPoint.active = false;
                    BrickNode iteratorNode = new BrickNode(startingPoint);
                    stepNode(startingPoint, true);
                    intersections.AddLast(new BrickNode(startingPoint, true));
                    int turns = 0;
                    iteratorNode.stepCount = 1;
                    do
                    {
                        CycleCount++;
                        int directionCount = 0;
                        GetDirection(ref iteratorNode, ref directionCount, ref turns);
                        if (iteratorNode.stepDirection == Direction.DeadEnd)
                        {
                            if (iteratorNode.Equals(intersections.Last()))
                            {
                                intersections.RemoveLast();

                            }
                            intersections.Last().stepCount = 0;
                            iteratorNode.SetTo(intersections.Last());
                        }
                        else
                        {
                            if (iteratorNode.Equals(intersections.Last()))
                            {
                                intersections.Last().stepDirection = iteratorNode.stepDirection;
                            }
                            else if (iteratorNode.stepDirection != intersections.Last().stepDirection || directionCount >= 2)
                            {
                                intersections.Last().stepCount = iteratorNode.stepCount;
                                iteratorNode.stepCount = 0;
                                intersections.AddLast(new BrickNode(iteratorNode, true));
                            }
                            intersections.Last().turnCount = turns;
                            stepNode(iteratorNode);
                            bool l = false;
                            l = ContainsNode(intersections, iteratorNode);
                            l &= !iteratorNode.Equals(startingPoint);
                            if (l)
                            {
                                intersections.Last().stepCount = 0;
                                iteratorNode.SetTo(intersections.Last());
                            }

                        }

                    } while (!iteratorNode.Equals(firstBrickNode) && !iteratorNode.Equals(secondBrickNode));
                    intersections.Last().stepCount = iteratorNode.stepCount;

                    if (!iteratorNode.Equals(startingPoint))
                    {
                        Direction finalDirection = Direction.None;
                        switch (2 * (iteratorNode.Row - startingPoint.Row) + (iteratorNode.Column - startingPoint.Column))
                        {
                            case -2: finalDirection = Direction.Down; break;
                            case -1: finalDirection = Direction.Right; break;
                            case 1: finalDirection = Direction.Left; break;
                            case 2: finalDirection = Direction.Up; break;
                        }
                        if (finalDirection != intersections.Last().stepDirection)
                        {
                            BrickNode finalIntersection = new BrickNode(iteratorNode.Row, iteratorNode.Column, finalDirection, 1);
                            intersections.AddLast(finalIntersection);
                        }
                        else
                        {
                            intersections.Last().stepCount++;
                        }
                    }
                    LinkedList<Node> nodeList = new LinkedList<Node>();

                    iteratorNode.SetTo(intersections.First());
                    iteratorNode.stepDirection = Turn(iteratorNode.stepDirection, Math.Sign(turns));
                    stepNode(iteratorNode);
                    nodeList.AddLast(new Node(iteratorNode.Row, iteratorNode.Column));
                    iteratorNode.stepDirection = startingPoint.stepDirection;
                    stepNode(iteratorNode);
                    nodeList.AddLast(new Node(iteratorNode.Row, iteratorNode.Column));


                    TileState[] invalidStates = { model.activePlayer.Value.Wall, model.activePlayer.Value.Field };
                    while (nodeList.Count != 0)
                    {
                        CycleCount++;
                        Node innerNode = nodeList.First();
                        if (!innerGameField.CheckTile(innerNode.Row, innerNode.Column, invalidStates))
                        {
                            if (innerGameField[innerNode.Row, innerNode.Column] == TileState.Unoccupied)
                            {
                            }
                            else
                            {
                                Players TileOwner = FindPlayerFromTile(innerGameField[innerNode.Row, innerNode.Column]);
                                PointDifference[TileOwner]--;
                            }
                            innerGameField[innerNode.Row, innerNode.Column] = model.activePlayer.Value.Field;
                            PointDifference[currentPlayer]++;
                            nodeList.AddLast(new Node(innerNode.Row + 1, innerNode.Column));
                            nodeList.AddLast(new Node(innerNode.Row, innerNode.Column + 1));
                            nodeList.AddLast(new Node(innerNode.Row - 1, innerNode.Column));
                            nodeList.AddLast(new Node(innerNode.Row, innerNode.Column - 1));
                            nodeList.AddLast(new Node(innerNode.Row - 1, innerNode.Column - 1));
                            nodeList.AddLast(new Node(innerNode.Row + 1, innerNode.Column - 1));
                            nodeList.AddLast(new Node(innerNode.Row - 1, innerNode.Column + 1));
                            nodeList.AddLast(new Node(innerNode.Row + 1, innerNode.Column + 1));
                        }
                        nodeList.RemoveFirst();

                    }

                }
                return true;
            }

            public Players FindPlayerFromTile(TileState state)
            {
                if (state == TileState.Unoccupied) return Players.None;


                LinkedListNode<Player> player = model.PlayerList.First;
                Func<Player, TileState> getState;
                Players PlayerToReturn = Players.None;

                if (state > TileState.Unoccupied)
                    getState = this.GetWall;
                else
                    getState = this.GetField;

                do
                {
                    if (getState(player.Value) == state)
                        PlayerToReturn = player.Value.Name;
                    player = player.NextOrFirst();
                } while (PlayerToReturn == Players.None && player != model.PlayerList.First);
                return PlayerToReturn;
            }

            public TileState GetWall(Player player)
            {
                return player.Wall;
            }
            public TileState GetField(Player player)
            {
                return player.Field;
            }



            private bool ContainsNode(LinkedList<BrickNode> nodeList, BrickNode bn)
            {
                bool ret = false;
                if (bn.Equals(nodeList.First())) return true;
                LinkedListNode<BrickNode> curr = nodeList.First;
                while (!ret && curr != nodeList.Last)
                {
                    curr = curr.Next;
                    ret = bn.Equals(curr.Value);
                }
                return ret;
            }

            private void GetDirection(ref BrickNode bn, ref int directionCount, ref int turns)
            {
                int sgn = turns == 0 ? 1 : Math.Sign(turns);
                directionCount = 0;
                Direction outputDirection = Direction.None;
                bool isIntersection = bn.Equals(intersections.Last());
                BrickNode referenceNode = isIntersection ? intersections.Last.PreviousOrLast().Value : intersections.Last();
                Direction referenceDirection = (Direction)(0 - (int)(referenceNode.stepDirection));
                if (!isIntersection)
                {
                    bn.stepDirection = referenceDirection;
                    bn.stepDirection = Turn(bn.stepDirection, sgn);
                    do
                    {
                        stepNode(bn);
                        if (innerGameField.CheckTile(bn.Row, bn.Column, model.activePlayer.Value.Wall))
                        {
                            directionCount++;
                            outputDirection = turns == 0 && bn.stepDirection == referenceNode.stepDirection
                                ? bn.stepDirection : outputDirection == Direction.None ? bn.stepDirection : outputDirection;
                        }
                        stepNode(bn, true);
                        bn.stepDirection = Turn(bn.stepDirection, sgn);
                    } while (bn.stepDirection != referenceDirection);

                }
                else
                {
                    bn.stepDirection = intersections.Last().stepDirection;

                    turns -= bn.stepDirection == Turn(referenceNode.stepDirection, 1)
                        ? 1
                        : bn.stepDirection == Turn(referenceNode.stepDirection, -1)
                            ? -1
                            : 0;
                    if (turns == 0)
                    {
                        while (outputDirection == Direction.None)
                        {
                            if (bn.stepDirection == referenceNode.stepDirection) bn.stepDirection = Turn(referenceNode.stepDirection, 1);
                            else if (bn.stepDirection == Turn(referenceNode.stepDirection, 1)) bn.stepDirection = Turn(referenceNode.stepDirection, -1);
                            else
                            {
                                outputDirection = Direction.DeadEnd;
                            }
                            if (outputDirection != Direction.DeadEnd)
                            {
                                stepNode(bn);
                                outputDirection = innerGameField.CheckTile(bn.Row, bn.Column, model.activePlayer.Value.Wall) ? bn.stepDirection : Direction.None;
                                stepNode(bn, true);
                            }
                        }
                    }
                    else
                    {
                        sgn = turns < 0 ? -1 : 1;
                        bn.stepDirection = Turn(bn.stepDirection, sgn);
                    }
                    while (outputDirection == Direction.None && bn.stepDirection != referenceDirection)
                    {
                        stepNode(bn);
                        if (innerGameField.CheckTile(bn.Row, bn.Column, model.activePlayer.Value.Wall))
                        {
                            outputDirection = bn.stepDirection;
                        }
                        stepNode(bn, true);
                        bn.stepDirection = Turn(bn.stepDirection, sgn);
                    }
                }
                if (outputDirection == Direction.None || outputDirection == Direction.DeadEnd) bn.stepDirection = Direction.DeadEnd;
                else
                {
                    bn.stepDirection = outputDirection;
                    turns += bn.stepDirection == Turn(referenceNode.stepDirection, 1)
                        ? 1
                        : bn.stepDirection == Turn(referenceNode.stepDirection, -1)
                            ? -1
                            : 0;
                }
            }

            private Direction Turn(Direction dir, int sgn)
            {
                switch (dir)
                {
                    case Direction.Up: return (Direction)(sgn * (int)Direction.Right);
                    case Direction.Right: return (Direction)(sgn * (int)Direction.Down);
                    case Direction.Down: return (Direction)(sgn * (int)Direction.Left);
                    case Direction.Left: return (Direction)(sgn * (int)Direction.Up);
                    default: return Direction.None;
                }

            }

            private BrickNode GetFirstActive(BrickNode[] nodeArray)
            {
                bool found = false;
                int i = 0;
                while (!found && i < nodeArray.Length)
                {
                    found = nodeArray[i].active;
                    i += found ? 0 : 1;
                }
                return found ? nodeArray[i] : null;
            }

            private int CountActive(BrickNode[] nodeArray)
            {
                int ret = 0;
                foreach (BrickNode bn in nodeArray)
                {
                    if (bn.active) ret++;
                }
                return ret;
            }

            void stepNode(BrickNode bn, bool isReverse = false)
            {
                switch (bn.stepDirection)
                {
                    case Direction.Down: bn.Row += isReverse ? -1 : 1; break;
                    case Direction.Up: bn.Row += isReverse ? 1 : -1; break;
                    case Direction.Left: bn.Column += isReverse ? 1 : -1; break;
                    case Direction.Right: bn.Column += isReverse ? -1 : 1; break;
                    default: return;
                }
                bn.stepCount += isReverse ? -1 : 1;
            }

            private BrickNode[] GetStartingPoints(BrickNode fbn, BrickNode sbn)
            {
                ArrayList startingPointsList = new ArrayList();
                startingPointsList.Capacity = 6;
                int count = 0;
                TileState searchedState = model.activePlayer.Value.Wall;
                if (innerGameField.CheckTile(fbn.Row - 1, fbn.Column, searchedState)) { startingPointsList.Add(new BrickNode(fbn.Row - 1, fbn.Column, Direction.Up)); count++; }
                if (innerGameField.CheckTile(fbn.Row, fbn.Column - 1, searchedState)) { startingPointsList.Add(new BrickNode(fbn.Row, fbn.Column - 1, Direction.Left)); count++; }
                if (innerGameField.CheckTile(fbn.Row + 1, fbn.Column, searchedState) && isBrickHorizontal) { startingPointsList.Add(new BrickNode(fbn.Row + 1, fbn.Column, Direction.Down)); count++; }
                if (innerGameField.CheckTile(fbn.Row, fbn.Column + 1, searchedState) && !isBrickHorizontal) { startingPointsList.Add(new BrickNode(fbn.Row, fbn.Column + 1, Direction.Right)); count++; }
                if (innerGameField.CheckTile(sbn.Row - 1, sbn.Column, searchedState) && isBrickHorizontal) { startingPointsList.Add(new BrickNode(sbn.Row - 1, sbn.Column, Direction.Up)); count++; }
                if (innerGameField.CheckTile(sbn.Row, sbn.Column - 1, searchedState) && !isBrickHorizontal) { startingPointsList.Add(new BrickNode(sbn.Row, sbn.Column - 1, Direction.Left)); count++; }
                if (innerGameField.CheckTile(sbn.Row + 1, sbn.Column, searchedState)) { startingPointsList.Add(new BrickNode(sbn.Row + 1, sbn.Column, Direction.Down)); count++; }
                if (innerGameField.CheckTile(sbn.Row, sbn.Column + 1, searchedState)) { startingPointsList.Add(new BrickNode(sbn.Row, sbn.Column + 1, Direction.Right)); count++; }

                startingPointsList.TrimToSize();
                BrickNode[] startingPoints = new BrickNode[count];
                startingPointsList.CopyTo(startingPoints);
                return startingPoints;
            }



        }

        private class GraphStepGenerator : StepGenerator
        {
            #region GraphStepGenerator Nested Classes
            private class GraphNode : Node
            {
                public int Id { get; }
                public GraphNode UpNode;
                public GraphNode DownNode;
                public GraphNode LeftNode;
                public GraphNode RightNode;
                public int ConnectionNumber { get
                    {
                        int i = 0;
                        if (UpNode != null) i++;
                        if (DownNode != null) i++;
                        if (LeftNode != null) i++;
                        if (RightNode != null) i++;
                        return i;
                    }
                }

                public GraphNode(int row, int col) : base(row, col)
                {
                    Id = 100 * Row + Column;
                }


            }

            private class CheckNode : Node
            {
                public bool IsDiagonal;
                public CheckNode(int row, int col, bool diag = false) : base(row, col)
                {
                    IsDiagonal = diag;
                }
            }

            private class TreeNode : Node
            {
                public TreeNode Parent;
                public static TreeNode Root;
                public int MaxDistance;

                public TreeNode(int row, int column, TreeNode parent) : base(row, column)
                {
                    Parent = parent;
                    int dist = Math.Abs(Row - Root.Row) + Math.Abs(Column - Root.Column);
                    if (dist > Parent.MaxDistance)
                        MaxDistance = dist;
                    else
                        MaxDistance = Parent.MaxDistance;
                }
                public TreeNode(GraphNode node, TreeNode parent) : this(node.Row, node.Column, parent)
                {

                }
            }
            #endregion

            private static GraphNode[] RowIndexers;
            private static GraphNode[] ColumnIndexers;
            private static readonly Node[] HorizontalCheckNodes = new CheckNode[] { new CheckNode(-1,-1,true), new CheckNode(-1,0), new CheckNode(-1, 1), new CheckNode(-1, 2,true), new CheckNode(0, 2), new CheckNode(1, 2,true), new CheckNode(1, 1), new CheckNode(1, 0), new CheckNode(1,-1,true), new CheckNode(0,-1) };
            private static readonly Node[] VerticalCheckNodes = new CheckNode[] { new CheckNode(-1, -1, true), new CheckNode(-1, 0), new CheckNode(-1, 1,true), new CheckNode(0, 1), new CheckNode(1, 1), new CheckNode(2, 1, true), new CheckNode(2, 0), new CheckNode(2, -1,true), new CheckNode(1, -1), new CheckNode(1, 0) };

            public GraphStepGenerator(GameModel m, int row, int col) : base(m, row, col)
            {
                
            }

            public override void InitializeStatic()
            {
                RowIndexers = new GraphNode[model.fieldHeight];
                ColumnIndexers = new GraphNode[model.fieldWidth];
                GraphList.Clear();
                StepId = null;
                Turns = model.Turns;
            }

            public override bool Generate()
            {
                if (IsOutputGenerated)
                    return true;
                innerGameField = new SurroundGameTable(model.gameField);
         //       Node FirstBrick = new Node(Row, Column);
                Node SecondBrick = isBrickHorizontal ?
                    new Node(Row, Column + 1) :
                    new Node(Row + 1, Column);

                bool IsValid =
                    innerGameField.CheckTile(Row, Column) &&
                    innerGameField.CheckTile(SecondBrick.Row, SecondBrick.Column);
                if (!IsValid)
                {
                    IsOutputGenerated = false;
                    return IsOutputGenerated;
                }
                TileState Wall = model.activePlayer.Value.Wall;
                TileState Field = model.activePlayer.Value.Field;

                innerGameField[Row, Column] = Wall;
                innerGameField[SecondBrick.Row, SecondBrick.Column] = Wall;

                LinkedList<TileState> SurroundingWalls = new LinkedList<TileState>();
                if(isBrickHorizontal)
                    foreach(CheckNode node in HorizontalCheckNodes)
                    {
                        SurroundingWalls.AddLast(innerGameField[Row + node.Row, Column + node.Column]);
                    }
                else
                    foreach(CheckNode node in VerticalCheckNodes)
                    {
                        SurroundingWalls.AddLast(innerGameField[Row + node.Row, Column + node.Column]);
                    }
                TreeNode.Root = new TreeNode(Row, Column, null);

                


                return true;
            }

            private static LinkedList<GraphNode> GraphList = new LinkedList<GraphNode>();
            private static string StepId;
            private static int Turns;
        }

        #endregion

        #region TrackPlayer
        private class TrackPlayer
        {
            LinkedList<ActionNode> TrackList;
            LinkedListNode<ActionNode> ActiveNode;
            LinkedListNode<ActionNode> EndNode = new LinkedListNode<ActionNode>(new ActionNode(ActionType.EndGame));
            public GameInfo TrackGameInfo { get; private set; }
            public ActionNode CurrentAction { get { return ActiveNode.Value; } }
            public bool IsTrackEnded { get { return CurrentAction.Action == ActionType.EndGame; } }

            public TrackPlayer()
            {
                TrackList = new LinkedList<ActionNode>();
            }
            public void NextAction()
            {
                if (ActiveNode == null)
                    ActiveNode = TrackList.First;
                else
                {
                    if (ActiveNode == TrackList.Last)
                        ActiveNode = EndNode;
                    else
                        ActiveNode = ActiveNode.Next;
                }
                if (IsTrackEnded)
                    return;
            }
            public void ToAction(int index)
            {
                if (index >= 0 && index < TrackList.Count)
                {
                    ActiveNode = new LinkedListNode<ActionNode>(TrackList.ElementAt(index));
                    while (CurrentAction.Table == null && ActiveNode != TrackList.First)
                    {
                        ActiveNode = ActiveNode.Previous;
                    }
                    //To complete
                }
            }

            public bool LoadTrack(int index)
            {
                TrackGameInfo = null;
                GameTrackerDataObject dataObject = GameTracker.LoadTracker(index);
                if (dataObject != null)
                {
                    TrackList = dataObject.TrackList;
                    TrackGameInfo = new GameInfo(dataObject);
                    
                }
                return TrackGameInfo != null; ;

            }

            public void EnableTracking()
            {
                GameTracker.ActionLogged += new EventHandler<GameTrackerEventArgs>(Tracker_Logged);
            }

            private void Tracker_Logged(object sender, GameTrackerEventArgs e)
            {
                TrackList.AddLast(e.Node);
            }

        }
        #endregion
        #region GameInfo class
        private class GameInfo
        {
            public int RowNumber { get; }
            public int ColumnNumber { get; }
            public Players[] PlayerNames { get; }

            public GameInfo(int row, int col, Players[] players)
            {
                RowNumber = row;
                ColumnNumber = col;
                PlayerNames = players;
            }
            public GameInfo(GameTrackerDataObject data) : this(data.RowNumber, data.ColumnNumber, data.PlayerArray) { }
        }
        #endregion
        #endregion

        #region Fields

        Node PlacableTileIndex;
        int EndGameCheckThreshold;
        StepGenerator currentGenerator;
        LinkedList<StepGenerator> generatorList;
        SurroundGameTable gameField;
        LinkedList<Player> PlayerList;
        LinkedListNode<Player> activePlayer;
        ISurroundGameDataAccess dataAccess;
        TrackPlayer trackPlayer;
        GameInfo gameInfo;
        bool isPlayingBack;

        #endregion

        #region Properties

        public Guid Id { get; private set; }
        public int fieldWidth { get { return gameInfo == null ? 0 : gameInfo.ColumnNumber; } }
        public int fieldHeight { get { return gameInfo == null ? 0 : gameInfo.RowNumber; } }
        public bool isBrickHorizontal { get { return activePlayer.Value.IsBrickHorizontal; } }
        public int Turns { get; private set; }
        public Player CurrentPlayer { get { return activePlayer.Value; } }

        public TileState[,] GameField { get { return gameField.ToArray(); } }
        public bool IsPlaybackEnabled { get; private set; }
        public bool IsGame { get; private set; }

        public bool IsPlayingBack { get { return IsPlaybackEnabled && isPlayingBack; } }

        public bool IsModeSet { get { return IsGame || IsPlaybackEnabled; } }

        public bool IsGameOver { get { return (PlacableTileIndex == null || (PlacableTileIndex.Row == -1 && PlacableTileIndex.Column == -1)); } }
        
        public ActionNode CurrentAction {  get { return trackPlayer.CurrentAction; } }
        
        #endregion
        public Players[] PlayerNames { get { return gameInfo.PlayerNames; } }

        public int TilesLeft { get { return gameField.tilesLeft; } }

        public int GetPoints(Players player)
        {
            LinkedListNode<Player> curr = PlayerList.First;
            do
            {
                if (curr.Value.Name == player) return curr.Value.Points;
                curr = curr.NextOrFirst();
            } while (curr != PlayerList.First);
            return -1;
        }

        public TileState GetState(int row, int col, bool IsPreview = false)
        {
            if (IsPreview && currentGenerator != null)
                return currentGenerator.GetGameField()[row, col];
            else
                return gameField[row, col];
        }





        public event EventHandler<FieldChangedEventArgs> FieldChanged;
        public event EventHandler<MapGeneratedEventArgs> MapGenerated;
        public event EventHandler<GameEndedEventArgs> GameEnded;
        public event EventHandler GameSuspended;
        public event EventHandler<GameEndedWithDrawEventArgs> GameEndedWithDraw;
        public event EventHandler<GameEndedWithWinEventArgs> GameEndedWithWin;
        public event EventHandler<StepMadeEventArgs> StepMade;
        public event EventHandler BrickRotated;


        public void OnFieldChanged(int row, int col, Players player)
        {
            if (FieldChanged != null)
            {
                FieldChanged(this, new FieldChangedEventArgs(row, col, player));
            }
        }
        public void OnMapGenerated()
        {
            if (MapGenerated != null)
            {
                Dictionary<Players, PlayerPoints> dict = new Dictionary<Players, PlayerPoints>();
                foreach (Player player in PlayerList)
                {

                    dict.Add(player.Name, new PlayerPoints(player.Points, currentGenerator != null ? currentGenerator.PointDifference[player.Name] : 0));
                }

                MapGenerated(this, new MapGeneratedEventArgs(dict));
            }
        }

        private void OnStepMade(StepGenerator generator)
        {
            if (StepMade != null)
            {
                StepMade(this, new StepMadeEventArgs(generator.Row, generator.Column, generator.isBrickHorizontal, generator.currentPlayer, generator.GetGameField().ToArray()));
            }
        }

        public void OnGameEnded()
        {
            LinkedList<Players> Winners = new LinkedList<Players>();
            LinkedListNode<Player> PlayerIterator = PlayerList.First;
            Winners.AddLast(PlayerIterator.Value.Name);
            int WinnerPoints = PlayerIterator.Value.Points;
            PlayerIterator = PlayerIterator.NextOrFirst();
            while (PlayerIterator != PlayerList.First)
            {
                if (PlayerIterator.Value.Points > WinnerPoints)
                {
                    Winners.Clear();
                    Winners.AddLast(PlayerIterator.Value.Name);
                    WinnerPoints = PlayerIterator.Value.Points;
                }
                else if (PlayerIterator.Value.Points == WinnerPoints)
                {
                    Winners.AddLast(PlayerIterator.Value.Name);
                }
                PlayerIterator = PlayerIterator.NextOrFirst();
            }
            if(GameEnded != null)
            {
                GameEnded(this, new GameEndedEventArgs(Winners, Winners.Count > 1));
            }
         /*   if (Winners.Count > 1)
            {
                if (GameEndedWithDraw != null)
                {
                    GameEndedWithDraw(this, new GameEndedWithDrawEventArgs(Winners));
                }
            }
            else
            {
                if (GameEndedWithWin != null)
                {
                    GameEndedWithWin(this, new GameEndedWithWinEventArgs(Winners.First()));
                }
            }*/
            Console.WriteLine("Game ended");
        }

        public GameModel(ISurroundGameDataAccess access = null)
        {
            PlayerList = new LinkedList<Player>();
            generatorList = new LinkedList<StepGenerator>();
            dataAccess = access;
            Id = new Guid();
            IsGame = IsPlaybackEnabled = isPlayingBack = false;
        }

        public bool NewPlayback(int index)
        {
            if (!IsGameOver)
                throw new SurroundGameModelException("Cannot start new playback, while game is running!");
            trackPlayer = new TrackPlayer();
            if (trackPlayer.LoadTrack(index))
            {
                IsPlaybackEnabled = true;
                isPlayingBack = true;
                IsGame = false;
                gameInfo = trackPlayer.TrackGameInfo;
                gameField = new SurroundGameTable(gameInfo.RowNumber, gameInfo.ColumnNumber);
                SetPlayers();
                InitializeGame();
                return true;
            }
            else
                return false;
        }

        public void NewGame(int rows, int cols, int playerNum=2, bool toTrack=true)
        {
            if (!IsGameOver)
                throw new SurroundGameModelException("Cannot start new game, while game is running!");
            if (rows < 6 || rows > 30 || cols < 6 || cols > 30)
                throw new SurroundGameModelException("Invalid game size!");
            gameField = new SurroundGameTable(rows, cols);
            IsGame = true;

            Players[] players = new Players[playerNum];
            for(int i=0; i< playerNum; i++)
            {
                players[i] = (Players)(i+1);
            }

            gameInfo = new GameInfo(rows, cols, players);

            SetPlayers();
            if (toTrack)
            {
                IsPlaybackEnabled = true;
                trackPlayer = new TrackPlayer();
                trackPlayer.EnableTracking();
                GameTracker.Track(this);
            }
            InitializeGame();
        }

        private void SetPlayers()
        {
            if (!IsGameOver)
                throw new SurroundGameModelException("Cannot set players while game is running");
            if (gameInfo == null || gameInfo.PlayerNames.Length == 0)
                throw new SurroundGameModelException("Game Info was not set properly!");
            if (PlayerList == null)
                PlayerList = new LinkedList<Player>();
            else
                PlayerList.Clear();
            foreach (Players p in gameInfo.PlayerNames)
            {
                PlayerList.AddLast(new Player(p, (TileState)p, (TileState)(0-p)));
            }

        }

        public void NewGame(int size) { NewGame(size, size, 2); }

        public async Task LoadGame(string fileName)
        {
            if (dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");
            SurroundGameDataObject dataObject = await dataAccess.Load(fileName);
            gameField = dataObject.GameTable;
            int StartingPoint = 0;
            gameInfo = new GameInfo(gameField.FieldHeight, gameField.FieldWidth, dataObject.PlayerArray);
            foreach (Players player in dataObject.PlayerArray)
            {
                int PlayerNumber = (int)player;
                TileState wall = (TileState)PlayerNumber;
                TileState field = (TileState)(-PlayerNumber);
                Player playerObject = new Player(player, wall, field);
                for (int i = StartingPoint; i < fieldWidth * fieldHeight; i++)
                {
                    TileState state = gameField[i / fieldWidth, i % fieldWidth];
                    if (state == wall || state == field)
                    {
                        StartingPoint += StartingPoint == i ? 1 : 0;
                        playerObject.Points++;
                    }
                    else if (state == TileState.Unoccupied)
                    {
                        StartingPoint += StartingPoint == i ? 1 : 0;
                    }
                }
                PlayerList.AddLast(playerObject);
            }
            IsGame = true;
            InitializeGame();

        }

        public async Task SaveGame(string fileName)
        {
            if (dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");
            Players[] playerNames = new Players[PlayerList.Count];
            LinkedListNode<Player> playerIterator = activePlayer;
            for (int i = 0; i < playerNames.Length; i++)
            {
                playerNames[i] = playerIterator.Value.Name;
                playerIterator = playerIterator.NextOrFirst();
            }
            SurroundGameDataObject dataObject = new SurroundGameDataObject(playerNames, gameField);
            await dataAccess.Save(fileName, dataObject);
        }


        public void EndGame()
        {
            if (GameSuspended != null)
            {
                GameSuspended(this, EventArgs.Empty);
            }
        }

        private void InitializeGame()
        {
            activePlayer = PlayerList.First;
            PlacableTileIndex = new Node(0, 0);
            EndGameCheckThreshold = gameField.FieldHeight * gameField.FieldWidth / 3;
            Turns = 0;
        }



        public void RotateBrick()
        {
            if (IsGame)
            {
                InnerRotateBrick();
             }
        }

        private void InnerRotateBrick()
        {
            activePlayer.Value.IsBrickHorizontal = !activePlayer.Value.IsBrickHorizontal;
            if (BrickRotated != null)
                BrickRotated(this, EventArgs.Empty);
        }

        public void GetFirstPlacableTile()
        {
            bool FoundPlacableTile = false;
            while (!FoundPlacableTile && PlacableTileIndex.Row < fieldHeight)
            {
                if (gameField.CheckTile(PlacableTileIndex.Row, PlacableTileIndex.Column) &&
                   (gameField.CheckTile(PlacableTileIndex.Row, PlacableTileIndex.Column + 1)
                   || gameField.CheckTile(PlacableTileIndex.Row + 1, PlacableTileIndex.Column)))
                {
                    FoundPlacableTile = true;
                }
                else
                {
                    if (PlacableTileIndex.Column == fieldWidth - 1)
                    {
                        PlacableTileIndex.Row++;
                        PlacableTileIndex.Column = 0;
                    }
                    else
                    {
                        PlacableTileIndex.Column++;
                    }
                }
            }
            if (!FoundPlacableTile)
            {
                PlacableTileIndex.Row = PlacableTileIndex.Column = -1;
            }
        }



        public void CheckPlacement(int row, int col)
        {
            if (IsGame)
                InnerCheckPlacement(row, col);
        }

        private void InnerCheckPlacement(int row, int col)
        {
            bool IsLastOutputGenerated = currentGenerator != null ? currentGenerator.IsOutputGenerated : false;
            bool IsOutputGenerated = false;
            bool IsGeneratorCached = false;
            if (generatorList.Count > 0)
            {
                LinkedListNode<StepGenerator> generatorIterator = generatorList.First;
                do
                {
                    if (generatorIterator.Value.Row == row && generatorIterator.Value.Column == col && generatorIterator.Value.isBrickHorizontal == activePlayer.Value.IsBrickHorizontal)
                    {
                        currentGenerator = generatorIterator.Value;
                        Console.Write("Using cached generator: ");
                        IsOutputGenerated = true;
                        IsGeneratorCached = true;
                    }
                    generatorIterator = generatorIterator.NextOrFirst();
                } while (currentGenerator == null && generatorIterator != generatorList.First);
            }
            if (!IsOutputGenerated)
            {
                currentGenerator = new DirectionalStepGenerator(this, row, col);
                IsOutputGenerated = currentGenerator.Generate();
            }
            if (!IsOutputGenerated)
            {
                currentGenerator = null;
                if (IsLastOutputGenerated)
                    OnMapGenerated();
            }
            else
            {
                if (currentGenerator.CycleCount > 100 && !IsGeneratorCached)
                {
                    generatorList.AddFirst(currentGenerator);
                    if (generatorList.Count > 5)
                    {
                        generatorList.RemoveLast();
                    }
                    Console.WriteLine(generatorList.Count);
                }
                OnMapGenerated();
                Console.WriteLine(currentGenerator.CycleCount);
            }
        }

        public void MakeStep(int row, int col)
        {
            if (IsGame)
            {
                CheckPlacement(row, col);
                MakeStep();
            }
        }

        public void MakeStep()
        {
            if (IsPlayingBack)
            {
                trackPlayer.NextAction();
                while (trackPlayer.CurrentAction.Action == ActionType.Rotate && !trackPlayer.IsTrackEnded)
                {
                    InnerRotateBrick();
                    trackPlayer.NextAction();
                }
                if (trackPlayer.IsTrackEnded)
                {
                    PlacableTileIndex = null;
                    OnGameEnded();
                    return;
                }
                else
                {
                    ActionNode node = trackPlayer.CurrentAction;
                    InnerCheckPlacement(node.Row, node.Column);
                }
            }
            if (currentGenerator != null)
            {
                gameField = currentGenerator.GetGameField();
                foreach (Player player in PlayerList)
                {
                    player.Points += currentGenerator.PointDifference[player.Name];
                }
                Turns++;
                OnStepMade(currentGenerator);
                currentGenerator = null;
                activePlayer = activePlayer.NextOrFirst();
                OnMapGenerated();
                generatorList.Clear();
                Console.WriteLine("Tiles remaining: " + gameField.tilesLeft);
                if (gameField.tilesLeft < EndGameCheckThreshold)
                {
                    Console.Write("Looking for usable blocks. ");
                    GetFirstPlacableTile();

                    Console.WriteLine("Found: " + PlacableTileIndex.Row + ":" + PlacableTileIndex.Column);
                    if (IsGameOver)
                    {
                        OnGameEnded();
                    }
                }
            }
        }

    }

}