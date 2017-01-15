using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurroundGameWPF.ViewModel
{
        public abstract class ViewModelBase : INotifyPropertyChanged
        {
            /// <summary>
            /// Tulajdonság változásának eseménye.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Nézetmodell ősosztály példányosítása.
            /// </summary>
            protected ViewModelBase() { }

            /// <summary>
            /// Tulajdonság változása ellenőrzéssel.
            /// </summary>
            /// <param name="propertyName">Tulajdonság neve.</param>
            protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = null)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    
}
