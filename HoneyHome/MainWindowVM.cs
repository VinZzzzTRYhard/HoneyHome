using HoneyHome.BaseVM;
using HoneyHome.Database;
using HoneyHome.Device;
using HoneyHome.Interfaces;
using HoneyHome.Model;
using HoneyHome.Plot;
using HoneyHome.Plugin;
using HoneyHome.Settings;
using System.Collections.ObjectModel;

namespace HoneyHome
{
    internal class MainWindowVM : ViewModelBase, ICloseable
    {
        private IDatabaseProvider _dataProvider;
        private PluginManager _pluginManager;
        private DeviceManager _deviceManager;
        public MainWindowVM()
        {
            // Connect to DB
            string fileDb = GetDatabasePath("HoneyHome.hdb");
            _dataProvider = new HoneyDatabaseProvider();
            if (!_dataProvider.ConnectToDatabase(fileDb)) // Can't connect -> let's create
                _dataProvider.CreateDatabaseAndConnect(fileDb);

            if (_dataProvider?.IsDatabaseConnected != true)
            {
                // Can't init database
            }

            _pluginManager = new PluginManager(_dataProvider);
            _pluginManager.InitializePlugins();

            _deviceManager = new DeviceManager(_dataProvider, _pluginManager);
            _deviceManager.InitializeDevices();


            UpdateRoomCollection();
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

        public IList<Model.Device> SwitchSources { get=> Get<IList<Model.Device>>(); set => Set(value);}
        public Model.Device SelectedSwitch { get=>  Get<Model.Device>(); set => Set(value);}

        public IList<Model.Device> TempSources { get => Get<IList<Model.Device>>(); set => Set(value); }
        public Model.Device SelectedTemp { get => Get<Model.Device>(); set => Set(value); }

        public IList<Model.Device> WeatherSources { get => Get<IList<Model.Device>>(); set => Set(value); }
        public Model.Device SelectedWeather{ get => Get<Model.Device>(); set => Set(value); }

        public IList<Model.Device> OtherSources { get => Get<IList<Model.Device>>(); set => Set(value); }
        public Model.Device SelectedOther { get => Get<Model.Device>(); set => Set(value); }

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
            if (_deviceManager?.Devices == null)
                return; // Not fully initialized

            var switchSources = new List<Model.Device>();
            var tempSources = new List<Model.Device>();
            var weatherSources = new List<Model.Device>();
            var otherSources = new List<Model.Device>();

            IEnumerable<Model.Device> devicesFilterByRoom = _deviceManager.Devices;
            if (roomId != 0)
                devicesFilterByRoom = _deviceManager.Devices.Where(x=>x.RoomId == roomId);

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

            SelectedSwitch = SwitchSources.FirstOrDefault() ?? Model.Device.None;
            SelectedTemp = TempSources.FirstOrDefault() ?? Model.Device.None;
            SelectedWeather = WeatherSources.FirstOrDefault() ?? Model.Device.None;  
            SelectedOther = OtherSources.FirstOrDefault() ?? Model.Device.None;
        }

        internal void WindowClosing()
        {
            _deviceManager?.DisposeDevices();

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

            _deviceManager.InitializeDevices();

            UpdateDeviceCollection(SelectedRoom.RoomId);

        }

        public event EventHandler<bool> CloseRequest;
        RelayCommand? _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(OnCloseCommand));

        private void OnCloseCommand()
        {
            CloseRequest?.Invoke(this, false);
        }

        RelayCommand? _showGraph;
        public RelayCommand ShowGraph => _showGraph ?? (_showGraph = new RelayCommand(OnShowGrapExecute));

        private void OnShowGrapExecute()
        {
            IDevice selectedDevice = null;
            Int64 deviceId = 0;
            string Title = string.Empty;
            switch (SelectedTab)
            {
                case 0:
                    selectedDevice  = SelectedSwitch; break;
                case 1:
                    selectedDevice =  SelectedTemp; break;
                case 2:
                    selectedDevice = SelectedWeather; break;
                case 3:
                    selectedDevice = SelectedOther; break;
            }
            if ((selectedDevice?.Id ?? 0) > 0)
            {
                var plotVm = new PlotVM(_dataProvider, selectedDevice.Id);
                plotVm.Title = $"{selectedDevice.Information} at {DateTime.UtcNow.ToString("D")}";
                var plotDlg = new Plot.Plot();
                plotDlg.DataContext = plotVm;
                plotDlg.ShowDialog();
            }

        }
    }
}
