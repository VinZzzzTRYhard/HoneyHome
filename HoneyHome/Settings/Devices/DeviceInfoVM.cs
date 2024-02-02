using HoneyHome.BaseVM;
using HoneyHome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Settings.Devices
{
    internal class DeviceInfoVM : ViewModelBase, ICloseable, IDevice
    {
        IDatabaseProvider? _databaseProvider;
        public DeviceInfoVM(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
            UpdateDeviceTypeSources();
            UpdatePluginSources();
            UpdateRoomSources();
        }


        public event EventHandler<bool> CloseRequest;
        RelayCommand? _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(OnCloseCommand));

        private void OnCloseCommand()
        {
            CloseRequest?.Invoke(this, false);
        }

        public Int64 Id { get; set; }

        public Int64? RoomId {get => Get<Int64?>(); set=>Set(value); }

        public DeviceType DeviceTypeId {get=>Get<DeviceType>(); set=>Set(value); }

        public string Name
        {
            get => Get<string>(); set
            {
                if (Set(value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string Information
        {
            get => Get<string>();
            set
            {
                if (Set(value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public Int64 PluginID { get => Get<Int64>(); set => Set(value); }

        public string PluginParameter {get=>Get<string>(); set=>Set(value); }

        public bool HasCommand { get =>Get<bool>(); set=>Set(value); }

        public string ExecuteButtonName { get=>Get<String>(); set=>Set(value); }

        #region DeviceTypeSources
        public class DeviceTypeInfo
        {
            public string Name { get; set; }
            public DeviceType DeviceType { get; set; }
        }

        public List<DeviceTypeInfo> DeviceTypeSources {  get; set; }
        private void UpdateDeviceTypeSources()
        {
            DeviceTypeSources = new List<DeviceTypeInfo>
            {
                new DeviceTypeInfo()
                {
                    Name = "Switch",
                    DeviceType = DeviceType.Switch
                },
                new DeviceTypeInfo()
                {
                    Name = "Temperature",
                    DeviceType = DeviceType.Temperature
                },
                new DeviceTypeInfo()
                {
                    Name = "Weather",
                    DeviceType = DeviceType.Weather
                },
                new DeviceTypeInfo()
                {
                    Name = "Other",
                    DeviceType = DeviceType.Other
                }
            };
            DeviceTypeId = DeviceType.Switch;
        }

        #endregion DeviceTypeSources

        #region Plugin Sources
        public class PluginInfo
        {
            public string Name { get; set; }
            public Int64 PluginId { get; set; }
        }
        public List<PluginInfo> PluginSources { get; set; }

        private void UpdatePluginSources()
        {
            PluginSources = new List<PluginInfo>();

            if (_databaseProvider?.IsDatabaseConnected != true)
                return;

            var plugins = _databaseProvider.GetPluginInfos();
            if (plugins.Any())
            {
                foreach (var plugin in plugins)
                    PluginSources.Add(new PluginInfo() { Name = plugin.Name, PluginId = plugin.Id ?? 0 });
                PluginID = PluginSources.FirstOrDefault()?.PluginId ?? 0;
            }
        }

        #endregion Plugin Sources

        #region Room Sources
        public class RoomInfo
        {
            public string Name { get; set; }
            public Int64 RoomId { get; set; }
        }
        public List<RoomInfo> RoomSources { get; set; }
        private void UpdateRoomSources()
        {
            RoomSources = new List<RoomInfo>();
            RoomSources.Add(new RoomInfo() { Name = "No room", RoomId = 0 });
            RoomId = RoomSources.FirstOrDefault()?.RoomId ?? 0;

            if (_databaseProvider?.IsDatabaseConnected != true)
                return;

            var rooms = _databaseProvider.GetRooms();
            if (rooms.Any())
            {
                foreach (var room in rooms)
                    RoomSources.Add(new RoomInfo() { Name = room.Name, RoomId = room.RoomId});
            }

        }

        #endregion Room Sources

        private RelayCommand? _saveCommand;
        public RelayCommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(OnSaveCommand, OnSaveCommandCanExecute));

        private bool OnSaveCommandCanExecute()
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Information);
        }

        private void OnSaveCommand()
        {
            if (_databaseProvider != null && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Information))
            {
                if (Id != 0)
                {
                    // Update Device
                    if (_databaseProvider.UpdateDeviceInfo(RoomId, DeviceTypeId, Name, Information, PluginID, PluginParameter, HasCommand, ExecuteButtonName, Id))
                        CloseRequest?.Invoke(this, true);
                }
                else
                {
                    // Add Device
                    if (_databaseProvider.AddDeviceInfo(RoomId, DeviceTypeId, Name, Information, PluginID, PluginParameter, HasCommand, ExecuteButtonName)) 
                        CloseRequest?.Invoke(this, true);
                }
            }

        }



    }
}
