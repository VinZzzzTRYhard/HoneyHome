using HoneyHome.BaseVM;
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

namespace HoneyHome.Settings.Devices
{
    /// <summary>
    /// Interaction logic for DeviceInfo.xaml
    /// </summary>
    public partial class DeviceInfo : Window
    {
        public DeviceInfo()
        {
            InitializeComponent();
            DataContextChanged += DeviceInfo_DataContextChanged;
        }

        private void DeviceInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ICloseable closeable)
            {
                closeable.CloseRequest += Closeable_CloseRequest;
            }
        }

        private void Closeable_CloseRequest(object? sender, bool e)
        {
            DialogResult = e;
        }
    }
}
