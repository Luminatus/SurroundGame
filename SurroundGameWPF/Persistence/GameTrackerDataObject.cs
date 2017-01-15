using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurroundGameWPF.Model;

namespace SurroundGameWPF.Persistence
{
    public class GameTrackerDataObject
    {
        public LinkedList<ActionNode> TrackList { get; }
        public Players[] PlayerArray { get; }
        public int RowNumber { get; }
        public int ColumnNumber { get; }

        public GameTrackerDataObject(LinkedList<ActionNode> list, Players[] players, int row, int col)
        {
            TrackList = list;
            PlayerArray = players;
            RowNumber = row;
            ColumnNumber = col;
        }
    }
}
