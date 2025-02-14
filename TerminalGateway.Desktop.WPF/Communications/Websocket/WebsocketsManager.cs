using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Database;

namespace TerminalGateway.Desktop.WPF.Communications.Websocket
{
    public class WebsocketsManager
    {
        private DatabaseManager _databaseManager;
        private List<WebsocketConnectionModel> _websocketConnectionModels;
        private Dictionary<string, WebsocketManager> _websocketConnections;
        public bool IsConnected { get; private set; }

        public WebsocketsManager(DatabaseManager databaseManager) 
        {
            _databaseManager = databaseManager;
            _websocketConnections = new Dictionary<string, WebsocketManager>();
        }

        public void SyncWebsocketConnections()
        {
            IsConnected = false;
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
                        currentConnection.Connect();
                        _websocketConnections[model.LaneId] = currentConnection;
                        IsConnected = true;
                    }
                    if (currentConnection.ipAddress != model.IpAddress)
                    {
                        // ApiKey has changed, reconnect with new ApiKey
                        currentConnection.CloseWebsocket();
                        currentConnection = new WebsocketManager(model);
                        currentConnection.Connect();
                        _websocketConnections[model.LaneId] = currentConnection;
                        IsConnected = true;
                    }
                }
                else
                {
                    // If the connection does not exist, create and connect it
                    var newConnection = new WebsocketManager(model);
                    newConnection.Connect();
                    _websocketConnections[model.LaneId] = newConnection;
                    IsConnected = true;
                }
            }
        }
    }
}
