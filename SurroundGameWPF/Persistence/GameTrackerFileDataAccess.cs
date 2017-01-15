using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurroundGameWPF.Model;
using System.IO;

namespace SurroundGameWPF.Persistence
{
    public struct InputTrackData
    {
        public int RowNumber { get; }
        public int ColNumber { get; }
        public int StepCount { get; }
        public int PlayerCount { get; }
        public string[] Data { get; }

        public InputTrackData(int rowNum, int colNum, int stepCount, int playerCount, string[] data)
        {
            RowNumber = rowNum;
            ColNumber = colNum;
            StepCount = stepCount;
            PlayerCount = playerCount;
            Data = data;
        }
    }
    class GameTrackerFileDataAccess
    {
        private const string END_FLAG = "#####";
        InputTrackData[] Data;
        public bool IsTestData { get; private set; }
        public bool IsDataLoaded { get; private set; }
        public int DataCount { get { return IsDataLoaded ? Data.Length : 0; }  }

        public GameTrackerFileDataAccess()
        {
            IsTestData = IsDataLoaded = false;            
        }
        public async Task SaveFile(GameTrackerDataObject dataObject, bool isTest = false)
        {
            if (dataObject.TrackList.Count == 0)
                return;
            string path = isTest ? "testtracklog.log" : SurroundGameWPF.Properties.Settings.Default.LogFilePath;
            try
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    string line = "";
                    writer.WriteLine(String.Format("{0} {1}", dataObject.RowNumber, dataObject.ColumnNumber));
                    foreach (Players player in dataObject.PlayerArray)
                    {
                        if (player == dataObject.PlayerArray.Last())
                            writer.WriteLine((int)player);
                        else
                            writer.Write((int)player + " ");
                    }
                    foreach (ActionNode node in dataObject.TrackList)
                    {
                        writer.Write((int)node.Action);
                        if (node.Action == ActionType.Rotate)
                            writer.WriteLine();
                        else
                        {
                            writer.WriteLine(String.Format(" {0} {1}", node.Column, node.Row));
                            Console.WriteLine(isTest ? "Test" : "Simple");
                            if (isTest)
                            {
                                for (int i = 0; i < dataObject.RowNumber * dataObject.ColumnNumber; i++)
                                {
                                    line += (int)node.Table[i / dataObject.ColumnNumber, i % dataObject.ColumnNumber];
                                    if ((i + 1) % dataObject.ColumnNumber == 0)
                                    {
                                        writer.WriteLine(line);
                                        line = "";
                                    }
                                    else
                                        line += " ";
                                }
                            }
                        }
                    }
                    writer.WriteLine("#####");
                }
            }
            catch
            {

            }
        }

        public  void PreLoad(bool isTest = false)
        {
            IsTestData = isTest;
            string path = isTest ? "testtracklog.log" : SurroundGameWPF.Properties.Settings.Default.LogFilePath;
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    int rowNum, colNum, playerNum, stepCount;
                    LinkedList<InputTrackData> dataList = new LinkedList<InputTrackData>();
                    while (!reader.EndOfStream)
                    {
                        rowNum = colNum = playerNum = stepCount = 0;
                        line =  reader.ReadLine();
                        string[] numbers = line.Split(' ');
                        rowNum = Int32.Parse(numbers[0]);
                        colNum = Int32.Parse(numbers[1]);

                        LinkedList<string> innerData = new LinkedList<string>();
                        innerData.AddLast(line);

                        line = reader.ReadLine();
                        numbers = line.Split(' ');
                        playerNum = numbers.Length;
                        innerData.AddLast(line);

                        line = reader.ReadLine();
                        while (line != END_FLAG)
                        {
                            innerData.AddLast(line);
                            if (line.Split(' ').First() == "1")
                            {
                                stepCount++;
                                for (int i = 0; i < rowNum && isTest; i++)
                                {
                                    line =  reader.ReadLine();
                                    if (line == END_FLAG)
                                        throw new SurroundGameDataException();
                                    else
                                        innerData.AddLast(line);
                                }
                            }
                            line = reader.ReadLine();
                        }
                        dataList.AddLast(new InputTrackData(rowNum, colNum, stepCount, playerNum, innerData.ToArray()));
                    }
                    Data = dataList.ToArray();
                }
                IsDataLoaded = true;
            }
            catch
            {
                IsDataLoaded = false;
                throw new SurroundGameDataException();
            }
        }

        public GameTrackerDataObject LoadData(int index)
        {
            if (index >= Data.Length || index < 0 || !IsDataLoaded)
                return null;
            InputTrackData data = Data[index];
            Players[] playerArray = new Players[data.PlayerCount];
            string[] playerString = data.Data[1].Split(' ');
            if (playerString.Length != data.PlayerCount)
                return null;
            for (int i = 0; i < data.PlayerCount; i++)
            {
                playerArray[i] = (Players)Int32.Parse(playerString[i]);
            }
            LinkedList<ActionNode> actionList = new LinkedList<ActionNode>();
            try
            {
                for (int i = 2; i < data.Data.Length; i++)
                {
                    string[] lineString = data.Data[i].Split(' ');
                    ActionType actionType = (ActionType)Int32.Parse(lineString[0]);
                    TileState[,] gameField = null;
                    if (actionType == ActionType.Step)
                    {
                        int actionRow = Int32.Parse(lineString[2]);
                        int actionCol = Int32.Parse(lineString[1]);
                        if (IsTestData)
                        {
                            gameField = new TileState[data.RowNumber, data.ColNumber];
                            for (int j = 1; j <= data.RowNumber; j++)
                            {
                                lineString = data.Data[i + j].Split(' ');
                                for (int k = 0; k < data.ColNumber; k++)
                                {
                                    gameField[j-1, k] = (TileState)Int32.Parse(lineString[k]);
                                }
                            }
                            i += data.RowNumber;
                        }

                        actionList.AddLast(new ActionNode(ActionType.Step, gameField, actionRow, actionCol));
                    }
                    else
                    {
                        actionList.AddLast(new ActionNode(ActionType.Rotate));
                    }

                }
            }
            catch
            {
                return null;
            }

            GameTrackerDataObject dataObject = new GameTrackerDataObject(actionList, playerArray, data.RowNumber, data.ColNumber);
            return dataObject;
        }
    }
}
