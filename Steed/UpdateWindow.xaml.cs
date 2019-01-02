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
using System.Net;
using System.Diagnostics;
using System.IO;

namespace Steed
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WebClient fetcher = new WebClient();
            tbUpdates.Text = fetcher.DownloadString("http://steedservers.000webhostapp.com/steedbuild/updatelog.txt").ToString();
        }

        void Update()
        {
            WebClient fetcher = new WebClient();
            File.WriteAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\steed_data.txt", File.ReadAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\steed_data.txt").Replace(File.ReadAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\steed_data.txt"), fetcher.DownloadString("http://steedservers.000webhostapp.com/steedbuild/version.txt").ToString()));
            string[] settings = new string[] { Properties.Settings.Default.steamPath, Properties.Settings.Default.userDataPath };
            System.IO.File.WriteAllLines(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\settings_temp.txt", settings);
            Process.Start(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Updater.exe");
            Process.GetCurrentProcess().Kill();
        }

        private void grdMenuBarScrolling_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void brdMinimize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void brdClose_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void btnNoThanks_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
