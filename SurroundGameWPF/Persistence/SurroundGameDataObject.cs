using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.Persistence
{
    public class SurroundGameDataObject
    {
        public SurroundGameTable GameTable { get; }
        public Players[] PlayerArray { get; }
  //      public Players StarterPlayer { get; }

        public SurroundGameDataObject(Players[] players, SurroundGameTable table)
        {
            PlayerArray = players;
            GameTable = table;
   //         StarterPlayer = starter;
        }
    }
}
