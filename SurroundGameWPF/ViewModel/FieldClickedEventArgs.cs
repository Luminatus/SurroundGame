using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.ViewModel
{
    public class FieldClickedEventArgs : EventArgs
    {
        public int Row;
        public int Columm;

        public FieldClickedEventArgs(int x, int y)
        {
            Row = x;
            Columm = y;
        }
    }
}
