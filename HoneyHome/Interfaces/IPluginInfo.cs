using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Interfaces
{
    interface IPluginInfo
    {
        Int64? Id { get; }
        string Name { get; }
        string PluginPath { get; }
    }
}
