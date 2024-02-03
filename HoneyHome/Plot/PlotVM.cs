using HoneyHome.BaseVM;
using HoneyHome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Plot
{
    internal class PlotVM  : ViewModelBase, ICloseable
    {
        IDatabaseProvider _dataProvider;
        Int64 _deviceId;
        public PlotVM(IDatabaseProvider dataProvider, Int64 deviceId)
        {
            if (dataProvider?.IsDatabaseConnected != true)
                throw new ArgumentNullException(nameof(dataProvider));
            _dataProvider = dataProvider;
            _deviceId = deviceId;
            GetMinMaxValues();
        }


        public string Title { get => Get<string>(); set => Set(value); }

        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }

        internal double[] GetHours()
        {
            double[] hours = new double[24];
            for (int i = 0; i < 24; i++)
                hours[i] = i;
            return hours;
        }
        private void GetMinMaxValues()
        {
            var res = _dataProvider.GetDeviceMinMaxValues(_deviceId, DateTime.UtcNow);
            MinValue = res.min ?? 0;
            MaxValue = res.max ?? MinValue;
        }

        internal double[] GetHoursValues()
        {
            double[] hoursValues = new double[24];
            for (int i = 0; i < 24; i++) {
                hoursValues[i] = _dataProvider.GetDeviceValues(_deviceId, i, DateTime.UtcNow) ?? 0.0;
            }
            return hoursValues;
        }
        #region ICloseable
        public event EventHandler<bool> CloseRequest;
        RelayCommand? _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(OnCloseCommand));

        private void OnCloseCommand()
        {
            CloseRequest?.Invoke(this, false);
        }


        #endregion ICloseable
    }
}
