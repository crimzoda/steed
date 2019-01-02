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

namespace Steed
{
    /// <summary>
    /// Interaction logic for GeneralSettingsPage.xaml
    /// </summary>
    public partial class GeneralSettingsPage : Page
    {
        public GeneralSettingsPage()
        {
            InitializeComponent();
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.steamPath = tbSteamPath.Text;
            Properties.Settings.Default.Save();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            tbSteamPath.Text = Properties.Settings.Default.steamPath;
        }
    }
}
