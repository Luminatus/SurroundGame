using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurroundGameWPF.Model;
using SurroundGameWPF.View;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private MainModel _model;
        private GameView _gameView;
        private GameViewModel _gameViewModel;
        private GameModel _gameModel;
        
        public event EventHandler RequestHide;
        public event EventHandler RequestShow;

        private int _rowCount;
        private int _columnCount;
        private int _playerCount;
        private Visibility _visibility;
        public Visibility WindowVisibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                if(_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RowCount
        {
            get
            {
                return _rowCount.ToString();
            }
            set
            {
                if (value != _rowCount.ToString())
                {
                    int temp;
                    if (Int32.TryParse(value, out temp) && temp >= 6 && temp <= 30)
                    {
                        _rowCount = temp;
                    }
                    else
                        _rowCount = temp < 6 ? 6 : 30;

                    if (_isEqualRowColumn)
                        ColumnCount = RowCount;
                    OnPropertyChanged();
                }
            }
        }

        public string ColumnCount
        {
            get
            {
                return _columnCount.ToString();
            }
            set
            {
                if (value != _columnCount.ToString())
                {
                    int temp;
                    if (Int32.TryParse(value, out temp) && temp >= 6 && temp <= 30)
                    {
                        _columnCount = temp;
                    }
                    else
                        _columnCount = temp < 6 ? 6 : 30;
                    OnPropertyChanged();
                }
            }
        }

        public string PlayerCount
        {
            get
            {
                return _playerCount.ToString();
            }
            set
            {
                if (value != _playerCount.ToString())
                {
                    int temp;
                    if (Int32.TryParse(value, out temp) && temp >= 1 && temp <= 6)
                    {
                        _playerCount = temp;
                    }
                    else
                    {
                        _playerCount = temp < 1 ? 1 : 6;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isEqualRowColumn;
        public bool IsEqualRowColumn
        {
            get
            {
                return _isEqualRowColumn;
            }
            set
            {
                if(value!=_isEqualRowColumn)
                {
                    _isEqualRowColumn = value;
                    if(_isEqualRowColumn)
                        ColumnCount = RowCount;
                    OnPropertyChanged();
                }
            }
        }

        private DelegateCommand _buttonCommand;
        public DelegateCommand ButtonCommand
        {
            get
            {
                return _buttonCommand ?? (_buttonCommand = new DelegateCommand((param) => ButtonClicked((string)param)));
            }
        }        
        
        
        public void ButtonClicked(string param)
        {
            switch(param)
            {
                case "NewGame": NewGameClicked(); break;
                case "LoadGame": LoadGameClicked(); break;
                default: break;
            }
        }

        public void NewGameClicked()
        {
    /*        if (_playerCount != 2)
            {
                MessageBox.Show("This game currently only supports 2-player matches, but thank you for showing interest in future features!", PlayerCount + "-player mode not supported", MessageBoxButton.OK);
                return;
            }           */ 
            if (_model.ValidateNewGame(_rowCount, _columnCount))
            {
                _gameModel = new GameModel(new SurroundGameFileDataAccess());
                _gameViewModel = new GameViewModel(_gameModel);
                _gameViewModel.RequestClose += new EventHandler((o, a) => { _gameView.Close(); });
                _gameViewModel.NewGame(_rowCount, _columnCount, _playerCount);
                if (_gameViewModel.IsGameReady)
                {
                    _gameView = new GameView();
                    _gameView.DataContext = _gameViewModel;
                    _gameView.Closed += new EventHandler((s, e) => { RequestShow(this, EventArgs.Empty); });
                    _gameView.RotateClicked += new EventHandler((s, e) => { _gameViewModel.Rotate(); });

                    _gameView.Show();
                    RequestHide(this, EventArgs.Empty);
                }
            }
        }

        public async void LoadGameClicked()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = false;
            dialog.ShowDialog();
            string fileName = dialog.FileName;
            if (fileName != String.Empty)
            {
                _gameModel = new GameModel(new SurroundGameFileDataAccess());
                _gameViewModel = new GameViewModel(_gameModel);
                await _gameViewModel.LoadGame(fileName);
                if (_gameViewModel.IsGameReady)
                {
                    _gameView = new GameView();
                    _gameView.DataContext = _gameViewModel;
                    _gameView.Closed += new EventHandler((s, e) => { RequestShow(this, EventArgs.Empty); });

                    _gameView.Show();
                    RequestHide(this, EventArgs.Empty);
                }
                else
                {
                    _gameView.Close();
                    MessageBox.Show("Loading game has failed - the save file might be corrupted", "Load error");
                }
            }
        }

        public MainViewModel()
        {
            _model = new MainModel();
            WindowVisibility = Visibility.Visible;
            RowCount = "6";
            ColumnCount = "6";
            PlayerCount = "2";
        }
    }
}
