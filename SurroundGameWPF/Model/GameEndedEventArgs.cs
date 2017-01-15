using System;
using System.Collections.Generic;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.Model
{
    public  class GameEndedEventArgs : EventArgs
    {
        public LinkedList<Players> Players;
        public bool IsDraw;

        public GameEndedEventArgs(LinkedList<Players> players, bool draw)
        {
            Players = players;
            IsDraw = draw;
        }
    }
    public class GameEndedWithDrawEventArgs : EventArgs
    {
        public LinkedList<Players> Players;

        public GameEndedWithDrawEventArgs( LinkedList<Players> players)
        {
            Players = players;
        }
    }

    public class GameEndedWithWinEventArgs : EventArgs
    {
         Players Player;

        public GameEndedWithWinEventArgs(Players player)
        {
            Player = player;
        }
    }
}
