using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.ViewModel
{
    public class SurroundGameField : ViewModelBase
    {
        public event EventHandler<FieldClickedEventArgs> FieldClicked;
        public int Row { get; }
        public int Column { get; }

        public Brush _color;
        public Brush Color
        {
            get
            {
                return _color;
            }
            set
            {
                if(_color!=value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }

        }

        public DelegateCommand FieldCommand { get; }

        public SurroundGameField(int row, int col)
        {
            Row = row;
            Column = col;
            Color = Brushes.White;
            FieldCommand = new DelegateCommand(param =>
            {
                FieldClicked(this, new FieldClickedEventArgs(Row, Column));
            });
        }
    }
}
