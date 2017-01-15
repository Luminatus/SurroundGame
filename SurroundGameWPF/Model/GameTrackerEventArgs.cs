using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.Model
{
    public class GameTrackerEventArgs : EventArgs
    {
        public ActionNode Node;
        public GameTrackerEventArgs(ActionNode node)
        {
            Node = node;
        }

    }
}
