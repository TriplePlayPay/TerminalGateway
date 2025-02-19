using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Database;
using Serilog;

namespace TerminalGateway.Desktop.WPF
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //private DatabaseManager _databaseManager;

        private bool _isWebSocketConnected;
        public bool IsWebSocketConnected
        {
            get => _isWebSocketConnected;
            set
            {
                if (_isWebSocketConnected != value)
                {
                    _isWebSocketConnected = value;
                    // Raise the standard INotifyPropertyChanged event
                    OnPropertyChanged(nameof(IsWebSocketConnected));

                    // (Optional) Also raise WebsocketChanged if you like,
                    // but it's the OnPropertyChanged that's critical for UI binding.
                    OnWebsocketChanged(nameof(IsWebSocketConnected));
                }
            }
        }

        private string _apiKey;
        public string ApiKey
        {
            get => _apiKey;
            set { _apiKey = value; OnApiKeyChanged(); }
        }

        public event PropertyChangedEventHandler ApiKeyChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler WebsocketChanged;

        protected virtual void OnApiKeyChanged([CallerMemberName] string propertyName = null)
        {
            Log.Information("OnApiKeyChanged: " + propertyName);
            ApiKeyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnWebsocketChanged([CallerMemberName] string propertyName = null)
        {
            Log.Information("OnWebsocketChanged: " + propertyName);
            WebsocketChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Log.Information("OnPropertyChanged: " + propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<TerminalModel> _terminals;
        public ObservableCollection<TerminalModel> Terminals
        {
            get { return _terminals; }
            set { _terminals = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {

        }
    }
}
