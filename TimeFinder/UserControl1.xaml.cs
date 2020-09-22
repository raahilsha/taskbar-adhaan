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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CSDeskBand;

namespace TimeFinder
{
    public partial class UserControl1 : INotifyPropertyChanged
    {
        private Orientation _taskbarOrientation;
        private int _taskbarWidth;
        private int _taskbarHeight;
        private Edge _taskbarEdge;
        private String _displayText;

        public float latitude;
        public float longitude;
        public int method;
        public int school;
        public bool configReloaded;

        public String DisplayText
        {
            get => _displayText;
            set
            {
                if (value == _displayText) return;
                _displayText = value;
                OnPropertyChanged();
            }
        }

        public Orientation TaskbarOrientation
        {
            get => _taskbarOrientation;
            set
            {
                if (value == _taskbarOrientation) return;
                _taskbarOrientation = value;
                OnPropertyChanged();
            }
        }

        public int TaskbarWidth
        {
            get => _taskbarWidth;
            set
            {
                if (value == _taskbarWidth) return;
                _taskbarWidth = value;
                OnPropertyChanged();
            }
        }

        public int TaskbarHeight
        {
            get => _taskbarHeight;
            set
            {
                //if (value == _taskbarHeight) return;
                double scale = PresentationSource.FromVisual(boundingBox).CompositionTarget.TransformToDevice.M22;
                double newHeight = value / scale * 0.9;
                boundingBox.Height = newHeight;
                if (newHeight < 30)
                {
                    boundingBox.Width = 100;
                    boundingBox.FontSize = 6;
                    nextTimeText.FontSize = 6;
                }
                else
                {
                    boundingBox.Width = 180;
                    boundingBox.FontSize = 8;
                    nextTimeText.FontSize = 8;
                }
                _taskbarHeight = value;
                OnPropertyChanged();
            }
        }

        public Edge TaskbarEdge
        {
            get => _taskbarEdge;
            set
            {
                if (value == _taskbarEdge) return;
                _taskbarEdge = value;
                OnPropertyChanged();
            }
        }

        public UserControl1()
        {
            configReloaded = false;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ConfigButtonPressed(object sender, RoutedEventArgs e)
        {
            ConfigWindow w2 = new ConfigWindow(this);
            w2.Show();
        }
    }
}
