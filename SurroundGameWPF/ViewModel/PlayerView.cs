using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.ViewModel
{
    public class PlayerView : ViewModelBase
    {
        Persistence.Players _name;
        int _points;
        int _pointsDifference;
        public Persistence.Players Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Points
        {
            get
            {
                return _points;
            }
            set
            {
                if(value != _points)
                {
                    _points = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PointsDifference
        {
            get
            {
                return _pointsDifference;
            }
            set
            {
                if(value!=_pointsDifference)
                {
                    _pointsDifference = value;
                    OnPropertyChanged();
                }
            }
        }

        public PlayerView(Persistence.Players name, int points, int pointsDiff)
        {
            Name = name;
            Points = points;
            PointsDifference = pointsDiff;
        }

    }
}
