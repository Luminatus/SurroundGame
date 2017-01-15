using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SurroundGameWPF.Persistence
{
    class SurroundGameFileDataAccess : ISurroundGameDataAccess
    {
        public async Task<SurroundGameDataObject> Load(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    string line = await reader.ReadLineAsync();
                    string[] numbers = line.Split(' '); // beolvasunk egy sort, és a szóköz mentén széttöredezzük
                    int tableRowNumber = Int32.Parse(numbers[0]); // beolvassuk a tábla méretét
                    int tableColumnNumber = Int32.Parse(numbers[1]); // beolvassuk a házak méretét

                    line = await reader.ReadLineAsync();
                    numbers = line.Split(' ');
                    int playerNum = numbers.Length;
                    SurroundGameTable table = new SurroundGameTable(tableRowNumber,tableColumnNumber); // létrehozzuk a táblát
                    Players[] players = new Players[playerNum];
                    for(int i=0; i<playerNum; i++)
                    {
                        Players player = (Players)Int32.Parse(numbers[i]);
                        if (player == Players.None || players.Contains(player))
                            throw new SurroundGameDataException();
                        players[i] = (Players)Int32.Parse(numbers[i]);
                    } 

                    for (Int32 i = 0; i < tableRowNumber; i++)
                    {
                        line = await reader.ReadLineAsync();
                        numbers = line.Split(' ');
                        for (Int32 j = 0; j < tableColumnNumber; j++)
                        {
                            table[i,j] = (TileState)Int32.Parse(numbers[j]);
                        }
                    }
                    SurroundGameDataObject dataObject = new SurroundGameDataObject(players,table);
                    return dataObject;
                }
            }
            catch
            {
                throw new SurroundGameDataException();
            }
        }

        public async Task Save(string fileName, SurroundGameDataObject dataObject)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName)) // fájl megnyitása
                {
                    writer.Write(dataObject.GameTable.FieldHeight); // kiírjuk a méreteket
                    await writer.WriteLineAsync(" " + dataObject.GameTable.FieldWidth);
                    int PlayerLength = dataObject.PlayerArray.Length;
                    for (int i=0; i<PlayerLength-1; i++)
                    {
                        writer.Write((int)dataObject.PlayerArray[i] + " ");
                    }
                    await writer.WriteLineAsync(((int)dataObject.PlayerArray[PlayerLength-1]).ToString());

                    for (int i = 0; i < dataObject.GameTable.FieldHeight; i++)
                    {
                        for (int j = 0; j < dataObject.GameTable.FieldWidth-1; j++)
                        {
                            await writer.WriteAsync((int)dataObject.GameTable[i, j] + " "); // kiírjuk az értékeket
                        }
                        await writer.WriteLineAsync(((int)dataObject.GameTable[i, dataObject.GameTable.FieldWidth - 1]).ToString());
                    }
                }
            }
            catch
            {
                throw new SurroundGameDataException();
            }
        }
    }
}
