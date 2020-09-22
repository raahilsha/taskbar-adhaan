using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimeFinder
{
    public partial class ConfigWindow : Window
    {
        UserControl1 _parent;

        public ConfigWindow(UserControl1 parent)
        {
            _parent = parent;
            InitializeComponent();
            latText.Text = parent.latitude.ToString();
            longText.Text = parent.longitude.ToString();
            methodCB.SelectedIndex = parent.method;
            schoolCB.SelectedIndex = parent.school;
        }

        private void SaveConfig(object sender, RoutedEventArgs e)
        {

            string configfilepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\NextPrayerTime.conf";
            string[] lines = { "latitude:" + latText.Text, "longitude:" + longText.Text, "method:" + methodCB.SelectedIndex, "school:" + schoolCB.SelectedIndex };
            System.IO.File.WriteAllLines(configfilepath, lines);

            _parent.configReloaded = true;
        }
    }
}
