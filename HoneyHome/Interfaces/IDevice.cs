using HoneyHome.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Interfaces
{
    public enum DeviceType
    {
        Switch = 1,
        Temperature = 2,
        Weather = 3,
        Other
    }
    interface IDevice
    {
        Int64 Id { get; }
        Int64? RoomId { get; }
        DeviceType DeviceTypeId { get; }
        string Name { get; }
        string Information { get; }
        Int64 PluginID { get; }
        string PluginParameter { get; }
        bool HasCommand { get; }
        string ExecuteButtonName { get; }

    }
}
