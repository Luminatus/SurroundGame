using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.Model
{
    public class SurroundGameModelException : Exception
    {
        public SurroundGameModelException(string exceptionText) : base(exceptionText) { }

    }
}

