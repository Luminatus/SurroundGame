using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SurroundGameWPF.View;
using SurroundGameWPF.ViewModel;

namespace SurroundGameWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainViewModel viewModel = new MainViewModel(); // nézetmodell létrehozása

            MainView window = new MainView(); // nézet létrehozása

            window.DataContext = viewModel; // nézetmodell és modell társítása

            viewModel.RequestHide += new EventHandler((s, a) => { window.Hide(); });
            viewModel.RequestShow += new EventHandler((s, a) => { window.Show(); });

            window.Show();
        }
    }
}
