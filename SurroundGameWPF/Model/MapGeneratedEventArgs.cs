using System;
using System.Collections.Generic;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.Model
{
    public class MapGeneratedEventArgs : EventArgs
    {
        public Dictionary<Players, PlayerPoints> PlayerPointDictionary { get; }

        public MapGeneratedEventArgs(Dictionary<Players, PlayerPoints> points)
        {
            PlayerPointDictionary = points;
        }
    }
    
    public class PlayerPoints
    {
        public int Points { get; }
        public int PointDifference { get; }
        public PlayerPoints(int points, int pointdiff = 0)
        {
            Points = points;
            PointDifference = pointdiff;
        }
    }
}
