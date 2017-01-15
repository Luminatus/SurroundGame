using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.ViewModel
{
    public static class PlayerColors
    {
        static Dictionary<TileState, Color> _colorDict = new Dictionary<Persistence.TileState, Color>
        {
                {TileState.Unoccupied,Colors.White},
                {TileState.RedWall,Color.FromArgb(255, 200, 0, 0)},
                {TileState.RedField,Color.FromArgb(255, 255, 80, 80)},
                {TileState.BlueWall,Color.FromArgb(255, 0, 0, 200) },
                {TileState.BlueField,Color.FromArgb(255, 90, 90, 255)},
                {TileState.GreenWall,Color.FromArgb(255, 0, 200, 0) },
                {TileState.GreenField,Color.FromArgb(255, 150, 250, 150) },
                {TileState.YellowWall,Color.FromArgb(255, 240, 240, 0) },
                {TileState.YellowField,Color.FromArgb(255, 255, 255, 150) },
                {TileState.PurpleWall,Color.FromArgb(255, 200, 0, 200) },
                {TileState.PurpleField,Color.FromArgb(255, 250, 150, 250) },
                {TileState.OrangeWall,Color.FromArgb(255, 255, 150, 0) },
                {TileState.OrangeField,Color.FromArgb(255, 255, 190, 70) },
        };

        static Dictionary<Players, Color> _playerColorDict = new Dictionary<Players, Color>
        {
                {Players.Red, Color.FromArgb(255,200,0,0) },
                {Players.Blue,Color.FromArgb(255, 0, 0, 200) },
                {Players.Green,Color.FromArgb(255, 0, 200, 0) },
                {Players.Yellow,Color.FromArgb(255, 230, 230, 0) },
                {Players.Purple,Color.FromArgb(255, 200, 0, 200) },
                {Players.Orange,Color.FromArgb(255, 255, 150, 0) },
                {Players.None,Color.FromArgb(255, 0, 0, 0) },
        };

        static Dictionary<TileState, SolidColorBrush> _brushDict = new Dictionary<TileState, SolidColorBrush>();

        public static SolidColorBrush GetColor(TileState state, byte alpha = 255)
        {
            if (_brushDict.ContainsKey(state))
                return _brushDict[state];
            Color retColor = _colorDict[state];
            retColor.A = alpha;
            SolidColorBrush retBrush = new SolidColorBrush(retColor);
            _brushDict.Add(state, retBrush);
            return retBrush;
        }

        public static SolidColorBrush GetColor(Players player)
        {
            return new SolidColorBrush(_playerColorDict[player]);
        }

        public static SolidColorBrush GetMixedColor(TileState baseColorState, TileState foreColorState, double ratio)
        {
            Color retColor = new Color();
            Color baseColor = _colorDict[baseColorState];
            Color foreColor = _colorDict[foreColorState];
            retColor.A = (byte)(baseColor.A * (1 - ratio) + foreColor.A * ratio);
            retColor.R = (byte)(baseColor.R * (1 - ratio) + foreColor.R * ratio);
            retColor.G = (byte)(baseColor.G * (1 - ratio) + foreColor.G * ratio);
            retColor.B = (byte)(baseColor.B * (1 - ratio) + foreColor.B * ratio);
            return new SolidColorBrush(retColor);
        }
    }
}
