using HoneyHome.Interfaces;
using HoneyHome.Model;
using HoneyHome.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Device
{
    internal class DeviceManager
    {
        private const double DEVICE_POLLING_INTERVAL_MSEC = 5000;

        IDatabaseProvider _dataProvider;
        PluginManager _pluginManager;
        readonly List<Model.Device> _devices = new List<Model.Device>();
        public DeviceManager(IDatabaseProvider? dataProvider, PluginManager pluginManager) 
        {
            if (dataProvider?.IsDatabaseConnected != true)
                throw new ArgumentNullException(nameof(dataProvider));

            _dataProvider = dataProvider;
            _pluginManager = pluginManager;
        }

        public void InitializeDevices()
        {
            if (_devices.Any())
                DisposeDevices();

            _devices.Clear();
            if (_dataProvider?.IsDatabaseConnected == true)
            {
                var devicesInfo = _dataProvider.GetDevices();
                foreach (var device in devicesInfo)
                {
                    var deviceModel = new Model.Device()
                    {
                        Id = device.Id,
                        RoomId = device.RoomId,
                        DeviceTypeId = device.DeviceTypeId,
                        Name = device.Name,
                        Information = device.Information,
                        HasCommand = device.HasCommand,
                        ExecuteButtonNameTemplate = device.ExecuteButtonName
                    };

                    if (deviceModel.HasCommand)
                    {
                        deviceModel.DeviceExternalAction = new Action( () => {
                            _pluginManager.MakePluginAction(deviceModel.Id);
                            deviceModel.CurrentValue = _pluginManager.GetCurrentValue(device.Id);
                        });
                    }

                    // We should add only devices which plugins can initialize
                    if (_pluginManager.InitializeDevice(device.Id, device.PluginID, device.PluginParameter))
                    {
                        deviceModel.CurrentValue = _pluginManager.GetCurrentValue(device.Id);
                        _devices.Add(deviceModel);
                    }
                }
            }
            if (_devices.Any())
                StartDeviceSyncTimer();

        }

        public void DisposeDevices()
        {
            StopDeviceSyncTimer();
            if (_devices.Any())
            {
                foreach (var device in _devices)
                {
                    _pluginManager.DisposePluginForDevice(device.Id);
                }
                _devices.Clear();
            }
        }


        public IEnumerable<Model.Device> Devices => _devices;

        #region Timer for devices
        System.Timers.Timer? _timer;
        private void StartDeviceSyncTimer()
        {
            _timer = new System.Timers.Timer(DEVICE_POLLING_INTERVAL_MSEC);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void StopDeviceSyncTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var deviceModel in _devices)
            {
                deviceModel.CurrentValue = _pluginManager.GetCurrentValue(deviceModel.Id);
                _dataProvider.AddDeviceValue(deviceModel.Id, deviceModel.CurrentValue);
            }

        }

        #endregion Timer for devices

    }
}
