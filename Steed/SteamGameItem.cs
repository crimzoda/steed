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
using System.ComponentModel;
using HtmlAgilityPack;

namespace Steed
{
    class SteamGameItem
    {
        Grid grdGame = new Grid();
        DropShadowEffect dropShadowEffect = new DropShadowEffect();
        string description = "";
        Grid grdPart1 = new Grid();
        Grid grdPart2 = new Grid();
        string globalAppId = "";
        BackgroundWorker worker = new BackgroundWorker();

        public Grid Load(string appId, string appName)
        {
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games\\" + appId + ".txt"))
            {
                File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games\\" + appId + ".txt").Close();
                StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games\\" + appId + ".txt");
                writer.WriteLine("null");
                writer.WriteLine("null");
                writer.Close();
                writer.Dispose();
            }
            globalAppId = appId;
            Border brdGame = new Border();
            Button btnLaunch = new Button();
        
            //grdGame properties
            grdGame.Tag = appId;
            grdGame.Margin = new Thickness(7);
            grdGame.HorizontalAlignment = HorizontalAlignment.Center;
            brdGame.VerticalAlignment = VerticalAlignment.Top;

            //brdGame properties
            brdGame.CornerRadius = new CornerRadius(4);
            brdGame.Tag = appName;
            brdGame.MouseEnter += brdGame_MouseEnter;
            brdGame.MouseLeave += brdGame_MouseLeave;
            brdGame.MouseLeftButtonUp += BrdGame_MouseLeftButtonUp;
            brdGame.Cursor = Cursors.Hand;
            brdGame.Width = 230;
            brdGame.Height = 107.5;
            brdGame.Background = new ImageBrush(new BitmapImage(new Uri("http://cdn.akamai.steamstatic.com/steam/apps/" + appId + "/header.jpg")));
            ContextMenu cmGameMenu = new ContextMenu();
            cmGameMenu.Style = (Style)Application.Current.Resources["ContextMenuStyle1"];
            MenuItem miEditGame = new MenuItem();
            miEditGame.Header = "Edit Game";
            miEditGame.Style = (Style)Application.Current.Resources["MenuItemStyle1"];
            MenuItem miUninstallGame = new MenuItem();
            miUninstallGame.Header = "Uninstall";
            miUninstallGame.Style = (Style)Application.Current.Resources["MenuItemStyle1"];
            miUninstallGame.Click += MiUninstallGame_Click;
            miEditGame.Click += MiEditGame_Click;
            cmGameMenu.Items.Add(miEditGame);
            cmGameMenu.Items.Add(miUninstallGame);
            brdGame.ContextMenu = cmGameMenu;

            //brdGameDetails properties
            Border brdGameDetails = new Border();
            brdGameDetails.VerticalAlignment = VerticalAlignment.Bottom;
            brdGameDetails.Height = 50;
            brdGameDetails.Background = new SolidColorBrush(Colors.Black);
            brdGameDetails.Opacity = 0.8;
            brdGameDetails.CornerRadius = new CornerRadius(0, 0, 4, 4);
            brdGameDetails.Margin = new Thickness(0, 0, 0, -0.5);
            brdGame.Child = brdGameDetails;
            brdGame.Child.Visibility = Visibility.Collapsed;

            //Game name label
            TextBlock tblGameTitle = new TextBlock();
            tblGameTitle.Text = appName;
            tblGameTitle.TextTrimming = TextTrimming.CharacterEllipsis;
            tblGameTitle.Foreground = new SolidColorBrush(Colors.White);
            tblGameTitle.VerticalAlignment = VerticalAlignment.Center;
            tblGameTitle.HorizontalAlignment = HorizontalAlignment.Left;
            tblGameTitle.MaxWidth = 145;
            tblGameTitle.Margin = new Thickness(8, 4, 4, 4);
            tblGameTitle.FontSize = 14;

            btnLaunch.Style = (Style)Application.Current.Resources["ButtonStyle1"];
            btnLaunch.Tag = appId;
            btnLaunch.MouseEnter += BtnLaunch_MouseEnter;
            btnLaunch.MouseLeave += BtnLaunch_MouseLeave;
            btnLaunch.Click += btnLaunch_Click;
            btnLaunch.Width = 90;
            btnLaunch.Height = 30;
            btnLaunch.Content = "Launch";
            btnLaunch.HorizontalAlignment = HorizontalAlignment.Right;
            btnLaunch.Margin = new Thickness(4);

            Border brdSteam = new Border();
            brdSteam.Width = 24;
            brdSteam.Height = 24;
            brdSteam.Cursor = Cursors.Hand;
            brdSteam.MouseLeftButtonUp += BrdSteam_MouseLeftButtonUp;
            brdSteam.Background = new ImageBrush(new BitmapImage(new Uri("https://i.imgur.com/eq3jnza.png")));
            brdSteam.CornerRadius = new CornerRadius(4);
            brdSteam.VerticalAlignment = VerticalAlignment.Top;
            brdSteam.HorizontalAlignment = HorizontalAlignment.Left;
            brdSteam.Opacity = 0.7;

            Grid grdGameDetails = new Grid();
            grdGameDetails.Children.Add(tblGameTitle);
            grdGameDetails.Children.Add(btnLaunch);

            brdGameDetails.Child = grdGameDetails;
            grdGame.Children.Add(brdGame);
            grdGame.Children.Add(brdSteam);

            worker.DoWork += Worker_DoWork;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += Worker_ProgressChanged;

            return grdGame;
        }

        private void MiUninstallGame_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://uninstall/" + globalAppId);
        }

        private void MiEditGame_Click(object sender, RoutedEventArgs e)
        {
            EditGameWindow editGameWindow = new EditGameWindow();
            editGameWindow.appId = globalAppId;
            editGameWindow.type = "steam";
            editGameWindow.Show();
        }

        private void BrdSteam_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("steam://store/" + globalAppId);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Grid grdLatest = grdPart1;
            var grdBox = grdGame;
            if (grdGame.Children.Count < 3)
            {
                grdBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF272727"));
                dropShadowEffect.Opacity = 0.3;
                dropShadowEffect.BlurRadius = 5;
                dropShadowEffect.ShadowDepth = 1;
                grdBox.Effect = dropShadowEffect;

                for (int i = 0; i < grdBox.Children.Count; i++)
                {
                    if (grdBox.Children[i].GetType() == typeof(Border))
                    {
                        ((Border)grdBox.Children[i]).HorizontalAlignment = HorizontalAlignment.Left;
                        ((Border)grdBox.Children[i]).VerticalAlignment = VerticalAlignment.Top;
                        ((Border)grdBox.Children[i]).Margin = new Thickness(10);
                    }
                }

                StackPanel spMisc = new StackPanel();
                spMisc.Orientation = Orientation.Horizontal;
                spMisc.Margin = new Thickness(10, 130, 10, 10);

                TextBlock tblRating = new TextBlock();
                tblRating.Text = string.Format("User Ratings: {0}%", e.UserState.ToString());
                tblRating.FontSize = 14;
                tblRating.Foreground = new SolidColorBrush(Colors.LightGray);
                tblRating.TextWrapping = TextWrapping.Wrap;

                Border brdRating = new Border();
                brdRating.VerticalAlignment = VerticalAlignment.Top;
                brdRating.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF202020"));
                brdRating.HorizontalAlignment = HorizontalAlignment.Left;
                brdRating.Margin = new Thickness(2);
                brdRating.Padding = new Thickness(8, 4, 4, 4);
                brdRating.Width = 150;
                brdRating.Height = 28;
                brdRating.CornerRadius = new CornerRadius(4);
                brdRating.Child = tblRating;

                TextBlock tblRatingText = new TextBlock();

                tblRatingText.FontSize = 14;
                tblRatingText.Foreground = new SolidColorBrush(Colors.LightGray);
                tblRatingText.TextWrapping = TextWrapping.Wrap;

                Border brdRatingText = new Border();
                brdRatingText.VerticalAlignment = VerticalAlignment.Top;
                brdRatingText.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF202020"));
                brdRatingText.HorizontalAlignment = HorizontalAlignment.Left;
                brdRatingText.Margin = new Thickness(2);
                brdRatingText.Padding = new Thickness(8, 4, 4, 4);
                brdRatingText.Width = 78;
                brdRatingText.Height = 28;
                brdRatingText.CornerRadius = new CornerRadius(4);
                brdRatingText.Child = tblRatingText;

                if (Convert.ToDecimal(e.UserState.ToString()) < 50)
                {
                    tblRatingText.Text = "Bad";
                    brdRatingText.Width = 50;
                }
                if (Convert.ToDecimal(e.UserState.ToString()) > 50 && Convert.ToDecimal(e.UserState.ToString()) < 80)
                {
                    tblRatingText.Text = "Good";
                    brdRatingText.Width = 55;
                }
                if (Convert.ToDecimal(e.UserState.ToString()) > 50 && Convert.ToDecimal(e.UserState.ToString()) > 80)
                {
                    tblRatingText.Text = "Excellent";
                    brdRatingText.Width = 75;
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

                            if (line.Contains("\"" + grdGame.Tag.ToString() + "\""))
                            {
                                gameNum = lineNum;
                            }
                            if (lineNum == gameNum + 2)
                            {
                                if (line.Contains("\"LastPlayed\""))
                                {
                                    string[] username = (line.Replace("\"LastPlayed\"", "")).Split('"');

                                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                    dateTime = dateTime.AddSeconds(Convert.ToDouble(username[1])).ToLocalTime();

                                    TextBlock tblLastPlayed = new TextBlock();
                                    tblLastPlayed.Text = string.Format("Last Played " + dateTime.ToShortDateString());
                                    tblLastPlayed.FontSize = 14;
                                    tblLastPlayed.Foreground = new SolidColorBrush(Colors.LightGray);
                                    tblLastPlayed.TextWrapping = TextWrapping.Wrap;

                                    Border brdLastPlayed = new Border();
                                    brdLastPlayed.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF202020"));
                                    brdLastPlayed.HorizontalAlignment = HorizontalAlignment.Left;
                                    brdLastPlayed.VerticalAlignment = VerticalAlignment.Top;
                                    brdLastPlayed.Margin = new Thickness(2);
                                    brdLastPlayed.Padding = new Thickness(8, 4, 4, 4);
                                    brdLastPlayed.Width = 165;
                                    brdLastPlayed.Height = 28;
                                    brdLastPlayed.CornerRadius = new CornerRadius(4);
                                    brdLastPlayed.Child = tblLastPlayed;

                                    spMisc.Children.Add(brdLastPlayed);
                                }
                            }
                        }
                    }

                }
                catch
                {

                }
                spMisc.Children.Add(brdRatingText);
                spMisc.Children.Add(brdRating);

                TextBlock tblDesc = new TextBlock();
                tblDesc.SnapsToDevicePixels = true;
                TextOptions.SetTextFormattingMode(tblDesc, TextFormattingMode.Display);
                tblDesc.Text = description.Replace("&amp;quot;", "\"");
                tblDesc.HorizontalAlignment = HorizontalAlignment.Right;
                tblDesc.VerticalAlignment = VerticalAlignment.Top;
                tblDesc.Margin = new Thickness(250, 25, 10, 0);
                tblDesc.Height = 90;
                tblDesc.TextTrimming = TextTrimming.WordEllipsis;
                tblDesc.FontSize = 14;
                tblDesc.Foreground = new SolidColorBrush(Colors.LightGray);
                tblDesc.TextWrapping = TextWrapping.Wrap;
                grdPart1.Children.Add(tblDesc);
                grdLatest = grdPart1;

                Button btnGameNews = new Button();
                btnGameNews.HorizontalAlignment = HorizontalAlignment.Left;
                btnGameNews.VerticalAlignment = VerticalAlignment.Top;
                btnGameNews.Margin = new Thickness(2);
                btnGameNews.Height = 28;
                btnGameNews.Width = 60;
                btnGameNews.Click += BtnGameNews_Click;
                btnGameNews.Content = "News";
                btnGameNews.Style = (Style)Application.Current.Resources["ButtonStyle1"];

                spMisc.Children.Add(btnGameNews);

                Image imgAddWidget = new Image();
                imgAddWidget.Source = new BitmapImage(new Uri(@"UI\add_widget.png", UriKind.Relative));
                imgAddWidget.VerticalAlignment = VerticalAlignment.Top;
                imgAddWidget.HorizontalAlignment = HorizontalAlignment.Right;
                imgAddWidget.Width = 24;
                imgAddWidget.Height = 24;
                imgAddWidget.Opacity = 0.5;
                imgAddWidget.Cursor = Cursors.Hand;
                imgAddWidget.MouseLeftButtonUp += ImgAddWidget_MouseLeftButtonUp;
                imgAddWidget.Margin = new Thickness(0, 5, 5, 0);
                grdPart1.Children.Add(imgAddWidget);

                grdPart1.Children.Add(spMisc);
                grdBox.Children.Add(grdPart1);

                //Screenshots
                StackPanel spScreenshots = new StackPanel();
                spScreenshots.Orientation = Orientation.Horizontal;

                try
                {
                    foreach (string userfiles in Directory.GetDirectories(Properties.Settings.Default.userDataPath))
                    {
                        if (Directory.Exists(userfiles + "\\760\\remote\\" + globalAppId) && Directory.Exists(userfiles + "\\760\\remote\\" + globalAppId + "\\screenshots"))
                        {
                            foreach (string sc in Directory.GetFiles(userfiles + "\\760\\remote\\" + globalAppId + "\\screenshots"))
                            {
                                if (sc != null)
                                {
                                    spScreenshots.Margin = new Thickness(10, 170, 10, 10);
                                    Image imgScreenshot = new Image();
                                    RenderOptions.SetBitmapScalingMode(imgScreenshot, BitmapScalingMode.Fant);
                                    imgScreenshot.Margin = new Thickness(4);
                                    imgScreenshot.Source = new BitmapImage(new Uri(sc));
                                    imgScreenshot.Width = 100;
                                    imgScreenshot.Cursor = Cursors.Hand;
                                    imgScreenshot.Tag = sc;
                                    imgScreenshot.MouseLeftButtonUp += ImgScreenshot_MouseLeftButtonUp;
                                    spScreenshots.Children.Add(imgScreenshot);
                                    ContextMenu cmScreenshot = new ContextMenu();
                                    cmScreenshot.Style = (Style)Application.Current.Resources["ContextMenuStyle1"];
                                    MenuItem miShowAllScreenshots = new MenuItem();
                                    miShowAllScreenshots.Header = "Show all";
                                    miShowAllScreenshots.Style = (Style)Application.Current.Resources["MenuItemStyle1"];
                                    miShowAllScreenshots.Tag = sc;
                                    miShowAllScreenshots.Click += MiShowAllScreenshots_Click;
                                    cmScreenshot.Items.Add(miShowAllScreenshots);
                                    imgScreenshot.ContextMenu = cmScreenshot;
                                }
                            }
                        }

                    }
                }
                catch
                {
                
                }

                grdPart2.Children.Add(spScreenshots);
                grdLatest = grdPart2;
                grdBox.Children.Add(grdPart2);
            }
            else
            {
                foreach (UIElement element in grdGame.Children)
                {
                    if (element.GetType() != typeof(Border))
                    {
                        element.Visibility = Visibility.Visible;
                        grdGame.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF272727"));
                    }
                    else
                    {
                        ((Border)element).Margin = new Thickness(10);
                    }
                }
            }

            Image imgUp = new Image();
            imgUp.Source = new BitmapImage(new Uri(@"UI\up.png", UriKind.Relative));
            imgUp.VerticalAlignment = VerticalAlignment.Bottom;
            imgUp.HorizontalAlignment = HorizontalAlignment.Right;
            imgUp.Width = 24;
            imgUp.Height = 24;
            imgUp.Cursor = Cursors.Hand;
            imgUp.Opacity = 0.5;
            imgUp.MouseLeftButtonUp += ImgUp_MouseLeftButtonUp;
            imgUp.Margin = new Thickness(0, 0, 5, 5);
            grdLatest.Children.Add(imgUp);
        }

        private void MiShowAllScreenshots_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            Process.Start(Directory.GetParent(menuItem.Tag.ToString()).ToString());
        }

        private void ImgScreenshot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(((Image)sender).Tag.ToString());
        }

        private void BtnGameNews_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://appnews/" + globalAppId);
        }

        private void ImgAddWidget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EditGameWindow editGameWindow = new EditGameWindow();
            editGameWindow.appId = globalAppId;
            editGameWindow.type = "steam";
            editGameWindow.Show();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            foreach (string line in File.ReadLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games\\" + globalAppId + ".txt"))
            {
                i++;
                if (i == 1)
                {
                    if (line == "null")
                    {
                        BackgroundWorker worker = sender as BackgroundWorker;

                        HtmlDocument steamDoc = new HtmlWeb().Load("http://store.steampowered.com/app/" + e.Argument);
                        HtmlNode node = steamDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
                        if (node != null)
                        {
                            HtmlAttribute desc = node.Attributes["content"];
                            description = desc.Value;
                        }
                    }
                    else
                    {
                        description = line;
                    }
                }
            }

            HtmlDocument steamdbDoc = new HtmlWeb().Load("http://steamdb.info/app/" + e.Argument);
            HtmlNode reviewsNode = steamdbDoc.DocumentNode.SelectSingleNode("//meta[@itemprop='ratingValue']");
            if (reviewsNode != null)
            {
                HtmlAttribute desc = reviewsNode.Attributes["content"];
                worker.ReportProgress(0, desc.Value);
            }

            
        }

        private void ImgUp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (UIElement element in grdGame.Children)
            {
                if (element.GetType() != typeof(Border) && element.GetType() != typeof(Image))
                {
                    element.Visibility = Visibility.Collapsed;
                    grdGame.Background = new SolidColorBrush(Colors.Transparent);
                    dropShadowEffect.Opacity = 0;
                }
                else
                {
                    ((Border)element).Margin = new Thickness(0);
                }
            }
        }

        private void BrdGame_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!worker.IsBusy)
            {
                dropShadowEffect.Opacity = 0.3;
                worker.RunWorkerAsync(grdGame.Tag.ToString());
            }
        }

        private void BtnLaunch_MouseLeave(object sender, MouseEventArgs e)
        {
            var game = (Button)sender;
            game.Foreground = new SolidColorBrush(Colors.LightGray);
            game.Opacity = 0.6;
        }

        private void BtnLaunch_MouseEnter(object sender, MouseEventArgs e)
        {
            var game = (Button)sender;
            game.Foreground = new SolidColorBrush(Colors.White);
            game.Opacity = 1;
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            var game = (Button)sender;
            Process.Start("steam://rungameid/" + game.Tag.ToString());
        }

        private void brdGame_MouseLeave(object sender, MouseEventArgs e)
        {
            var game = (Border)sender;
            game.Child.Visibility = Visibility.Collapsed;
        }

        private void brdGame_MouseEnter(object sender, MouseEventArgs e)
        {
            var game = (Border)sender;
            game.Child.Visibility = Visibility.Visible;
        }
    }
}
