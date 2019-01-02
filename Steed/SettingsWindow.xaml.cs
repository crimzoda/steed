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

namespace Steed
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Movable window
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void brdClose_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void brdMinimize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainFrame.Content = new GeneralSettingsPage();
        }

        private void lblHelpAbout_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainFrame.Content = new HelpPage();
        }

        private void lblGeneral_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainFrame.Content = new GeneralSettingsPage();
        }

        private void lblSupportMe_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainFrame.Content = new SupportMePage();
        }
    }
}
