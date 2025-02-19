using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Database;
using System.ComponentModel;

namespace TerminalGateway.Desktop.WPF.Communications.Websocket
{
    public class WebsocketsManager
    {
        private DatabaseManager _databaseManager;
        private List<WebsocketConnectionModel> _websocketConnectionModels;
        private Dictionary<string, WebsocketManager> _websocketConnections;
        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WebsocketsManager(DatabaseManager databaseManager) 
        {
            _databaseManager = databaseManager;
            _websocketConnections = new Dictionary<string, WebsocketManager>();
        }

        public void SyncWebsocketConnections()
        {
            IsConnected = false;  // start it off false each time
            _websocketConnectionModels = _databaseManager.GetAllWebsocketConnectionEntities();
            foreach (var model in _websocketConnectionModels)
            {
                if (_websocketConnections.ContainsKey(model.LaneId))
                {
                    // If the connection already exists, check if it needs to be updated
                    var currentConnection = _websocketConnections[model.LaneId];
                    if (currentConnection.apiKey != model.ApiKey)
                    {
                        // ApiKey has changed, reconnect with new ApiKey
                        currentConnection.CloseWebsocket();
                        currentConnection = new WebsocketManager(model);
                        currentConnection.ConnectionStateChanged += OnChildConnectionStateChanged;
                        currentConnection.Connect();
                        _websocketConnections[model.LaneId] = currentConnection;
                    }
                    if (currentConnection.ipAddress != model.IpAddress)
                    {
                        // ApiKey has changed, reconnect with new ApiKey
                        currentConnection.CloseWebsocket();
                        currentConnection = new WebsocketManager(model);
                        currentConnection.ConnectionStateChanged += OnChildConnectionStateChanged;
                        currentConnection.Connect();
                        _websocketConnections[model.LaneId] = currentConnection;
                    }
                }
                else
                {
                    // If the connection does not exist, create and connect it
                    var newConnection = new WebsocketManager(model);
                    // Subscribe to child's event
                    newConnection.ConnectionStateChanged += (sender, state) =>
                    {
                        // A single child changed, so let's re‐compute
                        var anyConnected = _websocketConnections.Values
                            .Any(child => child.isConnected);
                        IsConnected = anyConnected;
                    };
                    newConnection.ConnectionStateChanged += OnChildConnectionStateChanged;
                    newConnection.Connect();
                    _websocketConnections[model.LaneId] = newConnection;
                }
            }
            IsConnected = _websocketConnections.Values.Any(x => x.isConnected);
        }
        private void OnChildConnectionStateChanged(object sender, bool childIsConnected)
        {
            // Whenever one child changes, recalc aggregator’s overall status
            // “ANY” approach:
            IsConnected = _websocketConnections.Values.Any(c => c.isConnected);

            // Or “ALL” approach:
            // IsConnected = _websocketConnections.Values.All(c => c.isConnected);
        }
    }
}
