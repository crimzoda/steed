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
using HtmlAgilityPack;
using System.ComponentModel;
using Microsoft.Win32;

namespace Steed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Movable window
            if (e.ChangedButton == MouseButton.Left && e.ClickCount != 2)
            {
                this.DragMove();
            }
            else if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                switch (WindowState)
                {
                    case WindowState.Maximized:
                        WindowState = WindowState.Normal;
                        grdMainWindow.Margin = new Thickness(0);
                        break;
                    case WindowState.Normal:
                        WindowState = WindowState.Maximized;
                        grdMainWindow.Margin = new Thickness(8);
                        break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\settings_temp.txt"))
            {
                int lineNum = 0;
                foreach (string line in File.ReadLines(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\settings_temp.txt"))
                {
                    lineNum++;
                    if (lineNum == 1)
                    {
                        Properties.Settings.Default.steamPath = line;
                        Properties.Settings.Default.Save();
                    }
                    if (lineNum == 2)
                    {
                        Properties.Settings.Default.userDataPath = line;
                        Properties.Settings.Default.Save();
                    }
                }
                File.Delete(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\settings_temp.txt");
            }

            Width = System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            Height = System.Windows.SystemParameters.PrimaryScreenHeight / 2;
            Left = (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - SystemParameters.PrimaryScreenWidth / 2.5;
            Top = (System.Windows.SystemParameters.PrimaryScreenHeight / 2) - SystemParameters.PrimaryScreenHeight / 2.5;
            
            CheckForUpdates();

            //Properties.Settings.Default.Reset();
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games");
            }
            if (!Directory.Exists((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games")))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games");
            }

            //brdBackground.Background = new ImageBrush(new BitmapImage(new Uri("https://steamcdn-a.akamaihd.net/steam/apps/223470/ss_13e460aa2915fb31bdbd723365e7965c9accf117.1920x1080.jpg")));
            //Creates new thread for steam game loading to save ui pressure on gpu and cpu, stops the window from freezing
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new ThreadStart(() =>
            {
                LoadSteamGames();
            }));
        }

        void CheckForUpdates()
        {
            WebClient fetcher = new WebClient();
            string version = "";
            int lineNum = 0;

            try
            {
                foreach (string line in File.ReadLines(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\steed_data.txt"))
                {
                    lineNum++;

                    if (lineNum == 1)
                    {
                        if (fetcher.DownloadString("http://steedservers.000webhostapp.com/steedbuild/version.txt").ToString() != line)
                        {
                            UpdateWindow updateWindow = new UpdateWindow();
                            updateWindow.Show();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                
            }
           
        }
        
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MessageBox.Show(e.UserState.ToString());
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            HtmlDocument steamdbDoc = new HtmlWeb().Load("http://steamdb.info/app/" + e.Argument);

            foreach (HtmlNode reviewsNode in steamdbDoc.DocumentNode.SelectNodes("//table[@class='table table-bordered table-fixed']//tr//td"))
            {
                if (reviewsNode != null)
                {
                    if (reviewsNode.InnerText != null)
                    {
                        if (reviewsNode.InnerText.EndsWith(".exe"))
                        {
                            worker.ReportProgress(0, reviewsNode.InnerText);
                        }
                    }
                }
            }
        }

        void LoadSteamGames()
        {
            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.ProcessName == "Steam")
                {
                    Properties.Settings.Default.userDataPath = System.IO.Path.GetDirectoryName(proc.MainModule.FileName) + "\\userdata";
                    Properties.Settings.Default.Save();
                }
            }

            try
            {
                foreach (string userfolder in Directory.GetDirectories(Properties.Settings.Default.userDataPath))
                {
                    foreach (string line in File.ReadLines(userfolder + "\\config\\localconfig.vdf"))
                    {
                        if (line.Contains("\"PersonaName\""))
                        {
                            string[] username = (line.Replace("\"PersonaName\"", "")).Split('"');
                            lblTitle.Content = "Steed - Game Library (" + username[1] + ")";
                            ttAvatar.Content = username[1].ToString();
                        }
                    }
                }
            }
            catch
            {

            }

            try
            {
                foreach (string userfolder in Directory.GetDirectories(Properties.Settings.Default.userDataPath))
                {
                    int lineNum = 0;
                    int gameNum = 0;
                    foreach (string line in File.ReadLines(userfolder + "\\config\\localconfig.vdf"))
                    {
                        lineNum++;

                        if (line.Contains("\"PersonaName\""))
                        {
                            gameNum = lineNum;
                        }    
                        if (lineNum == gameNum + 8)
                        {
                            if (line.Contains("\"avatar\""))
                            {
                                string[] username = (line.Replace("\"avatar\"", "")).Split('"');
                                imgProfilePic.Source = new BitmapImage(new Uri("http://cdn.edgecast.steamstatic.com/steamcommunity/public/images/avatars/ed/" + username[1].ToString() + "_full.jpg%22"));
                            }
                            else
                            {
                                imgProfilePic.Source = new BitmapImage(new Uri("https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/"));
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            if (Properties.Settings.Default.steamPath == "" && Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games").Length == 0)
            {
                TextBlock tblEmpty = new TextBlock();
                tblEmpty.Tag = "notice";
                tblEmpty.Text = "No games :( Start with connecting your Steam library or adding a game manually. (Settings)";
                tblEmpty.TextWrapping = TextWrapping.Wrap;
                tblEmpty.TextAlignment = TextAlignment.Center;
                tblEmpty.VerticalAlignment = VerticalAlignment.Center;
                tblEmpty.Margin = new Thickness(0, 100, 0, 0);
                tblEmpty.FontSize = 14;
                tblEmpty.Foreground = new SolidColorBrush(Colors.LightGray);
                tblEmpty.Width = 300;
                Image imgEmpty = new Image();
                imgEmpty.Tag = "notice";
                imgEmpty.Source = new BitmapImage(new Uri(@"\UI\empty-box.png", UriKind.Relative));
                imgEmpty.HorizontalAlignment = HorizontalAlignment.Center;
                imgEmpty.VerticalAlignment = VerticalAlignment.Center;
                imgEmpty.Stretch = Stretch.None;
                imgEmpty.Opacity = 0.5;
                imgEmpty.Margin = new Thickness(0, -90, 0, 0);
                grdMainWindow.Children.Add(tblEmpty);
                grdMainWindow.Children.Add(imgEmpty);
            }
            else
            {
                for (int i = 0; i < grdMainWindow.Children.Count; i++)
                {
                    if (grdMainWindow.Children[i].GetType() == typeof(TextBlock))
                    {
                        if (((TextBlock)grdMainWindow.Children[i]).Tag.ToString() == "notice")
                        {
                            grdMainWindow.Children.Remove(grdMainWindow.Children[i]);
                        }
                    }
                    if (grdMainWindow.Children[i].GetType() == typeof(Image))
                    {
                        if (((Image)grdMainWindow.Children[i]).Tag.ToString() == "notice")
                        {
                            grdMainWindow.Children.Remove(grdMainWindow.Children[i]);
                        }
                    }
                }

            }
            wpContents.Children.Clear();

            //Gets the appid and title of the steam game
            try
            {
                foreach (string steamGame in Directory.GetFiles(Properties.Settings.Default.steamPath + "\\steamapps"))
                {
                    if (System.IO.Path.GetFileName(steamGame).Contains(".acf"))
                    {
                        string gameTitle = "";
                        string appid = System.IO.Path.GetFileNameWithoutExtension(steamGame).Replace("appmanifest_", "");


                        int lineNumber = 0;
                        foreach (string property in File.ReadLines(steamGame))
                        {
                            lineNumber++;
                            if (lineNumber == 5)
                            {
                                foreach (string gameName in property.Split('"'))
                                {
                                    if (gameName != "name" && gameName != " " && gameName != "")
                                    {
                                        gameTitle = gameName;
                                    }
                                }
                            }
                        }
                        SteamGameItem SteamGame = new SteamGameItem();

                        //Don't know how this fucking stops invalid game urls but it just works
                        WebClient wc = new WebClient();
                        wc.DownloadString("http://cdn.akamai.steamstatic.com/steam/apps/" + appid + "/header.jpg");

                        wpContents.Children.Add(SteamGame.Load(appid, gameTitle));
                        //}
                    }
                }
            }
            catch
            {
                //Do nothing
            }

            foreach (string game in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games"))
            {
                GameItem gameItem = new GameItem();

                string gameDir = "";
                string gameHeader = "";
                string gameName = "";
                string gameEmulator = "";

                int lineNum = 0;
                foreach (string line in File.ReadLines(game))
                {
                    lineNum++;

                    switch (lineNum)
                    {
                        case 1:
                            gameDir = line;
                            break;
                        case 2:
                            gameName = line;
                            break;
                        case 3:
                            gameEmulator = line;
                            break;
                        case 4:
                            gameHeader = line;
                            break;
                    }
                }

                wpContents.Children.Add(gameItem.Load(gameDir, gameName, gameHeader, gameEmulator, game));
            }
            int games = 0;
            foreach (Grid grdGame in wpContents.Children)
            {
                games++;
                lblStatus.Content = games.ToString() + " Games";
            }
        }

        private void imgRefresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new ThreadStart(() =>
            {
                LoadSteamGames();
            }));
        }

        private void brdClose_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void brdMinimize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void imgAddGame_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddGameWindow addGameWindow = new AddGameWindow();
            addGameWindow.Closing += AddGameWindow_Closing;
            addGameWindow.Show();
        }

        private void AddGameWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var window = (AddGameWindow)sender;

            if (window.appDir != null && window.appHeader != null && window.appName != null)
            {
                if (window.appHeader == "")
                {
                    window.appHeader = "none";
                }

                int gameCount = 0;

                foreach (string game in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games"))
                {
                    while (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games\\" + gameCount.ToString() + ".txt"))
                    {
                        gameCount++;
                    }
                }
                

                File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games\\" + gameCount.ToString() + ".txt").Close();
                using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games\\" + gameCount.ToString() + ".txt"))
                {
                    sw.WriteLine(window.appDir);
                    sw.WriteLine(window.appName);
                    sw.WriteLine(window.appEmulator);
                    sw.WriteLine(window.appHeader);
                    sw.WriteLine("null");
                    sw.Dispose();
                    sw.Close();
                }

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new ThreadStart(() =>
                {
                    LoadSteamGames();
                }));
            }
        }

        private void imgConfigure_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Closing += SettingsWindow_Closing;
            settingsWindow.Show();
        }

        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new ThreadStart(() =>
            {
                LoadSteamGames();
            }));
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text != "")
            {
                foreach (UIElement element in wpContents.Children)
                {
                    element.Opacity = 1;
                }

                for (int i = 0; i < wpContents.Children.Count; i++)
                {
                    if (wpContents.Children[i].GetType() == typeof(Grid))
                    {
                        var grid = (Grid)wpContents.Children[i];

                        if (grid.Tag != null)
                        {
                            for (int a = 0; a < grid.Children.Count; a++)
                            {
                                if (grid.Children[a].GetType() == typeof(Border))
                                {
                                    if (((Border)grid.Children[a]).Tag != null)
                                    {
                                        if (((Border)grid.Children[a]).Tag.ToString().StartsWith(tbSearch.Text, StringComparison.OrdinalIgnoreCase))
                                        {
                                            for (int x = 0; x < wpContents.Children.Count; x++)
                                            {
                                                if (wpContents.Children[x] != grid)
                                                {
                                                    wpContents.Children[x].Opacity = 0.4;
                                                    grid.BringIntoView();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (UIElement element in wpContents.Children)
                {
                    element.Opacity = 1;
                }
            }
            
        }

        private void imgProfilePic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void miSettings_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://settings");
        }

        private void miFriends_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://friends");
        }

        private void miOffline_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://friends/status/offline");
        }

        private void miLookingToTrade_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://friends/status/trade");
        }

        private void miLookingToPlay_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://friends/status/play");
        }

        private void miBusy_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://friends/status/busy");
        }

        private void miAway_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://friends/status/away");
        }

        private void miOnline_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://friends/status/online");
        }

        private void brdMaximize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    grdMainWindow.Margin = new Thickness(0);
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    grdMainWindow.Margin = new Thickness(8);
                    break;
            }
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    grdMainWindow.Margin = new Thickness(0);
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    grdMainWindow.Margin = new Thickness(8);
                    break;
            }
        }
    }
}
