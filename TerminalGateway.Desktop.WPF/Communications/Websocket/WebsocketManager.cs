using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Text.Json;
using TerminalGateway.Desktop.WPF.Communications.Terminal;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.NewPos;
using WebSocketSharp;
using System.Diagnostics;
using System.Security.Authentication;
using Serilog;

namespace TerminalGateway.Desktop.WPF.Communications.Websocket
{
    public class WebsocketManager
    {
        public string apiKey { get; private set; }
        public string ipAddress { get; private set; }
        public string laneId { get; private set; }

        private WebSocket _webSocket;
        private int _reconnectDelay = 1000;

        public bool isConnected { get; private set; }

        public event EventHandler<bool> ConnectionStateChanged;

        private System.Timers.Timer _heartbeatTimer;
        private const int HEARTBEAT_INTERVAL_MS = 15000; // 15 seconds, pick your interval

        private WebsocketConnectionModel _websocketConnectionModel;

        public WebsocketManager(WebsocketConnectionModel websocketConnectionModel)
        {
            _websocketConnectionModel = websocketConnectionModel;
            apiKey = websocketConnectionModel.ApiKey;
            ipAddress = websocketConnectionModel.IpAddress;
            laneId = websocketConnectionModel.LaneId;
            isConnected = false;
        }

        public void Connect()
        {
            // If an old WebSocket exists, close/cleanup it first (optional, but safer)
            if (_webSocket != null)
            {
                _webSocket.OnClose -= WebSocket_OnClose;
                _webSocket.OnError -= WebSocket_OnError;
                _webSocket.OnMessage -= WebSocket_OnMessage;
                _webSocket.OnOpen -= WebSocket_OnOpen;
                _webSocket.Close();
            }

            _webSocket = new WebSocket("wss://tripleplaypay.network/api/terminal-gateway");
            _webSocket.OnMessage += WebSocket_OnMessage;

            // THIS IS CRITICAL: Actually subscribe to OnClose
            _webSocket.OnClose += WebSocket_OnClose;

            // Optional: Subscribe to OnError
            _webSocket.OnError += WebSocket_OnError;

            // on open emit the proper event
            _webSocket.OnOpen += WebSocket_OnOpen;

            // Logging config
            _webSocket.Log.Level = LogLevel.Trace;
            _webSocket.Log.Output = (logData, message) =>
            {
                Log.Debug($"[{logData.Level}] {logData.Message}");
            };
            _webSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;

            // Reset reconnect delay in case this is a successful new connect
            _reconnectDelay = 1000;

            _webSocket.Connect(); // Synchronous connect in WebSocketSharp
            isConnected = _webSocket.IsAlive;

            if (isConnected)
            {
                StartHeartbeat(); // start sending periodic heartbeats
            }
            else
            {
                // Optionally do something if Connect failed
                Log.Warning("WebSocket failed to connect.");
            }
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            // reset backoff
            _reconnectDelay = 1000;
            isConnected = true;
            ConnectionStateChanged?.Invoke(this, true);
            SendAuth();
            StartHeartbeat();
        }

        private void StartHeartbeat()
        {
            StopHeartbeat(); // safety in case it was already running

            _heartbeatTimer = new System.Timers.Timer(HEARTBEAT_INTERVAL_MS);
            _heartbeatTimer.Elapsed += (s, e) => SendHeartbeat();
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();
        }

        private void StopHeartbeat()
        {
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Stop();
                _heartbeatTimer.Elapsed -= (s, e) => SendHeartbeat();
                _heartbeatTimer.Dispose();
                _heartbeatTimer = null;
            }
        }

        private void SendHeartbeat()
        {
            if (_webSocket != null && _webSocket.IsAlive)
            {
                var heartbeat = new
                {
                    Type = "HEARTBEAT",
                    ApiKey = apiKey,
                    LaneId = laneId,
                    // Any other data you want
                };
                string heartbeatJson = JsonSerializer.Serialize(heartbeat);
                _webSocket.Send(Encoding.UTF8.GetBytes(heartbeatJson));
                Log.Debug("Sent heartbeat.");
            }
        }   

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            Log.Information($"WebSocket closed: {e.Reason}");
            isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);

            // Stop sending heartbeats while we’re disconnected
            StopHeartbeat();

            // If not intentional, schedule a reconnect
            Log.Information($"Reconnecting in {_reconnectDelay / 1000} seconds...");
            var delay = _reconnectDelay;
            Task.Run(async () =>
            {
                await Task.Delay(delay);
                Connect();
                // Exponential backoff
                _reconnectDelay = Math.Min(_reconnectDelay * 2, 60000);
            });
        }

        private void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Log.Error($"WebSocket Error: {e.Message}");
            // optional: if not connected, you might handle reconnect here
            // but typically OnClose triggers the main reconnect logic
        }

        private WebsocketTransmissionChargeModel CallTerminal(TerminalMessageModel terminalMessageModel)
        {
            try
            {
                var terminalResult = TerminalTransmitter.TransmitToTerminal(terminalMessageModel);
                Log.Information("Terminal successfully called with Request Id: " + terminalMessageModel.RequestId);
                return new WebsocketTransmissionChargeModel()
                {
                    Status = true,
                    Details = terminalResult,
                    Message = "",
                    RequestId = terminalMessageModel.RequestId,
                    ApiKey = apiKey,
                    LaneId = terminalMessageModel.LaneId
                };
            }
            catch (Exception ex)
            {
                Log.Error("Error with calling terminal: " + ex.Message);
                return new WebsocketTransmissionChargeModel()
                {
                    Status = false,
                    Details = null,
                    Message = ex.Message,
                    RequestId = terminalMessageModel.RequestId,
                    ApiKey = apiKey,
                    LaneId = terminalMessageModel.LaneId
                };
            }
        }

        /// <summary>
        /// If this succeeds it will send off a body to the third party websocket that looks like: 
        /// {[STX]0[FS]T01[FS]1.54[FS]000000[FS]OK[FS]00[US]APPROVAL[US]611529[US]410813655803[US][US]3[US]MCC0000135180417[US][US][FS]01[FS]15000[US]0[US]0[US]0[US]0[US]0[US][US][US][US][US][US]0[US]0[US]0[FS]4111[US]4[US]1225[US][US][US][US]02[US]UAT USA/Test Card 07      [US][US][US]0[US][US][US][US][US][US][FS]1[US]163135[US]20240417141656[US][US][US][US][US][FS]0[US]AVS Not Requested.[US][US][US][FS][FS][FS][US]EDCTYPE=CREDIT[US]HRef=200069123678[US]CARDBIN=541333[US]PROGRAMTYPE=0[US]SN=1240334673[US]PINSTATUSNUM=0[US]GLOBALUID=1240334673202404171416564204[US]ARQC=93198522979CE040[US]TVR=0400088000[US]AID=A0000000041010[US]TSI=E800[US]ATC=0025[US]APPLAB=MASTERCARD[US]APPPN=Mastercard[US]IAD=0110200009620000000000000000000000FF[US]CID=00[US]CVM=6[US]userLanguageStatus=1[US][FS][FS][FS][FS][ETX]_}
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            // Handle inbound message
            TerminalMessageModel terminalMessageModel = WebsocketSerializer.DeserializeTerminalMessageModel(e.Data, _websocketConnectionModel);
            Log.Information("Message Received: " + e.Data);
            // Because we're creating one websocket per instance of the terminal, we need to 
            // double check that the laneId value passed to us from the websocket is equal to this
            // uniquely valued laneId.
            if (terminalMessageModel.LaneId == laneId)
            {
                if (terminalMessageModel.Action == "charge" && terminalMessageModel.TerminalType == "PAX")
                {
                    try
                    {
                        WebsocketTransmissionChargeModel websocketTransmissionChargeModel = CallTerminal(terminalMessageModel);
                        var websocketChargeString = JsonSerializer.Serialize(websocketTransmissionChargeModel);
                        var encodedChargeString = Encoding.UTF8.GetBytes(websocketChargeString);
                        _webSocket.Send(encodedChargeString);
                    } catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }
                if (terminalMessageModel.Action == "charge" && terminalMessageModel.TerminalType == "NEWPOS")
                {
                    try
                    {
                        var tppDeviceInput = new TPPDeviceCallerInput
                        {
                            Host = ipAddress,
                            Port = 1419,
                            MerchantKey = apiKey,
                            PaymentType = terminalMessageModel.TerminalPaymentType
                        };
                        var result = TPPDeviceCaller.SendTerminalMessage(tppDeviceInput);
                        WebsocketTransmissionChargeModel websocketTransmissionChargeModel = new WebsocketTransmissionChargeModel
                        {
                            Status = true,
                            Message = "",
                            Details = result,
                            RequestId = terminalMessageModel.RequestId,
                            ApiKey = apiKey,
                            LaneId = terminalMessageModel.LaneId
                        };

                        var websocketChargeString = JsonSerializer.Serialize(websocketTransmissionChargeModel);
                        var encodedChargeString = Encoding.UTF8.GetBytes(websocketChargeString);
                        _webSocket.Send(encodedChargeString);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }
                else
                {
                    Log.Error("Unsupported payment method called: " + terminalMessageModel.Action);
                    var websocketErrorModel = new WebsocketTransmissionChargeModel()
                    {
                        Status = false,
                        Details = null,
                        Message = "The payment type passed in is unsupported at this time.",
                        RequestId = terminalMessageModel.RequestId,
                        ApiKey = apiKey,
                    };
                    var websocketChargeString = JsonSerializer.Serialize(websocketErrorModel);
                    var encodedChargeString = Encoding.UTF8.GetBytes(websocketChargeString);
                    _webSocket.Send(encodedChargeString);
                }
            }
        }

        private void SendAuth()
        {
            var authModel = new WebsocketTransmissionAuthModel
            {
                Type = "AUTH",
                ApiKey = apiKey,
                LaneId = laneId,
            };
            var websocketConnectionString = JsonSerializer.Serialize(authModel);
            _webSocket.Send(Encoding.UTF8.GetBytes(websocketConnectionString));
            Log.Debug("Sent AUTH message.");
        }

        public void CloseWebsocket()
        {
            isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);
            Log.Warning("Websocket Closing");
            _webSocket.Close();
            isConnected = false;
        }
    }
}
