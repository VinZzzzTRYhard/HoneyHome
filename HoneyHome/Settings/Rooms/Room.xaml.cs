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

namespace HoneyHome.Settings.Rooms
{
    /// <summary>
    /// Interaction logic for Room.xaml
    /// </summary>
    public partial class Room : Window
    {
        public Room()
        {
            InitializeComponent();
            DataContextChanged += Room_DataContextChanged;
        }

        private void Room_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ICloseable closeable)
                closeable.CloseRequest += Closeable_CloseRequest;
        }

        private void Closeable_CloseRequest(object? sender, bool e)
        {
            if (e == true)
                DialogResult = true;
            else
                DialogResult = false;
        }
    }
}
