using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.Model
{
    class MainModel
    {
        const int minimumSize = 6;
        const int maximumSize = 30;

        public MainModel()
        {

        }

        public bool ValidateNewGame(int row, int col)
        {
            return
                    row >= minimumSize
                && row <= maximumSize
                && col >= minimumSize
                && col <= maximumSize;
        }
        
    }
}
