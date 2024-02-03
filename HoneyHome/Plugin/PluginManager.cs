using HoneyHome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PluginSDK;
using System.Diagnostics;

namespace HoneyHome.Plugin
{
    internal class PluginManager
    {
        IDatabaseProvider _databaseProvider;
        readonly Dictionary<Int64, string> _checkedPlugins = new Dictionary<long, string>();
        public PluginManager(IDatabaseProvider? databaseProvider) 
        {
            if (databaseProvider?.IsDatabaseConnected != true)
                throw new ArgumentNullException(nameof(databaseProvider));

            _databaseProvider = databaseProvider;
        }

        internal void InitializePlugins()
        {
            if (!_databaseProvider.IsDatabaseConnected)
                return;

            _checkedPlugins.Clear();
            try
            {
                // InitializePluginsAsync();
                // We now use sync model of plugin initialization because in async model we need to wait for complete 
                // of plugin initialize before Device initialization
                InitializePluginsImpl();
            }
            catch { }           
        }

        private async void InitializePluginsAsync()
        {
            var result = await InitializePluginsAwaited();
        }

        public bool IsInitCompleted { get; private set; } = false;


        private bool InitializePluginsImpl()
        {
            var pluginsToInitialize = _databaseProvider.GetPluginInfos();
            foreach (var pluginInfo in pluginsToInitialize)
            {
                var pluginPath = pluginInfo.PluginPath;
                if (!System.IO.Path.IsPathRooted(pluginPath))
                {
                    // Make Full path
                    pluginPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), pluginPath);
                }
                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(pluginPath);
                    Type[] pluginAssemblyTypesList = null;
                    try
                    {
                        pluginAssemblyTypesList = pluginAssembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                    }
                    if (pluginAssemblyTypesList?.Any() == true)
                        foreach (Type assemblyType in pluginAssemblyTypesList)
                        {
                            if (assemblyType?.IsClass == true && typeof(IPluginSDK).IsAssignableFrom(assemblyType))
                            {
                                IPluginSDK currentPlugin = pluginAssembly.CreateInstance(assemblyType.FullName) as IPluginSDK;
                                if (currentPlugin != null && pluginInfo.Id != null)
                                {
                                    _checkedPlugins.Add(pluginInfo.Id.Value, pluginPath);
                                }
                            }
                        }
                }
                catch (Exception ex)
                {

                }
            }

            IsInitCompleted = true;

            return false;
        }
        private Task<bool> InitializePluginsAwaited()
        {
            return Task.Run(() =>
            {
                return InitializePluginsImpl();
            });
        }

        Dictionary<Int64, IPluginSDK> _devicesDict = new Dictionary<Int64, IPluginSDK>();
        internal void DisposePluginForDevice(Int64 deviceId)
        {
            if (_devicesDict.TryGetValue(deviceId, out var pluginSDK))
            {
                pluginSDK.Dispose();
                _devicesDict.Remove(deviceId);
            }
        }

        internal bool InitializeDevice(long deviceId, long pluginID, string pluginParameter)
        {
            try
            {
                if (!_checkedPlugins.TryGetValue(pluginID, out var pluginPath))
                    throw new ArgumentException($"Verified plugin can't be founded for device {deviceId}");

                Assembly pluginAssembly = Assembly.LoadFrom(pluginPath);
                Type[] pluginAssemblyTypesList = null;
                try
                {
                    pluginAssemblyTypesList = pluginAssembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                }
                bool foundAndInitialized = false;
                if (pluginAssemblyTypesList?.Any() == true)
                    foreach (Type assemblyType in pluginAssemblyTypesList)
                    {
                        if (assemblyType?.IsClass == true && typeof(IPluginSDK).IsAssignableFrom(assemblyType))
                        {
                            IPluginSDK currentPlugin = pluginAssembly.CreateInstance(assemblyType.FullName) as IPluginSDK;
                            if (currentPlugin != null)
                            {
                                if (currentPlugin.InitializePlugin(pluginParameter))
                                {
                                    _devicesDict[deviceId] = currentPlugin;
                                    foundAndInitialized = true;
                                    break;
                                }
                                else
                                    throw new ArgumentException($"Verified plugin can't be initialized for device {deviceId}");
                            }
                        }
                    }
                if (!foundAndInitialized)
                    throw new ArgumentException($"Verified plugin can't be initialized (no interface support) for device {deviceId}");

            }
            catch (Exception ex) {
                Debug.Fail($"Can't initialize plugin because {ex}");
                return false;
            }
            return true;
        }

        internal string GetCurrentValue(Int64 deviceId)
        {
            if (_devicesDict.TryGetValue(deviceId, out var pluginSDK))
            {
                if (pluginSDK.IsInitialized)
                    return pluginSDK.GetCurrentValue();
            }
            return string.Empty;
        }

        internal void MakePluginAction(Int64 deviceId, string parameters = "")
        {
            if (_devicesDict.TryGetValue(deviceId, out var pluginSDK))
            {
                if (pluginSDK.IsInitialized)
                    pluginSDK.MakePluginAction(parameters);
            }
        }

    }
}
