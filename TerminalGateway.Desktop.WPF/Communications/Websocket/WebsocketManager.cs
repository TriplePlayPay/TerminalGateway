using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using TerminalGateway.Desktop.WPF.Communications.Terminal;
using TerminalGateway.Desktop.WPF.Communications.Models;
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
            _webSocket = new WebSocket("wss://sandbox.tripleplaypay.com/pax");
            _webSocket.OnMessage += WebSocket_OnMessage;
            _webSocket.Log.Level = LogLevel.Trace;
            _webSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls13;
            _webSocket.Log.Output = (logData, message) =>
            {
                // `logData` is a WebSocketSharp.LogData object; `message` is the log message string.
                Log.Debug($"[{logData.Level}] {logData.Message}");
            };

            _webSocket.Connect();
            isConnected = true;

            WebsocketTransmissionAuthModel authModel = new WebsocketTransmissionAuthModel()
            {
                Type = "AUTH",
                ApiKey = apiKey,
                LaneId = laneId,
            };
            var websocketConnectionString = JsonSerializer.Serialize(authModel);
            var encoded = Encoding.UTF8.GetBytes(websocketConnectionString);
            _webSocket.Send(encoded);
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            Log.Information($"WebSocket closed: {e.Reason}");
            // Implement your reconnection logic here
            // Be mindful of the reason for the close and implement exponential backoff to avoid flooding the server with reconnect attempts
            Log.Information($"WebSocket closed: {e.Reason}. Reconnecting in {_reconnectDelay / 1000} seconds...");
            Thread.Sleep(_reconnectDelay);

            Connect(); // Reconnect

            // Increase the delay for the next attempt, up to a maximum
            _reconnectDelay = Math.Min(_reconnectDelay * 2, 60000); // Cap at 60 seconds
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

            // Because we're creating one websocket per instance of the terminal, we need to 
            // double check that the laneId value passed to us from the websocket is equal to this
            // uniquely valued laneId.
            if (terminalMessageModel.LaneId == laneId)
            {
                if (terminalMessageModel.Action == "charge")
                {
                    WebsocketTransmissionChargeModel websocketTransmissionChargeModel = CallTerminal(terminalMessageModel);
                    var websocketChargeString = JsonSerializer.Serialize(websocketTransmissionChargeModel);
                    var encodedChargeString = Encoding.UTF8.GetBytes(websocketChargeString);
                    _webSocket.Send(encodedChargeString);
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

        public void CloseWebsocket()
        {
            Log.Warning("Websocket Closing");
            _webSocket.Close();
            isConnected = false;
        }
    }
}
