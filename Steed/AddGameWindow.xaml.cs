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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Windows.Media.Effects;
using System.Web;
using System.Net;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Steed
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {
        public string appDir, appName, appHeader, appEmulator;

        public AddGameWindow()
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //brdBackground.Background = new ImageBrush(new BitmapImage(new Uri("https://steamcdn-a.akamaihd.net/steam/apps/223470/ss_13e460aa2915fb31bdbd723365e7965c9accf117.1920x1080.jpg")));
            //Creates new thread for steam game loading to save ui pressure on gpu and cpu, stops the window from freezing
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new ThreadStart(() =>
            {
                LoadInstalled();
            }));
        }

        void LoadInstalled()
        {
            //spContents.Children.Clear();

            
        }
       

        private void brdClose_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void brdMinimize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            appDir = tbLocation.Text;
            appName = tbName.Text;
            appHeader = tbHeader.Text;
            appEmulator = tbEmulator.Text;
        }
    }
}
