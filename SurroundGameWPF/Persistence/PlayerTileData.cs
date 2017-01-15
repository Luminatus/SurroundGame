using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.Persistence
{
    public enum TileType
    {
        None,
        Wall,
        Field
    }
    public class Tile
    {
        public TileType Type { get; }
        public TileState State { get; }
        public PlayerNode Player { get; }

        public Tile(TileType type, TileState state, PlayerNode player)
        {
            Type = type;
            State = state;
            Player = player;
        }
    }

    public class PlayerNode
    {
        public Players Name { get; }
        public Tile Wall { get; }
        public Tile Field { get; }

        public PlayerNode(Players name, Tile wall, Tile field)
        {            
            Name = name;
            Wall = wall;
            Field = field;
        }
        public PlayerNode(Players name, TileState wall, TileState field)
        {
            Name = name;
            Wall = new Tile(name==Players.None ? TileType.None : TileType.Wall, wall, this);
            Field = new Tile(name == Players.None ? TileType.None : TileType.Field, field, this);
        }

        public PlayerNode()
        {
            Name = Players.None;
            Tile UnoccupiedTile = new Tile(TileType.None, TileState.Unoccupied, this);
            Wall = UnoccupiedTile;
            Field = UnoccupiedTile;
        }
    }

    public static class PlayerTileData
    {
        private static Dictionary<Players, Tuple<TileState, TileState>> _playerToStateDict = new Dictionary<Players, Tuple<TileState, TileState>>
        {
            {Players.Red, new Tuple<TileState, TileState>
                (TileState.RedWall,
                TileState.RedField)
            },
            {Players.Blue, new Tuple<TileState, TileState>
                ( TileState.BlueWall,
                TileState.BlueField)
            },
            {Players.Green, new Tuple<TileState, TileState>
                (TileState.GreenWall,
                TileState.GreenField)
            },
            {Players.Yellow, new Tuple<TileState, TileState>
                (TileState.YellowWall,
                TileState.YellowField)
            },
            {Players.Purple, new Tuple<TileState, TileState>
                (TileState.PurpleWall,
                TileState.PurpleField)
            },
            {Players.Orange, new Tuple<TileState, TileState>
                (TileState.OrangeWall,
                TileState.OrangeField)
            },
        };
        static bool isDataGenerated = false;

        private static Dictionary<Players, PlayerNode> _playerNodeDict = new Dictionary<Players, PlayerNode>();
        private static Dictionary<TileState, Tile> _tileDict = new Dictionary<TileState, Tile>();

        public static void GenerateData()
        {
            if (isDataGenerated)
                return;

            PlayerNode NonePlayer = new PlayerNode();
            _playerNodeDict.Add(NonePlayer.Name, NonePlayer);
            _tileDict.Add(NonePlayer.Wall.State, NonePlayer.Wall);
            foreach(KeyValuePair<Players, Tuple<TileState, TileState>> playerStatePair in _playerToStateDict)
            {
                PlayerNode player = new PlayerNode(playerStatePair.Key, playerStatePair.Value.Item1, playerStatePair.Value.Item2);
                _playerNodeDict.Add(playerStatePair.Key,player);
                _tileDict.Add(player.Wall.State, player.Wall);
                _tileDict.Add(player.Field.State, player.Field);
            }
            isDataGenerated = true;
            return;
        }

        public static PlayerNode GetPlayer(Players playerName)
        {
            if (!isDataGenerated)
                GenerateData();
            return _playerNodeDict[playerName];
        }


        public static Tile GetTile(TileState tileName)
        {
            if (!isDataGenerated)
                GenerateData();
            return _tileDict[tileName];
        }


    }
}
