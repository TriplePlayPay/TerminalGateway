using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;
using GlobalPayments.Api.Terminals;
using GlobalPayments.Api.Terminals.Abstractions;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Provider;

namespace TerminalGateway.Desktop.WPF.Communications.Terminal
{
    public class TerminalTransmitter
    {
        /// <summary>
        /// Transmits to a terminal. Response below if successful.
        /// 
        /// {[STX]0[FS]T01[FS]1.54[FS]000000[FS]OK[FS]00[US]APPROVAL[US]611532[US]410813655763[US][US]3[US]MCC0000135210417[US][US][FS]01[FS]15000[US]0[US]0[US]0[US]0[US]0[US][US][US][US][US][US]0[US]0[US]0[FS]4111[US]4[US]1225[US][US][US][US]02[US]UAT USA/Test Card 07      [US][US][US]0[US][US][US][US][US][US][FS]2[US]256549[US]20240417141959[US][US][US][US][US][FS]0[US]AVS Not Requested.[US][US][US][FS][FS][FS][US]EDCTYPE=CREDIT[US]HRef=200069124602[US]CARDBIN=541333[US]PROGRAMTYPE=0[US]SN=1240334673[US]PINSTATUSNUM=0[US]GLOBALUID=1240334673202404171419590310[US]ARQC=6FD3375A73F0F2DC[US]TVR=0400088000[US]AID=A0000000041010[US]TSI=E800[US]ATC=0026[US]APPLAB=MASTERCARD[US]APPPN=Mastercard[US]IAD=0110200009620000000000000000000000FF[US]CID=00[US]CVM=6[US]userLanguageStatus=1[US][FS][FS][FS][FS][ETX]*}
        /// </summary>
        /// <param name="terminalMessageModel"></param>
        /// <returns></returns>
        public static IDeviceResponse TransmitToTerminal(TerminalMessageModel terminalMessageModel)
        {

            var _device = DeviceService.Create(new ConnectionConfig
            {
                DeviceType = DeviceType.PAX_DEVICE,
                ConnectionMode = ConnectionModes.TCP_IP,
                IpAddress = terminalMessageModel.TerminalIpAddress,
                Port = "10009",
                RequestIdProvider = new RandomIdProvider()
            });

            var paymentMethodType = PaymentMethodType.Credit;
            switch (terminalMessageModel.TerminalPaymentType)
            {
                case "DEBIT":
                    paymentMethodType = PaymentMethodType.Debit; 
                    break;
                case "REFERENCE":
                    paymentMethodType = PaymentMethodType.Reference;
                    break;
                case "EBT":
                    paymentMethodType = PaymentMethodType.EBT;
                    break;
                case "CASH":
                    paymentMethodType = PaymentMethodType.Cash;
                    break;
                case "ACH":
                    paymentMethodType = PaymentMethodType.ACH;
                    break;
                case "GIFT":
                    paymentMethodType = PaymentMethodType.Gift;
                    break;
                case "OTHER":
                    paymentMethodType = PaymentMethodType.Other;
                    break;
                default:
                    paymentMethodType = PaymentMethodType.Credit;
                    break;
            }

            var result = _device.Sale(terminalMessageModel.Amount)
                .WithPaymentMethodType(paymentMethodType)
                .WithAllowDuplicates(true)
                .Execute();

            return result;
        }
    }
}
