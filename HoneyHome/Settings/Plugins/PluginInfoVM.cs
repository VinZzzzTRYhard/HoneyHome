using HoneyHome.BaseVM;
using HoneyHome.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Settings.Plugins
{
    internal class PluginInfoVM : ViewModelBase, ICloseable
    {
        IDatabaseProvider? _databaseProvider;
        public PluginInfoVM(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }


        public event EventHandler<bool> CloseRequest;
        RelayCommand? _closeCommand;
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(OnCloseCommand));

        private void OnCloseCommand()
        {
            CloseRequest?.Invoke(this, false);
        }


        public Int64? Id { get; set; }
        public string Name
        {
            get => Get<string>();
            set
            {
                if (Set(value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string PluginPath
        {
            get => Get<string>();
            set
            {
                if (Set(value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private RelayCommand? _saveCommand;
        public RelayCommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(OnSaveCommand, OnSaveCommandCanExecute));

        private bool OnSaveCommandCanExecute()
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(PluginPath);
        }

        private void OnSaveCommand()
        {
            if (_databaseProvider != null && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(PluginPath))
            {
                if (Id.HasValue)
                {
                    // Update Room
                    if (_databaseProvider.UpdatePluginInfo(Name, PluginPath, Id.Value))
                        CloseRequest?.Invoke(this, true);
                }
                else
                {
                    // Add PluginInfo
                    if (_databaseProvider.AddPluginInfo(Name, PluginPath))
                        CloseRequest?.Invoke(this, true);
                }
            }

        }


        private RelayCommand? _browseCommand;
        public RelayCommand BrowseCommand => _browseCommand ?? (_browseCommand = new RelayCommand(OnBrowseCommand));

        private void OnBrowseCommand()
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Honey Home plugins (*.dll)|*.dll";
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = ".dll";
            openFileDialog.InitialDirectory = GetPluginPath();
            if (openFileDialog.ShowDialog() == true)
            {
                // Check plugin for support IPlugin ineterface
                PluginPath = GetPathWithoutExeFolder(openFileDialog.FileName);

            }
        }

        private string GetPathWithoutExeFolder(string fileName)
        {
            var assemblyFolder = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(assemblyFolder))
            {
                var directory = System.IO.Path.GetDirectoryName(assemblyFolder);
                if (!string.IsNullOrEmpty(directory))
                {

                    if (fileName.Contains(directory))
                    {
                        return fileName.Substring(directory.Length+1);
                    }
                }
            }
            return fileName;
        }

        private string GetPluginPath()
        {
            var assemblyFolder = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(assemblyFolder))
            {
                var pluginPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assemblyFolder), "plugins");
                if (!System.IO.File.Exists(pluginPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(pluginPath);
                    }
                    catch 
                    {
                    }
                }
                return pluginPath;
            }
            return string.Empty;
        }
    }
}
