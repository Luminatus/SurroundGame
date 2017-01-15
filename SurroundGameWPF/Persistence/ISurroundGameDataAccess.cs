using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.Persistence
{
    public interface ISurroundGameDataAccess
    {
        Task<SurroundGameDataObject> Load(string path);

        Task Save(string fileName, SurroundGameDataObject dataObject);
    }
}
