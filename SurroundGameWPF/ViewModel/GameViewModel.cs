using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurroundGameWPF.Model;
using SurroundGameWPF.Persistence;
using SurroundGameWPF.Extensions;

namespace SurroundGameWPF.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        private  struct ClickInfo
        {
            public int Row { get; }
            public int Column { get; }
            public bool IsBrickHorizontal { get; }
            public ClickInfo(int row, int col, bool isHorizontal)
            {
                Row = row;
                Column = col;
                IsBrickHorizontal = isHorizontal;
            }

            public static readonly ClickInfo Empty = new ClickInfo(-1, -1, true);
            
        }
        ClickInfo LastClick;



        private GameModel _model;
        public event EventHandler RequestClose;        
        public ObservableCollection<SurroundGameField> Fields { get; set; }
        public ObservableCollection<PlayerView> Players { get; set; }

        private int _buttonSize;
        private int fieldWidth;
        private int fieldHeight;
        private int windowWidth;
        int windowHeight;
        int minimumTileSize = 20;
        int FieldStartX = 130;
        int FieldStartY = 60;
        private DelegateCommand _buttonPress;
        public DelegateCommand ButtonPress
        {
            get
            {
                return _buttonPress ?? (_buttonPress = new DelegateCommand((param) => { ButtonPressed((string)param); }));
            }
        }
        public int ButtonSize
        {
            get
            {
                return _buttonSize;
            }
            set
            {
                if (_buttonSize != value)
                {
                    _buttonSize = value;
                    OnPropertyChanged();
                }
            }
        }
        public int FieldHeight
        {
            get
            {
                return fieldHeight;
            }
            set
            {
                if (fieldHeight != value)
                {
                    fieldHeight = value;
                    OnPropertyChanged();
                }
            }
        }
        public int FieldWidth
        {
            get
            {
                return fieldWidth;
            }
            set
            {
                if (fieldWidth != value)
                {
                    fieldWidth = value;
                    OnPropertyChanged();
                }
            }

        }       
        public double WindowWidth
        {
            get { return windowWidth; }
            set
            {
                if ((int)value != windowWidth)
                {
                    windowWidth = (int)value;
                    OnPropertyChanged();
                }
            }
        }
        public double WindowHeight
        {
            get { return windowHeight; }
            set
            {
                if ((int)value != windowHeight)
                {
                    windowHeight = (int)value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsBrickHorizontal
        {
            get
            {
                return _model != null && !_model.IsGameOver ? _model.isBrickHorizontal : true;
            }
        }
        public SolidColorBrush ActiveColor
        {
            get
            {
                try
                {
                    return PlayerColors.GetColor(_model.CurrentPlayer.Name);
                }
                catch
                {
                    return Brushes.Black;
                }
            }
        }
        public SolidColorBrush ActiveFieldColor
        {
            get
            {
                try
                {
                    return PlayerColors.GetColor(_model.CurrentPlayer.Field);
                }
                catch
                {
                    return Brushes.Black;
                }
            }
        }

        double ScreenWidth = SystemParameters.WorkArea.Width;
        double ScreenHeight = SystemParameters.WorkArea.Height;

        public const int minimumHeight = 430;
        public int minimumWidth { get { return FieldStartX + 60 + (minimumTileSize * _model.fieldWidth); } }
        public int maximumWidth { get { return (int)ScreenWidth; } }
        public int maximumHeight { get { return (int)ScreenHeight; } }
        public bool IsGameReady { get; private set; }

        public GameViewModel(GameModel model)
        {
            _model = model;
            Fields = new ObservableCollection<SurroundGameField>();
            Players = new ObservableCollection<PlayerView>();
        }

        private void SetSizeParameters()
        {
            int calculatedTileSize = Math.Min((minimumHeight - FieldStartY - 60) / _model.fieldHeight, (maximumWidth - FieldStartX - 60) / _model.fieldWidth);
            if (calculatedTileSize < minimumTileSize)
            {
                ButtonSize = minimumTileSize;
            }
            else
            {
                ButtonSize = calculatedTileSize;
            }

           FieldWidth = ButtonSize * _model.fieldWidth;
            FieldHeight = ButtonSize * _model.fieldHeight;
        }

        public void Rotate()
        {
            _model.RotateBrick();
            OnPropertyChanged("IsBrickHorizontal");
            if (!LastClick.Equals(ClickInfo.Empty))
            {
                _model.CheckPlacement(LastClick.Row, LastClick.Column);
                LastClick = new ClickInfo(LastClick.Row, LastClick.Column, IsBrickHorizontal);
            }
        }
        private void ButtonPressed(string param)
        {
            switch (param)
            {
                case "ROTATE":
                    Rotate();
                    break;
                case "EXIT":
                    Close();
                    break;
                case "CONFIRM":
                    MakeStep();
                    break;
                default: break;
            }

        }

        public void Close()
        {
            switch (PromptExit())
            {
                case MessageBoxResult.Yes: SaveGame(); _model.EndGame(); OnRequestClose(); break;
                case MessageBoxResult.No: _model.EndGame(); OnRequestClose(); break;
                default: break;
            }
        }

        private void OnRequestClose()
        {
            if(RequestClose!=null)
            {
                RequestClose(null, EventArgs.Empty);
            }
        }

        private MessageBoxResult PromptExit()
        {

            string messageText = "Do you want to save your game before quitting?";
            string messageCaption = "Quit game";
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            return MessageBox.Show(messageText, messageCaption, buttons);
        }

        public void NewGame(int row, int col, int players)
        {
            try
            {
                _model.NewGame(row, col, players);
            }
            catch (SurroundGameModelException e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            InitializeGameView();
            IsGameReady = true;
        }

        private void InitializeGameView()
        {
            for (int rows = 0; rows < _model.fieldHeight; rows++)
            {
                for (int cols = 0; cols < _model.fieldWidth; cols++)
                {
                    Fields.Add(new SurroundGameField(rows, cols));
                    Fields[cols + rows * _model.fieldWidth].FieldClicked += new EventHandler<FieldClickedEventArgs>(GameFieldClicked);
                }
            }
            ButtonSize = 20;
                     SetSizeParameters();
            LastClick = ClickInfo.Empty;
            
        _model.GameEnded += new EventHandler<GameEndedEventArgs>((o, e) => {
                if (e.IsDraw)
                    GameEndedWithDraw(e);
                else
                    GameEndedWithWin(e);
            });
            _model.MapGenerated += new EventHandler<MapGeneratedEventArgs>((o, e) => {
                Players.Clear();
                foreach(KeyValuePair<Players,PlayerPoints> points in e.PlayerPointDictionary)
                {
                    Players.Add(new PlayerView(points.Key,points.Value.Points, points.Value.PointDifference));
                }
                Refresh();
            });


            foreach (Players player in _model.PlayerNames)
            {
                Players.Add(new PlayerView(player, 0, 0));
            }
            OnPropertyChanged("ActiveColor");
            OnPropertyChanged("ActiveFieldColor");
            Refresh();
        }

        private void GameEndedWithDraw(GameEndedEventArgs e)
        {

            string winnerString = "Game has ended in a draw between ";
            LinkedListNode<Players> player = e.Players.First;
            do
            {
                if (player == player.List.Last)
                    winnerString += " and " + player.Value + ".";
                else
                {
                    winnerString += player.Value;
                    if (player.Next != player.List.Last)
                        winnerString += ", ";
                }
                player = player.NextOrFirst();
            } while (player != player.List.First);
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBox.Show(winnerString, "It's a draw!", button);
            OnRequestClose();
        }

        private void GameEndedWithWin(GameEndedEventArgs e)
        {
            string winnerString = e.Players.First() + " player has won the game!";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBox.Show(winnerString, "Game won!", button);
            OnRequestClose();
        }

        public async Task LoadGame(string fileName)
        {
            try
            {
                await _model.LoadGame(fileName);
            }
            catch (SurroundGameDataException e)
            {
                IsGameReady = false;
                return;
            }
            InitializeGameView();
            IsGameReady = true;
        }

        public async void SaveGame()
        {
            bool ToRetry = false;
            do
            {
                ToRetry = false;
                string fileName = String.Empty;
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.DefaultExt = "sgs";
                dialog.Filter = "Surround Game save filed (.sgs)| *.sgs";
                dialog.OverwritePrompt = true;
                bool? dialogResult = dialog.ShowDialog();

                if (!dialogResult.HasValue || !dialogResult.Value)
                    return;

                fileName = dialog.FileName;
                try
                {
                    await _model.SaveGame(fileName);
                }
                catch (SurroundGameDataException e)
                {
                    string errorMessage = "Error saving your game - file path might not exist! \n Do you want to save to another location?";
                    string messageCaption = "Save error";
                    MessageBoxButton buttons = MessageBoxButton.YesNo;
                    if (MessageBox.Show(errorMessage, messageCaption, buttons) == MessageBoxResult.Yes)
                    {
                        ToRetry = true;
                    }
                }
            } while (ToRetry);
        }


        public void GameFieldClicked(object sender, FieldClickedEventArgs e)
        {
            OnPropertyChanged("WindowWidth");
            OnPropertyChanged("WindowHeight");
            if (LastClick.Column == e.Columm && LastClick.Row == e.Row && LastClick.IsBrickHorizontal == IsBrickHorizontal)
            {
                MakeStep();
            }
            else
            {
                LastClick = new ClickInfo(e.Row, e.Columm, IsBrickHorizontal);
                _model.CheckPlacement(e.Row, e.Columm);
            }
            Refresh();
        }

        private void MakeStep()
        {
            _model.MakeStep();
            OnPropertyChanged("ActiveColor");
            OnPropertyChanged("ActiveFieldColor");
            OnPropertyChanged("IsBrickHorizontal");
        }
        





        private void Refresh()
        {
            for (int row = 0; row < _model.fieldHeight; row++)
            {
                for (int col = 0; col < _model.fieldWidth; col++)
                {
                    TileState state = _model.GetState(row, col);
                    TileState previewState = _model.GetState(row, col, true);
                    SolidColorBrush color = PlayerColors.GetColor(state); // getStateColor(state);

                    if (state != previewState)
                    {
                        // getStateColor(previewState, 180);
                        // MixColors(color, previewcolor, (float)previewcolor.Color.A / 255);
                        color = PlayerColors.GetMixedColor(state, previewState, 0.55); 
                    }
                    if (Fields[col + _model.fieldWidth * row].Color != color)
                        Fields[col + _model.fieldWidth * row].Color = color;
                }
            }
            OnPropertyChanged();
        }

        private SolidColorBrush MixColors(SolidColorBrush brush1, SolidColorBrush brush2, double opacity)
        {
            SolidColorBrush ret = new SolidColorBrush();
            byte colorA = (byte)(brush1.Color.A * (1 - opacity) + brush2.Color.A * opacity);
            byte colorR = (byte)(brush1.Color.R * (1 - opacity) + brush2.Color.R * opacity);
            byte colorG = (byte)(brush1.Color.G * (1 - opacity) + brush2.Color.G * opacity);
            byte colorB = (byte)(brush1.Color.B * (1 - opacity) + brush2.Color.B * opacity);
            ret.Color = Color.FromArgb(colorA, colorR, colorG, colorB);
            return ret;
        }

        private SolidColorBrush getStateColor(TileState state, byte alpha = 255)
        {
            switch (state)
            {
                case TileState.Unoccupied: return Brushes.White;
                case TileState.RedWall: return new SolidColorBrush(Color.FromArgb(alpha, 200, 0, 0));
                case TileState.RedField: return new SolidColorBrush(Color.FromArgb(alpha, 255, 80, 80));
                case TileState.BlueWall: return new SolidColorBrush(Color.FromArgb(alpha, 0, 0, 200));
                case TileState.BlueField: return new SolidColorBrush(Color.FromArgb(alpha, 90, 90, 255));
                default: return Brushes.Black;
            }
        }
    }
}
