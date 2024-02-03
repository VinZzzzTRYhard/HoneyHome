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

namespace HoneyHome.Plot
{
    /// <summary>
    /// Interaction logic for Plot.xaml
    /// </summary>
    public partial class Plot : Window
    {
        public Plot()
        {
            InitializeComponent();
            DataContextChanged += Plot_DataContextChanged;
            Loaded += Plot_Loaded;
        }


        private void Plot_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ICloseable closeable)
                closeable.CloseRequest += Closeable_CloseRequest;
        }

        private void Closeable_CloseRequest(object? sender, bool e)
        {
            DialogResult = e;
        }

        private void Plot_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlotVM vm)
            {
                double[] dataX = vm.GetHours();
                WpfPlot1.Plot.Title(vm.Title);
                WpfPlot1.Plot.XLabel("Hours");
                WpfPlot1.Plot.Axes.SetLimitsX(0, 23);
                WpfPlot1.Plot.Axes.SetLimitsY(vm.MinValue, vm.MaxValue);
                WpfPlot1.Plot.Add.Scatter(vm.GetHours(), vm.GetHoursValues());
                WpfPlot1.Refresh();
            }
        }


    }
}
