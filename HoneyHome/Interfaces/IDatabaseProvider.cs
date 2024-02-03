using System.Xml.Linq;

namespace HoneyHome.Interfaces
{
    interface IDatabaseProvider
    {
        bool IsDatabaseConnected { get; }

        bool ConnectToDatabase(string parameters);

        bool CreateDatabaseAndConnect(string parameters);
        void CloseDatabase();

        #region Rooms
        IList<IRoom> GetRooms();
        IRoom GetRoom(Int64 roomId);
        bool UpdateRoom(string name, Int64 id);
        bool AddRoom(string name);
        bool DeleteRoom(Int64 roomId);
        #endregion Rooms

        #region PluginInfo
        IList<IPluginInfo> GetPluginInfos();
        IPluginInfo GetPluginInfo(Int64 pluginId);
        bool AddPluginInfo(string name, string pluginPath);
        bool UpdatePluginInfo(string name, string pluginPath, Int64 pluginId);
        bool DeletePlugin(Int64 pluginId);
        #endregion PluginInfo

        #region Devices
        IList<IDevice> GetDevices();
        IDevice GetDevice(Int64 deviceId);
        bool AddDeviceInfo(Int64? roomId, DeviceType deviceTypeId, string name, string information, long pluginID, string pluginParameter, bool hasCommand, string executeButtonName);
        bool UpdateDeviceInfo(Int64? roomId, DeviceType deviceTypeId, string name, string information, long pluginID, string pluginParameter, bool hasCommand, string executeButtonName, long id);
        bool DeleteDeviceInfo(Int64 deviceId);
        bool AddDeviceValue(Int64 deviceId, string currentDeviceValue);
        #endregion Devices
    }
}
