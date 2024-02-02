using HoneyHome.BaseVM;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HoneyHome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowVM();
            Closing += MainWindow_Closing;
            if (DataContext is ICloseable closeable)
                closeable.CloseRequest += Closeable_CloseRequest;
        }

        private void Closeable_CloseRequest(object? sender, bool e)
        {
            this.Close();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainWindowVM vm)
            {
                vm.WindowClosing();
            }
        }
    }
}