using HoneyHome.BaseVM;
using HoneyHome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Model
{


    internal class Device : ViewModelBase, IDevice
    {
        public static Device None = new Device();

        public Device()
        {
            HasCommand = false;
        }

        public Int64? RoomId { get=>Get<int?>(); set=>Set(value); }
        public DeviceType DeviceTypeId { get=>Get<DeviceType>(); set=>Set(value); }
        public Int64 Id { get => Get<Int64>(); set => Set(value); }
        public string Name { get => Get<string>(); set => Set(value); }
        public string CurrentValue { get => Get<string>(); set => Set(value); }

        public string Information { get => Get<string>(); set { Set(value); } }

        public bool HasCommand { get => Get<bool>(); set => Set(value); }

        public Int64 PluginID { get => Get<Int64>(); set => Set(value); }
        public string PluginParameter { get=>Get<string>(); set => Set(value); }


        RelayCommand? _deviceExecuteCommand;
        public RelayCommand DeviceCommand
        {
            get
            {
                return _deviceExecuteCommand ?? (_deviceExecuteCommand = new RelayCommand(OnDeviceExecute, CanDeviceExecute));
            }
        }

        private bool CanDeviceExecute()
        {
            return true;
        }

        private void OnDeviceExecute()
        {
            DeviceExternalAction?.Invoke();
        }

        public string ExecuteButtonName { get => Get<string>(); set => Set(value); }

        public Action? DeviceExternalAction { get; set; }

    }
}
