using System;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.Model
{
    public class FieldChangedEventArgs : EventArgs
    {
        public int Row { get; private set; }
       ///<summary>
       ///Oszlop lekérdezése
       ///</summary>
        public int Column { get; private set; }

        public Players Player;

        public FieldChangedEventArgs(int row, int col, Players player)
        {
            Row = row;
            Column = col;
            Player = player;
        }
    }
}
