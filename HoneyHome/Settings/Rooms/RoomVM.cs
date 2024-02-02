using HoneyHome.BaseVM;
using HoneyHome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Settings.Rooms
{
    internal class RoomVM : ViewModelBase, ICloseable
    {
        IDatabaseProvider? _databaseProvider;
        public RoomVM(IDatabaseProvider databaseProvider) { 
            _databaseProvider = databaseProvider;   
        }
        public string Name
        {
            get => Get<string>(); set
            {
                if (Set(value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public Int64? Id { get; set; }


        public event EventHandler<bool> CloseRequest;

        private RelayCommand? _saveCommand;
        public RelayCommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(OnSaveCommand, OnSaveCommandCanExecute));

        private bool OnSaveCommandCanExecute()
        {
            return !string.IsNullOrEmpty(Name);
        }

        private void OnSaveCommand()
        {
            if (_databaseProvider != null && !string.IsNullOrEmpty(Name))
            {
                if (Id.HasValue)
                {
                    // Update Room
                    if (_databaseProvider.UpdateRoom(Name, Id.Value))
                        CloseRequest?.Invoke(this, true);
                }
                else
                {
                    // Add Room
                    if (_databaseProvider.AddRoom(Name))
                        CloseRequest?.Invoke(this, true);
                }
            }            
        }
    }
}
