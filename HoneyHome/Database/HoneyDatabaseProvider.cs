using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Data.Common;
using HoneyHome.Interfaces;
using System.Xml.Linq;

namespace HoneyHome.Database
{
    internal class HoneyDatabaseProvider : IDatabaseProvider, IDisposable
    {
        private SQLiteConnection? _connection = null;
        private bool disposedValue;

        public HoneyDatabaseProvider()
        {

        }



        public bool IsDatabaseConnected => _connection != null;

        public void CloseDatabase()
        {
            if (IsDatabaseConnected)
                _connection?.Close();
        }

        public bool ConnectToDatabase(string parameters)
        {

            if (!System.IO.File.Exists(parameters))
                return false;

            var cs = $"Data Source={parameters}";

            if (IsDatabaseConnected)
                CloseDatabase();

            try
            {
                _connection = new SQLiteConnection(cs);
                _connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                _connection?.Close();
                _connection = null;
            }

            return false;
        }


        public bool CreateDatabaseAndConnect(string parameters)
        {
            var cs = $"Data Source={parameters}";

            if (IsDatabaseConnected)
                CloseDatabase();

            try
            {
                _connection = new SQLiteConnection(cs);
                _connection.Open();
                // Create schema
                ///
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "HoneyHome.Resources.CreateSQL.sql";

                string createCmd = string.Empty;
                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader? reader = new StreamReader(stream))
                {
                    createCmd = reader.ReadToEnd();
                    using (var cmd = new SQLiteCommand(createCmd, _connection))
                        cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                _connection?.Close();
                _connection = null;
            }


            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    CloseDatabase();
                    _connection?.Dispose();
                    _connection = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HoneyDatabaseProvider()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #region Room
        class Room : IRoom
        {
            public Room(string name, Int64 roomId)
            {
                Name = name;
                RoomId = roomId;
            }
            public string Name { get; }

            public Int64 RoomId { get; }
        }

        public IList<IRoom> GetRooms()
        {
            List<IRoom> retList = new List<IRoom>();
            if (IsDatabaseConnected)
            {
                try
                {
                    var selectRooms = "SELECT ID, Name FROM Rooms";
                    using (var cmd = new SQLiteCommand(selectRooms, _connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                                retList.Add(new Room(reader.GetString(1), reader.GetInt64(0)));
                    }
                }
                catch { }
            }
            return retList;
        }
        public IRoom GetRoom(Int64 roomId)
        {
            IRoom room = new Room(string.Empty, 0);
            if (IsDatabaseConnected)
            {
                try
                {
                    var selectRooms = $"SELECT Name FROM Rooms WHERE ID = {roomId}";
                    using (var cmd = new SQLiteCommand(selectRooms, _connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                            room = new Room(reader.GetString(0), roomId);
                    }
                }
                catch { }
            }

            return room;

        }
        public bool UpdateRoom(string name, long id)
        {
            bool ret = false;
            if (IsDatabaseConnected)
            {
                try
                {
                    var selectRooms = $"UPDATE Rooms Set Name = ? WHERE Id = {id}";
                    using (var cmd = new SQLiteCommand(selectRooms, _connection))
                    {
                        cmd.Parameters.Add(new SQLiteParameter() { Value = name });
                        return cmd.ExecuteNonQuery() != 0;
                    }
                }
                catch { }
            }

            return ret;
        }

        public bool AddRoom(string name)
        {
            bool ret = false;

            if (IsDatabaseConnected)
            {
                try
                {
                    var selectRooms = $"INSERT INTO Rooms(Name) VALUES(?)";
                    using (var cmd = new SQLiteCommand(selectRooms, _connection))
                    {
                        cmd.Parameters.Add(new SQLiteParameter() { Value = name });
                        return cmd.ExecuteNonQuery() != 0;
                    }
                }
                catch { }
            }
            return ret;
        }
        public bool DeleteRoom(Int64 roomId)
        {
            bool ret = false;

            if (IsDatabaseConnected)
            {
                try
                {
                    var selectRooms = $"DELETE FROM Rooms WHERE ID = {roomId}";
                    using (var cmd = new SQLiteCommand(selectRooms, _connection))
                        return cmd.ExecuteNonQuery() != 0;
                }
                catch { }
            }
            return ret;
        }
        #endregion Room

        #region Plugin Info
        class PluginInfo : IPluginInfo
        {
            public PluginInfo(Int64 pluginId, string name, string path)
            {
                Id = pluginId;
                Name = name;
                PluginPath = path;
            }

            public long? Id { get; }

            public string Name { get; }

            public string PluginPath { get; }
        }

        public IList<IPluginInfo> GetPluginInfos()
        {
            List<IPluginInfo> retList = new List<IPluginInfo>();
            if (IsDatabaseConnected)
            {
                try
                {
                    var selectPlugins = "SELECT ID, Name, Path FROM Plugins";
                    using (var cmd = new SQLiteCommand(selectPlugins, _connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                                retList.Add(new PluginInfo(reader.GetInt64(0), reader.GetString(1), reader.GetString(2)));
                    }
                }
                catch { }
            }
            return retList;
        }
        public IPluginInfo GetPluginInfo(Int64 pluginId)
        {
            IPluginInfo pluginInfo = new PluginInfo(0, string.Empty, string.Empty);
            if (IsDatabaseConnected)
            {
                try
                {
                    var selectPlugin = $"SELECT Name, Path FROM Plugins WHERE ID = {pluginId}";
                    using (var cmd = new SQLiteCommand(selectPlugin, _connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                            pluginInfo = new PluginInfo(pluginId, reader.GetString(0), reader.GetString(1));
                    }
                }
                catch { }
            }

            return pluginInfo;

        }

        public bool AddPluginInfo(string name, string pluginPath)
        {
            bool ret = false;
            if (IsDatabaseConnected)
            {
                try
                {
                    var insertPluginInfo = $"INSERT INTO Plugins(Name, Path) VALUES(?,?)";
                    using (var cmd = new SQLiteCommand(insertPluginInfo, _connection))
                    {
                        cmd.Parameters.Add(new SQLiteParameter() { Value = name });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = pluginPath });
                        return cmd.ExecuteNonQuery() != 0;
                    }
                }
                catch { }
            }
            return ret;
        }
        public bool UpdatePluginInfo(string name, string pluginPath, Int64 pluginId)
        {
            bool ret = false;
            if (IsDatabaseConnected)
            {
                try
                {
                    var updatePluginInfo = $"UPDATE Plugins Set Name = ?, Path = ? WHERE Id = {pluginId}";
                    using (var cmd = new SQLiteCommand(updatePluginInfo, _connection))
                    {
                        cmd.Parameters.Add(new SQLiteParameter() { Value = name });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = pluginPath });
                        return cmd.ExecuteNonQuery() != 0;
                    }
                }
                catch { }
            }

            return ret;
        }

        public bool DeletePlugin(Int64 pluginId)
        {
            bool ret = false;

            if (IsDatabaseConnected)
            {
                try
                {
                    var delPlugin = $"DELETE FROM Plugins WHERE ID = {pluginId}";
                    using (var cmd = new SQLiteCommand(delPlugin, _connection))
                        return cmd.ExecuteNonQuery() != 0;
                }
                catch { }
            }
            return ret;
        }

        #endregion Plugin Info

        #region Device Info
        class DeviceInfo : IDevice
        {
            public long Id { get; set; }

            public long? RoomId { get; set; }

            public DeviceType DeviceTypeId { get; set; }

            public string Name { get; set; }

            public string Information { get; set; }

            public long PluginID { get; set; }

            public string PluginParameter { get; set; }

            public bool HasCommand { get; set; }

            public string ExecuteButtonName { get; set; }
        }


        public IList<IDevice> GetDevices()
        {
            List<IDevice> retList = new List<IDevice>();
            if (IsDatabaseConnected)
            {
                try
                {
                    var selectDevices = "SELECT ID, RoomID, DeviceTypeId, Name, Information, PluginID, PluginParameter, HasCommand, ExecuteButtonName FROM Devices";
                    using (var cmd = new SQLiteCommand(selectDevices, _connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                            {
                                var device = new DeviceInfo()
                                {
                                    Id = reader.GetInt64(0),
                                    RoomId = !reader.IsDBNull(1) ? reader.GetInt64(1) : null,
                                    DeviceTypeId = (DeviceType)reader.GetInt32(2),
                                    Name = reader.GetString(3),
                                    Information = reader.GetString(4),
                                    PluginID = reader.GetInt64(5),
                                    PluginParameter = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                    HasCommand = reader.GetBoolean(7),
                                    ExecuteButtonName = !reader.IsDBNull(8) ? reader.GetString(8) : string.Empty,
                                };
                                retList.Add(device);
                            }
                    }
                }
                catch { }
            }
            return retList;
        }

        public IDevice GetDevice(Int64 deviceId)
        {
            IDevice deviceInfo = new DeviceInfo() { Id = 0, Name = string.Empty, Information = string.Empty };
            if (IsDatabaseConnected)
            {
                try
                {
                    var selectDevices = $"SELECT ID, RoomID, DeviceTypeId, Name, Information, PluginID, PluginParameter, HasCommand, ExecuteButtonName FROM Devices WHERE ID = {deviceId}";
                    using (var cmd = new SQLiteCommand(selectDevices, _connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            deviceInfo = new DeviceInfo()
                            {
                                Id = reader.GetInt64(0),
                                RoomId = !reader.IsDBNull(1) ? reader.GetInt64(1) : null,
                                DeviceTypeId = (DeviceType)reader.GetInt32(2),
                                Name = reader.GetString(3),
                                Information = reader.GetString(4),
                                PluginID = reader.GetInt64(5),
                                PluginParameter = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                HasCommand = reader.GetBoolean(7),
                                ExecuteButtonName = !reader.IsDBNull(8) ? reader.GetString(8) : string.Empty,
                            };
                        }
                    }
                }
                catch { }
            }

            return deviceInfo;
        }
        public bool AddDeviceInfo(long? roomId, DeviceType deviceTypeId, string name, string information, long pluginID, string pluginParameter, bool hasCommand, string executeButtonName)
        {
            bool ret = false;
            if (IsDatabaseConnected)
            {
                try
                {
                    var insertPluginInfo = $"INSERT INTO Devices(RoomID, DeviceTypeId, Name, Information, PluginID, PluginParameter, HasCommand, ExecuteButtonName) VALUES(?,?,?,?,?,?,?,?)";
                    using (var cmd = new SQLiteCommand(insertPluginInfo, _connection))
                    {
                        cmd.Parameters.Add(new SQLiteParameter() { Value = roomId });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = (int)deviceTypeId });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = name });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = information });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = pluginID });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = pluginParameter });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = hasCommand });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = executeButtonName });
                        return cmd.ExecuteNonQuery() != 0;
                    }
                }
                catch { }
            }
            return ret;
        }
        public bool UpdateDeviceInfo(long? roomId, DeviceType deviceTypeId, string name, string information, long pluginID, string pluginParameter, bool hasCommand, string executeButtonName, long deviceId)
        {
            bool ret = false;
            if (IsDatabaseConnected)
            {
                try
                {
                    var updatePluginInfo = $"UPDATE Devices " +
                        $"Set RoomID = ?, " +
                        $"DeviceTypeId = ?, " +
                        $"Name = ?, " +
                        $"Information = ?, " +
                        $"PluginID = ?, " +
                        $"PluginParameter = ?, " +
                        $"HasCommand = ?, " +
                        $"ExecuteButtonName = ? " +
                        $"WHERE Id = {deviceId}";
                    using (var cmd = new SQLiteCommand(updatePluginInfo, _connection))
                    {
                        cmd.Parameters.Add(new SQLiteParameter() { Value = roomId });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = (int)deviceTypeId });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = name });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = information });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = pluginID });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = pluginParameter });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = hasCommand });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = executeButtonName });
                        return cmd.ExecuteNonQuery() != 0;
                    }
                }
                catch { }
            }

            return ret;
        }

        public bool DeleteDeviceInfo(Int64 deviceId)
        {
            bool ret = false;

            if (IsDatabaseConnected)
            {
                try
                {
                    var delDevice = $"DELETE FROM Devices WHERE ID = {deviceId}";
                    using (var cmd = new SQLiteCommand(delDevice, _connection))
                        return cmd.ExecuteNonQuery() != 0;
                }
                catch { }
            }
            return ret;

        }
        public bool AddDeviceValue(Int64 deviceId, string currentDeviceValue)
        {
            bool ret = false;
            if (IsDatabaseConnected)
            {
                try
                {
                    var insertPluginInfo = $"INSERT INTO DeviceValues(DeviceID, DeviceValue) VALUES(?,?)";
                    using (var cmd = new SQLiteCommand(insertPluginInfo, _connection))
                    {
                        cmd.Parameters.Add(new SQLiteParameter() { Value = deviceId });
                        cmd.Parameters.Add(new SQLiteParameter() { Value = currentDeviceValue });
                        return cmd.ExecuteNonQuery() != 0;
                    }
                }
                catch { }
            }

            return ret;
        }
        #endregion Device Info
    }
}
