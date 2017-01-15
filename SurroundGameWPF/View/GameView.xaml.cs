using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SurroundGameWPF.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class GameView : Window
    {
        public event EventHandler  RotateClicked;
        public GameView()
        {
            InitializeComponent();
        }

        private void GameWindow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RotateClicked(this, EventArgs.Empty);
        }

        private void GameWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void ResizeHorizontal(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
