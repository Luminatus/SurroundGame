using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SurroundGame.Model;

namespace SurroundGame.Persistence
{
    class GameTracker
    {
        private const string DEFAULT_PATH = "tracklog.log";
        private const string TESTFILE_PATH = "testracklog.log";
        private GameModel _model;
        private string FilePath;
        private LinkedList<ActionNode> TrackList;
        public bool IsTracking;
        public GameTracker(string path = DEFAULT_PATH)
        {
            FilePath = path;
            IsTracking = false;
        }
        public void SetModel(GameModel model)
        {
            _model = model;
        }
        public bool Track()
        {
            TrackList = new LinkedList<ActionNode>();
            if (_model == null)
                return false;
            _model.BrickRotated += new EventHandler(TrackRotate);
            _model.StepMade += new EventHandler<StepMadeEventArgs>(TrackStep);
            return true;
        }

        private void TrackRotate(object sender, EventArgs e)
        {
            TrackList.AddLast(new ActionNode(ActionType.Rotate));
        }

        private void TrackStep(object sender, StepMadeEventArgs e)
        {
            TrackList.AddLast(new ActionNode(ActionType.Step, e.Table, e.Row, e.Column));
        }

        public async void Save()
        {
            SaveFile(FilePath, false);
            SaveFile(TESTFILE_PATH, true);               
        }

        private async Task SaveFile(string path, bool isTest)
        { 
            try
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    string line = "";
                    writer.WriteLine(String.Format("{0} {1}", _model.fieldHeight, _model.fieldWidth));
                    foreach (Players player in _model.PlayerNames)
                    {
                        if (player == _model.PlayerNames.Last())
                            writer.WriteLine((int)player);
                        else
                            writer.Write((int)player + " ");
                    }
                    foreach(ActionNode node in TrackList)
                    {
                        writer.Write((int)node.Action);
                        if (node.Action == ActionType.Rotate)
                            writer.WriteLine();
                        else
                        {
                            writer.WriteLine(String.Format(" {0} {1}", node.PosY, node.PosX));
                            Console.WriteLine(isTest ? "Test" : "Simple");
                            if (isTest)
                            {
                                for (int i = 0; i < _model.fieldHeight * _model.fieldWidth; i++)
                                {
                                    line += (int)node.Table[i / _model.fieldWidth, i % _model.fieldWidth];
                                    if ((i + 1 ) % _model.fieldWidth == 0)
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

    }

    public struct ActionNode
    {
        public ActionType Action {get; private set; }
        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public TileState[,] Table { get; private set; }
        public ActionNode(ActionType type, TileState[,] table = null, int x = -1, int y = -1)
        {
            Action = type;
            PosX = type == ActionType.Step ? x : -1;
            PosY = type == ActionType.Step ? y : -1;
            Table = table;
        }
    }
}
