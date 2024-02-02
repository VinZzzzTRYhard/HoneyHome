using HoneyHome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Model
{
    public class PluginInfo : IPluginInfo
    {
        public PluginInfo(Int64? pluginId, string name, string path)
        {
            Id = pluginId;
            Name = name;
            PluginPath = path;
        }

        public Int64? Id { get; set; }

        public string Name { get; set; }

        public string PluginPath { get; set; }
    }
}
