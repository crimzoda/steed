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
    class GameItem
    {
        string appEmu = "";
        string appDirectory = "";
        string appDataFile = "";
        Grid grdGame = new Grid();
        Button btnGameState;
        DropShadowEffect dropShadowEffect = new DropShadowEffect();
        Grid grdPart1 = new Grid();
        bool isPlaying = false;
        Border brdEmulator = new Border();
        BackgroundWorker worker = new BackgroundWorker();

        public Grid Load(string appDir, string appName, string appHeader, string appEmulator, string appNum)
        {
            Border brdGame = new Border();
            Button btnLaunch = new Button();

            appDataFile = appNum;
            appDirectory = appDir;
            appEmu = appEmulator;
            //grdGame properties
            grdGame.Tag = appName;
            grdGame.Margin = new Thickness(7);
            grdGame.HorizontalAlignment = HorizontalAlignment.Center;
            grdGame.VerticalAlignment = VerticalAlignment.Top;

            //brdGame properties
            brdGame.CornerRadius = new CornerRadius(4);
            brdGame.Tag = appName;
            brdGame.MouseEnter += brdGame_MouseEnter;
            brdGame.MouseLeave += brdGame_MouseLeave;
            brdGame.MouseLeftButtonUp += BrdGame_MouseLeftButtonUp;
            brdGame.Cursor = Cursors.Hand;
            brdGame.Width = 230;
            brdGame.Height = 107.5;

            Border brdContainer = new Border();
            brdContainer.Child = brdGame;
            brdContainer.CornerRadius = new CornerRadius(4);
            brdContainer.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#7E111111"));
            grdGame.Children.Add(brdContainer);

            ContextMenu cmGameMenu = new ContextMenu();
            cmGameMenu.Style = (Style)Application.Current.Resources["ContextMenuStyle1"];
            MenuItem miEditGame = new MenuItem();
            miEditGame.Header = "Edit Game";
            MenuItem miDeleteGame = new MenuItem();
            miDeleteGame.Header = "Delete";
            miDeleteGame.Style = (Style)Application.Current.Resources["MenuItemStyle1"];
            miEditGame.Style = (Style)Application.Current.Resources["MenuItemStyle1"];
            miEditGame.Click += MiEditGame_Click;
            miDeleteGame.Click += MiDeleteGame_Click;
            cmGameMenu.Items.Add(miEditGame);
            cmGameMenu.Items.Add(miDeleteGame);
            brdGame.ContextMenu = cmGameMenu;

            //brdGameDetails properties
            Border brdGameDetails = new Border();
            brdGameDetails.VerticalAlignment = VerticalAlignment.Bottom;
            if (appHeader == "none")
            {
                ImageBrush ibIcon = new ImageBrush(GetIcon(appDir));
                ibIcon.Stretch = Stretch.None;
                ibIcon.AlignmentX = AlignmentX.Center;
                ibIcon.AlignmentY = AlignmentY.Center;
                brdGame.Background = ibIcon;
            }
            else
            {
                brdGame.Background = new ImageBrush(new BitmapImage(new Uri(appHeader)));
            }
            brdGameDetails.Height = 50;
            brdGameDetails.Background = new SolidColorBrush(Colors.Black);
            brdGameDetails.Opacity = 0.8;
            brdGameDetails.CornerRadius = new CornerRadius(0, 0, 4, 4);
            brdGameDetails.Margin = new Thickness(0, 0, 0, -0.5);
            brdGame.Child = brdGameDetails;
            brdGame.Child.Visibility = Visibility.Collapsed;

            if (appEmulator != "")
            {
                brdEmulator.Width = 24;
                brdEmulator.Height = 24;
                brdEmulator.Margin = new Thickness(5);
                brdEmulator.Cursor = Cursors.Hand;
                brdEmulator.Background = new ImageBrush(GetIcon(appEmulator));
                brdEmulator.CornerRadius = new CornerRadius(4);
                brdEmulator.VerticalAlignment = VerticalAlignment.Top;
                brdEmulator.HorizontalAlignment = HorizontalAlignment.Left;
                brdEmulator.Opacity = 0.7;
            }

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
            btnLaunch.MouseEnter += BtnLaunch_MouseEnter;
            btnLaunch.MouseLeave += BtnLaunch_MouseLeave;
            btnLaunch.Tag = appDir;
            btnLaunch.Click += btnLaunch_Click;
            btnLaunch.Width = 90;
            btnLaunch.Height = 30;
            btnLaunch.Content = "Launch";
            btnLaunch.HorizontalAlignment = HorizontalAlignment.Right;
            btnLaunch.Margin = new Thickness(4);
            btnGameState = btnLaunch;

            worker.DoWork += Worker_DoWork;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += Worker_ProgressChanged;

            Grid grdGameDetails = new Grid();
            grdGameDetails.Children.Add(tblGameTitle);
            grdGameDetails.Children.Add(btnLaunch);

            brdGameDetails.Child = grdGameDetails;
            grdGame.Children.Add(brdEmulator);

            return grdGame;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            var grdBox = grdGame;

            grdBox.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF272727"));
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

            brdEmulator.Margin = new Thickness(15);

            if (e.UserState.ToString() == "null")
            {
                TextBlock tblDesc = new TextBlock();
                tblDesc.SnapsToDevicePixels = true;
                TextOptions.SetTextFormattingMode(tblDesc, TextFormattingMode.Display);
                tblDesc.Text = "This game doesn't have a description.";
                tblDesc.HorizontalAlignment = HorizontalAlignment.Right;
                tblDesc.Margin = new Thickness(250, 50, 10, 0);
                tblDesc.FontSize = 14;
                tblDesc.Foreground = new SolidColorBrush(Colors.LightGray);
                tblDesc.TextWrapping = TextWrapping.Wrap;
                grdPart1.Children.Add(tblDesc);
            }
            else
            {
                TextBlock tblDesc = new TextBlock();
                tblDesc.SnapsToDevicePixels = true;
                TextOptions.SetTextFormattingMode(tblDesc, TextFormattingMode.Display);
                tblDesc.Text = e.UserState.ToString();
                tblDesc.HorizontalAlignment = HorizontalAlignment.Right;
                tblDesc.VerticalAlignment = VerticalAlignment.Top;
                tblDesc.Margin = new Thickness(250, 25, 10, 0);
                tblDesc.Height = 90;
                tblDesc.TextTrimming = TextTrimming.WordEllipsis;
                tblDesc.FontSize = 14;
                tblDesc.Foreground = new SolidColorBrush(Colors.LightGray);
                tblDesc.TextWrapping = TextWrapping.Wrap;
                grdPart1.Children.Add(tblDesc);
            }

            TextBlock tblLastPlayed = new TextBlock();
            tblLastPlayed.Text = string.Format("Last Played " + File.GetLastAccessTime(appDirectory));
            tblLastPlayed.FontSize = 14;
            tblLastPlayed.Foreground = new SolidColorBrush(Colors.LightGray);
            tblLastPlayed.TextWrapping = TextWrapping.Wrap;

            Border brdLastPlayed = new Border();
            brdLastPlayed.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF202020"));
            brdLastPlayed.HorizontalAlignment = HorizontalAlignment.Left;
            brdLastPlayed.Margin = new Thickness(10, 130, 10, 10);
            brdLastPlayed.Padding = new Thickness(8, 4, 4, 4);
            brdLastPlayed.Width = 200;
            brdLastPlayed.Height = 28;
            brdLastPlayed.CornerRadius = new CornerRadius(4);
            brdLastPlayed.Child = tblLastPlayed;
            grdPart1.Children.Add(brdLastPlayed);

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
            grdPart1.Children.Add(imgUp);

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

            grdBox.Children.Add(grdPart1);
        }

        private void ImgAddWidget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EditGameWindow editGameWindow = new EditGameWindow();
            editGameWindow.appId = System.IO.Path.GetFileNameWithoutExtension(appDataFile);
            editGameWindow.type = "game";
            editGameWindow.Show();
        }

        private void ImgUp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            grdGame.Children.Remove(grdPart1);
            dropShadowEffect.Opacity = 0;
            foreach (UIElement element in grdGame.Children)
            {
                if (element.GetType() == typeof(Border))
                {
                    ((Border)element).Margin = new Thickness(0);
                }
            }

            brdEmulator.Margin = new Thickness(5);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int lineNum = 0;
            foreach (string line in File.ReadLines(appDataFile))
            {
                lineNum++;

                if (lineNum == 5)
                {
                    worker.ReportProgress(0, line);
                }
            }
        }

        private void MiDeleteGame_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(appDataFile);
            grdGame.Visibility = Visibility.Collapsed;
        }

        private void MiEditGame_Click(object sender, RoutedEventArgs e)
        {
            EditGameWindow editGameWindow = new EditGameWindow();
            editGameWindow.appId = System.IO.Path.GetFileNameWithoutExtension(appDataFile);
            editGameWindow.type = "game";
            editGameWindow.Show();
        }

        public static ImageSource GetIcon(string fileName)
        {
            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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
            Process gameProc = new Process();
            gameProc.StartInfo.FileName = appEmu;
            gameProc.EnableRaisingEvents = true;

            if (appEmu != "")
            {
                gameProc.StartInfo.Arguments = " XXXX \"" + game.Tag.ToString() + "\"";
            }
            else
            {
                gameProc.StartInfo.FileName = appDirectory;
            }

            gameProc.Exited += GameProc_Exited;
            gameProc.Start();
            btnGameState.Content = "Playing";
            isPlaying = true;
        }

        private void GameProc_Exited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate { btnGameState.Content = "Launch"; isPlaying = false; }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
        }

        private void brdGame_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isPlaying == false)
            {
                var game = (Border)sender;
                game.Child.Visibility = Visibility.Collapsed;
            }
        }

        private void brdGame_MouseEnter(object sender, MouseEventArgs e)
        {
            var game = (Border)sender;
            game.Child.Visibility = Visibility.Visible;
        }
    }
}
