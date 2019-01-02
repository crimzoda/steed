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
using System.IO;

namespace Steed
{
    /// <summary>
    /// Interaction logic for EditGameWindow.xaml
    /// </summary>
    public partial class EditGameWindow : Window
    {
        public string appId = "";
        public string type = "";

        public EditGameWindow()
        {
            InitializeComponent();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> lines = new List<string>();

            if (type == "steam")
            {
                foreach (string line in File.ReadLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games\\" + appId + ".txt"))
                {
                    lines.Add(line);
                }

                tbDescription.Text = lines[0];

                if (lines[0] == "null")
                {
                    tbDescription.Text = "(Enter custom description)";
                }
            }
            else
            {
                foreach (string line in File.ReadLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games\\" + appId + ".txt"))
                {
                    lines.Add(line);
                }

                tbDescription.Text = lines[4];

                if (lines[4] == "null")
                {
                    tbDescription.Text = "(Enter custom description)";
                }
            }

           
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            tbDescription.Text = tbDescription.Text.Replace(Environment.NewLine, " ");
            if (type == "steam")
            {
                List<string> lines = new List<string>();
                foreach (string line in File.ReadLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games\\" + appId + ".txt"))
                {
                    lines.Add(line);
                }

                if (tbDescription.Text == "" || tbDescription.Text == "(Enter custom description)")
                {
                    lines[0] = "null";
                }
                else
                {
                    lines[0] = tbDescription.Text;
                }


                File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Steam Games\\" + appId + ".txt", lines);
            }
            else
            {
                List<string> lines = new List<string>();
                foreach (string line in File.ReadLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games\\" + appId + ".txt"))
                {
                    lines.Add(line);
                }

                if (tbDescription.Text == "" || tbDescription.Text == "(Enter custom description)")
                {
                    lines[4] = "null";
                }
                else
                {
                    lines[4] = tbDescription.Text;
                }

                File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Steed\\Games\\" + appId + ".txt", lines);
            }

        }
    }
}
