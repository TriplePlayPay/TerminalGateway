using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TerminalGateway.Desktop.WPF.Communications.Models;

namespace TerminalGateway.Desktop.WPF.Communications.Websocket
{
    public class WebsocketSerializer
    {
        public static TerminalMessageModel DeserializeTerminalMessageModel(string chargeRequest, WebsocketConnectionModel websocketConnectionModel)
        {
            var inboundChargeModel = JsonSerializer.Deserialize<WebsocketTransmissionInboundChargeModel>(chargeRequest);
            return new TerminalMessageModel()
            {
                Amount = inboundChargeModel.Amount,
                TerminalIpAddress = websocketConnectionModel.IpAddress,
                LaneId = inboundChargeModel.LaneId,
                RequestId = inboundChargeModel.RequestId,
                Action = inboundChargeModel.Action,
                TerminalPaymentType = inboundChargeModel.TerminalPaymentType,
                TerminalType = inboundChargeModel.TerminalType
            };
        }
    }
}
