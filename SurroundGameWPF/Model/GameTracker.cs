using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.Model
{
    public static class GameTracker
    {
        private static GameModel _model;
        private static LinkedList<ActionNode> TrackList;
        public static bool IsTracking { get; private set; }
        public static int CachedTracks { get { if (DataAccess == null) return 0; return DataAccess.DataCount; } }
        private static GameTrackerFileDataAccess DataAccess = new GameTrackerFileDataAccess();
        public static event EventHandler<GameTrackerEventArgs> ActionLogged;
        public static void SetModel(GameModel model)
        {
            if (_model != null)
            {
                _model.BrickRotated -= TrackRotate;
                _model.StepMade -= TrackStep;
                _model.GameEnded -= GameEnded;
            }
            _model = model;
            if (_model != null)
            {
                _model.BrickRotated += new EventHandler(TrackRotate);
                _model.StepMade += new EventHandler<StepMadeEventArgs>(TrackStep);
                _model.GameEnded += new EventHandler<GameEndedEventArgs>(GameEnded);
                _model.GameSuspended += new EventHandler(GameEnded);
            }
            IsTracking = false;
        }
        public static bool Track()
        {
            TrackList = new LinkedList<ActionNode>();
            IsTracking = _model != null;
            return IsTracking;
        }

        public static bool Track(GameModel model)
        {
            SetModel(model);
            return Track();
        }

        public static void StopTrack()
        {
            IsTracking = false;
        }

        public static bool IsTrackingModel(Guid id)
        {
            return _model.Id == id;
        }
        /*public void NextAction()
        {
            if (currentActionNode == TrackList.Last)
                return;
            if (currentActionNode == null)
                currentActionNode = TrackList.First;
            else
                currentActionNode = currentActionNode.Next;
        }
        */

        private async static void GameEnded(object sender, EventArgs e)
        {
            await Save();
            SetModel(null);
        }

        private static void TrackRotate(object sender, EventArgs e)
        {
            if (IsTracking)
                TrackList.AddLast(new ActionNode(ActionType.Rotate));
        }

        private static void TrackStep(object sender, StepMadeEventArgs e)
        {
            if (IsTracking)
            {
                ActionNode node = new ActionNode(ActionType.Step, e.Table, e.Row, e.Column);
                TrackList.AddLast(node);
                ActionLogged(null, new GameTrackerEventArgs(node));
            }
        }

        public static async Task Save()
        {
            if (_model != null)
            {
                GameTrackerDataObject dataObject = new GameTrackerDataObject(TrackList, _model.PlayerNames, _model.fieldHeight, _model.fieldWidth);
                await DataAccess.SaveFile(dataObject, false);
                await DataAccess.SaveFile(dataObject, true);
            }
        }

        public static void Load(bool isTest = false)
        {
            DataAccess.PreLoad(isTest);
            return;
        }

        public static GameTrackerDataObject LoadTracker(int index)
        {
            if (!DataAccess.IsDataLoaded)
                return null;
            return DataAccess.LoadData(index);
        }


    }

    public struct ActionNode
    {
        public ActionType Action { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }
        public TileState[,] Table { get; private set; }
        public ActionNode(ActionType type, TileState[,] table = null, int row = -1, int col = -1)
        {
            Action = type;
            Row = type == ActionType.Step ? row : -1;
            Column = type == ActionType.Step ? col : -1;
            Table = table;
        }
    }
}
