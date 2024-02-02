using HoneyHome.BaseVM;
using HoneyHome.Database;
using HoneyHome.Interfaces;
using HoneyHome.Model;
using HoneyHome.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome
{
    internal class MainWindowVM : ViewModelBase, ICloseable
    {
        private readonly List<Device> _devices = new List<Device>();
        private IDatabaseProvider _dataProvider;

        public MainWindowVM()
        {
            // Connect to DB
            string fileDb = GetDatabasePath("HoneyHome.hdb");
            _dataProvider = new HoneyDatabaseProvider();
            if (!_dataProvider.ConnectToDatabase(fileDb)) // Can't connect -> let's create
                _dataProvider.CreateDatabaseAndConnect(fileDb);

            UpdateRoomCollection();

            
            GetDevicesList();
            UpdateDeviceCollection(SelectedRoom.RoomId);
        }

        private string GetDatabasePath(string v)
        {
            var assemblyFolder = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(assemblyFolder))
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assemblyFolder), v);
            }
            return v;
        }

        private void GetDevicesList()
        {
            _devices.Clear();
            if (_dataProvider?.IsDatabaseConnected == true)
            {
                var devicesInfo = _dataProvider.GetDevices();
                foreach (var device in devicesInfo)
                {
                    _devices.Add(new Device()
                    {
                        Id = device.Id,
                        RoomId = device.RoomId,
                        DeviceTypeId = device.DeviceTypeId,
                        Name = device.Name,
                        Information = device.Information,
                        HasCommand = device.HasCommand,
                        ExecuteButtonName = device.ExecuteButtonName
                    });                    
                }
            }
        }


        // Source for Rooms
        public ObservableCollection<RoomsItems> RoomsCollection { get; private set; }

        // Current Selected Room
        public RoomsItems SelectedRoom
        {
            get =>Get<RoomsItems>();
            set
            {
                if (Set(value))
                    UpdateDeviceCollection(SelectedRoom.RoomId);
            }
        }

        public int SelectedTab { get=> Get<int>(); set => Set(value); }

        public IList<Device> SwitchSources { get=> Get<IList<Device>>(); set => Set(value);}
        public Device SelectedSwitch { get=>  Get<Device>(); set => Set(value);}

        public IList<Device> TempSources { get => Get<IList<Device>>(); set => Set(value); }
        public Device SelectedTemp { get => Get<Device>(); set => Set(value); }

        public IList<Device> WeatherSources { get => Get<IList<Device>>(); set => Set(value); }
        public Device SelectedWeather{ get => Get<Device>(); set => Set(value); }

        public IList<Device> OtherSources { get => Get<IList<Device>>(); set => Set(value); }
        public Device SelectedOther { get => Get<Device>(); set => Set(value); }

        private void UpdateRoomCollection()
        {
            var selectedRoom = new RoomsItems() { Name = "All rooms", RoomId = 0 };
            RoomsCollection = new ObservableCollection<RoomsItems>
            {
                selectedRoom
            };
            
            if (_dataProvider.IsDatabaseConnected)
            {
                var roomsList = _dataProvider.GetRooms();
                foreach (var room in roomsList)
                    RoomsCollection.Add( new RoomsItems() { Name = room.Name, RoomId = room.RoomId });
            }
            SelectedRoom = selectedRoom;
        }

        private void UpdateDeviceCollection(Int64 roomId)
        {
            var switchSources = new List<Device>();
            var tempSources = new List<Device>();
            var weatherSources = new List<Device>();
            var otherSources = new List<Device>();

            IEnumerable<Device> devicesFilterByRoom = _devices;
            if (roomId != 0)
                devicesFilterByRoom = _devices.Where(x=>x.RoomId == roomId);

            if (devicesFilterByRoom.Any())
                foreach (var device in devicesFilterByRoom)
                {
                    switch (device.DeviceTypeId)
                    {
                        case DeviceType.Switch:
                            {
                                switchSources.Add(device);
                            }
                            break;
                        case DeviceType.Temperature:
                            {
                                tempSources.Add(device);
                            }
                            break;
                        case DeviceType.Weather:
                            {
                                weatherSources.Add(device);
                            }
                            break;
                        default:
                            {
                                otherSources.Add(device);
                            }
                            break;
                    }
            }

            SwitchSources = switchSources;
            TempSources = tempSources;
            WeatherSources = weatherSources;
            OtherSources = otherSources;

            SelectedSwitch = SwitchSources.FirstOrDefault() ?? Device.None;
            SelectedTemp = TempSources.FirstOrDefault() ?? Device.None;
            SelectedWeather = WeatherSources.FirstOrDefault() ?? Device.None;  
            SelectedOther = OtherSources.FirstOrDefault() ?? Device.None;
        }

        internal void WindowClosing()
        {
            if (_dataProvider?.IsDatabaseConnected == true)
                _dataProvider.CloseDatabase();
        }

        // 
        RelayCommand? _settingsCommand;


        public RelayCommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand = new RelayCommand(OnSettinsCommand));
            }
        }

        private void OnSettinsCommand()
        {
            var settingsVM = new SettingsVM(_dataProvider);
            var dialog = new HoneyHome.Settings.Settings();
            dialog.Owner = App.Current.MainWindow;
            dialog.DataContext = settingsVM;
            dialog.ShowDialog();

            GetDevicesList();
            UpdateDeviceCollection(SelectedRoom.RoomId);

        }

        public event EventHandler<bool> CloseRequest;
        RelayCommand? _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(OnCloseCommand));

        private void OnCloseCommand()
        {
            CloseRequest?.Invoke(this, false);
        }
    }
}
