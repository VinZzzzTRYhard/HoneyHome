using HoneyHome.BaseVM;
using HoneyHome.Interfaces;
using HoneyHome.Model;
using HoneyHome.Settings.Devices;
using HoneyHome.Settings.Plugins;
using HoneyHome.Settings.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static HoneyHome.Settings.Devices.DeviceInfoVM;
using PluginInfo = HoneyHome.Model.PluginInfo;

namespace HoneyHome.Settings
{
    internal class SettingsVM : ViewModelBase, ICloseable
    {
        private IDatabaseProvider _dataProvider;
        public SettingsVM(IDatabaseProvider dataProvider) {
            _dataProvider = dataProvider;
            UpdateRooms();
            UpdatePlugins();
            UpdateDevices();
        }

        public event EventHandler<bool> CloseRequest;
        RelayCommand? _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(OnCloseCommand));

        private void OnCloseCommand()
        {
            CloseRequest?.Invoke(this, false);
        }

        #region Rooms
        public IList<RoomsItems> Rooms
        {
            get => Get<IList<RoomsItems>>();
            set=> Set(value);
        }
        public Int64 SelectedRoomId { get => Get<Int64>(); set => Set(value); }

        private void UpdateRooms(Int64 selectedId = 0)
        {
            if (_dataProvider?.IsDatabaseConnected == true)
            {
                var roomsList = _dataProvider.GetRooms();
                if (roomsList?.Any() == true)
                {
                    var roomList = new List<RoomsItems>();
                    SelectedRoomId = 0;
                    foreach (var room in roomsList)
                        roomList.Add(new RoomsItems() { Name = room.Name, RoomId = room.RoomId });

                    Rooms = roomList;
                    if (selectedId == 0)
                        SelectedRoomId = roomList.FirstOrDefault()?.RoomId ?? 0;
                    else if (selectedId == -1)
                        SelectedRoomId = roomList.LastOrDefault()?.RoomId ?? 0;
                    else
                        SelectedRoomId = selectedId;
                }
            }
        }

        private RelayCommand? _addRoomCommand;
        public RelayCommand AddRoomCommand => _addRoomCommand ?? (_addRoomCommand = new RelayCommand(OnAddRoomCommand));

        private void OnAddRoomCommand()
        {
            var roomVm = new RoomVM(_dataProvider);
            roomVm.Id = null;
            roomVm.Name = string.Empty;
            var dialog = new Room();
            dialog.DataContext = roomVm;
            if (dialog.ShowDialog() ==  true)
                UpdateRooms(-1);
        }

        private RelayCommand? _editRoomCommand;
        public RelayCommand EditRoomCommand => _editRoomCommand ?? (_editRoomCommand = new RelayCommand(OnEditRoomCommand));

        private void OnEditRoomCommand()
        {
            if (_dataProvider?.IsDatabaseConnected == true)
            {
                // Check if exists
                var nameOfRoom = _dataProvider.GetRoom(SelectedRoomId)?.Name ?? string.Empty;
                if (!string.IsNullOrEmpty(nameOfRoom))
                {
                    var roomVm = new RoomVM(_dataProvider);
                    roomVm.Id = SelectedRoomId;
                    roomVm.Name = nameOfRoom;
                    var dialog = new Room();
                    dialog.DataContext = roomVm;
                    if (dialog.ShowDialog() == true)
                        UpdateRooms(SelectedRoomId);
                }
            }
        }

        private RelayCommand? _delRoomCommand;
        public RelayCommand DelRoomCommand => _delRoomCommand ?? (_delRoomCommand = new RelayCommand(OnDelRoomCommand));

        private void OnDelRoomCommand()
        {
            if (MessageBox.Show("Do you wish to delete this room?", "HoneyHome", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                if (_dataProvider?.IsDatabaseConnected == true)
                    if (_dataProvider.DeleteRoom(SelectedRoomId))
                        UpdateRooms();
            }
        }

        #endregion Rooms


        #region Plugin
        public IList<IPluginInfo> PluginSources
        {
            get => Get<IList<IPluginInfo>>();
            set => Set(value);
        }
        public IPluginInfo? PluginSelected { get => Get<IPluginInfo?>(); set => Set(value); }

        private void UpdatePlugins(Int64 selectedId = 0)
        {
            if (_dataProvider?.IsDatabaseConnected == true)
            {
                var pluginList = _dataProvider.GetPluginInfos();
                if (pluginList?.Any() == true)
                {
                    var pluginSources = new List<IPluginInfo>();
                    PluginSelected = null;
                    foreach (var plugin in pluginList)
                        pluginSources.Add(new PluginInfo(plugin.Id, plugin.Name, plugin.PluginPath));

                    PluginSources = pluginSources;
                    if (selectedId == 0)
                        PluginSelected = pluginSources.FirstOrDefault();
                    else if (selectedId == -1)
                        PluginSelected = pluginSources.LastOrDefault();
                    else
                        PluginSelected = PluginSources.Where(x=> x.Id == selectedId).FirstOrDefault();
                }
            }
        }

        RelayCommand? _addPluginCommand;

        public RelayCommand AddPluginCommand => _addPluginCommand ?? ( _addPluginCommand = new RelayCommand(OnAddPluginCommand));

        private void OnAddPluginCommand()
        {
            var pInfoVm = new PluginInfoVM(_dataProvider);
            pInfoVm.Id = null;
            pInfoVm.Name = string.Empty;
            pInfoVm.PluginPath = string.Empty;
            var dialog = new Plugins.PluginInfo();
            dialog.DataContext = pInfoVm;
            if (dialog.ShowDialog() == true)
                UpdatePlugins(-1);
        }

        RelayCommand? _editPluginCommand;
        public RelayCommand EditPluginCommand => _editPluginCommand ?? (_editPluginCommand = new RelayCommand(OnEditPluginCommand));

        private void OnEditPluginCommand()
        {
            if (_dataProvider?.IsDatabaseConnected == true && PluginSelected?.Id != null)
            {
                // Check if exists
                var pluginInfo = _dataProvider.GetPluginInfo(PluginSelected.Id.Value);
                if (!string.IsNullOrEmpty(pluginInfo?.Name) && !string.IsNullOrEmpty(pluginInfo?.PluginPath))
                {
                    var pInfoVm = new PluginInfoVM(_dataProvider);
                    pInfoVm.Id = PluginSelected.Id;
                    pInfoVm.Name = pluginInfo.Name;
                    pInfoVm.PluginPath = pluginInfo.PluginPath;
                    var dialog = new Plugins.PluginInfo();
                    dialog.DataContext = pInfoVm;
                    if (dialog.ShowDialog() == true)
                        UpdatePlugins(PluginSelected.Id.Value);
                }
            }
        }
        private RelayCommand? _delPluginCommand;
        public RelayCommand DelPluginCommand => _delPluginCommand ?? (_delPluginCommand = new RelayCommand(OnDelPluginCommand, OnDelPluginCommandCanExecute));

        private bool OnDelPluginCommandCanExecute()
        {
            return (PluginSelected?.Id ?? 0) > 0;
        }

        private void OnDelPluginCommand()
        {
            if ((PluginSelected?.Id ?? 0) <= 0)
                return;

            if (MessageBox.Show("Do you wish to delete this plugin?", "HoneyHome", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                if (_dataProvider?.IsDatabaseConnected == true)
                    if (_dataProvider.DeletePlugin(PluginSelected?.Id ?? 0))
                        UpdatePlugins();
            }
        }
        #endregion Plugin

        #region Devices
        public IList<IDevice> DeviceSources
        {
            get => Get<IList<IDevice>>();
            set => Set(value);
        }
        public IDevice? DeviceSelected { get => Get<IDevice?>(); set => Set(value); }

        private void UpdateDevices(Int64 selectedId = 0)
        {
            if (_dataProvider?.IsDatabaseConnected == true)
            {
                var deviceList = _dataProvider.GetDevices();
                if (deviceList?.Any() == true)
                {
                    var deviceSources = new List<IDevice>();
                    DeviceSelected = null;
                    foreach (var device in deviceList)
                        deviceSources.Add(new Device() 
                        { 
                            Id = device.Id,
                            RoomId = device.RoomId,
                            DeviceTypeId = device.DeviceTypeId,
                            Name = device.Name,
                            Information = device.Information,
                            PluginID = device.PluginID,
                            PluginParameter = device.PluginParameter,
                            HasCommand = device.HasCommand,
                            ExecuteButtonName = device.ExecuteButtonName
                        });

                    DeviceSources = deviceSources;
                    if (selectedId == 0)
                        DeviceSelected = deviceSources.FirstOrDefault();
                    else if (selectedId == -1)
                        DeviceSelected = deviceSources.LastOrDefault();
                    else
                        DeviceSelected = deviceSources.Where(x => x.Id == selectedId).FirstOrDefault();
                }
            }
        }

        RelayCommand? _addDeviceCommand;

        public RelayCommand AddDeviceCommand => _addDeviceCommand ?? (_addDeviceCommand = new RelayCommand(OnAddDeviceCommand));

        private void OnAddDeviceCommand()
        {
            var dInfoVm = new DeviceInfoVM(_dataProvider);
            dInfoVm.Id = 0;
            dInfoVm.Name = string.Empty;
            var dialog = new DeviceInfo();
            dialog.DataContext = dInfoVm;
            if (dialog.ShowDialog() == true)
                UpdateDevices (-1);
        }

        RelayCommand? _editDeviceCommand;
        public RelayCommand EditDeviceCommand => _editDeviceCommand ?? (_editDeviceCommand = new RelayCommand(OnEditDeviceCommand, OnEditDeviceCommandCanExecute));

        private bool OnEditDeviceCommandCanExecute()
        {
            return (DeviceSelected?.Id ?? 0) > 0;
        }

        private void OnEditDeviceCommand()
        {
            if ((DeviceSelected?.Id ?? 0) <= 0)
                return;
            // Check if exists
            var deviceInfo = _dataProvider.GetDevice(DeviceSelected.Id);
            if (!string.IsNullOrEmpty(deviceInfo?.Name) && !string.IsNullOrEmpty(deviceInfo.Information))
            {

                var dInfoVm = new DeviceInfoVM(_dataProvider);
                dInfoVm.Id = deviceInfo.Id;
                dInfoVm.RoomId = deviceInfo.RoomId;
                dInfoVm.DeviceTypeId = deviceInfo.DeviceTypeId;
                dInfoVm.Name = deviceInfo.Name;
                dInfoVm.Information = deviceInfo.Information;
                dInfoVm.PluginID = deviceInfo.PluginID;
                dInfoVm.PluginParameter = deviceInfo.PluginParameter;
                dInfoVm.HasCommand = deviceInfo.HasCommand;
                dInfoVm.ExecuteButtonName = deviceInfo.ExecuteButtonName;

                var dialog = new DeviceInfo();
                dialog.DataContext = dInfoVm;
                if (dialog.ShowDialog() == true)
                    UpdateDevices(DeviceSelected.Id);
            }
        }
        private RelayCommand? _delDeviceCommand;
        public RelayCommand DelDeviceCommand => _delDeviceCommand ?? (_delDeviceCommand = new RelayCommand(OnDelDeviceCommand, OnDelDeviceCommandCanExecute));

        private bool OnDelDeviceCommandCanExecute()
        {
            return (DeviceSelected?.Id ?? 0) > 0;
        }

        private void OnDelDeviceCommand()
        {
            if ((DeviceSelected?.Id ?? 0) <= 0)
                return;

            if (MessageBox.Show("Do you wish to delete this device?", "HoneyHome", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                if (_dataProvider?.IsDatabaseConnected == true)
                    if (_dataProvider.DeleteDeviceInfo(DeviceSelected?.Id ?? 0))
                        UpdateDevices();
            }
        }

        #endregion Devices

    }
}
