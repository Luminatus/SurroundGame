using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.Model
{
    public class StepMadeEventArgs : EventArgs
    {
        public int Row;
        public int Column;
        public bool IsBrickHorizontal;
        public Persistence.Players ActivePlayer;
        public Persistence.TileState[,] Table;

        public StepMadeEventArgs(int row, int col, bool brickHorizontal, Persistence.Players player, Persistence.TileState[,] table)
        {
            Row = row;
            Column = col;
            IsBrickHorizontal = brickHorizontal;
            ActivePlayer = player;
            Table = table;
        }
    }
}
