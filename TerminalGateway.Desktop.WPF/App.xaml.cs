using System.Configuration;
using System.Data;
using System.Windows;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.BatchFormatters;
using TerminalGateway.Desktop.WPF.Communications.Writers;
using TerminalGateway.Desktop.WPF.Communications.Database;
using TerminalGateway.Desktop.WPF.Communications.Websocket;

namespace TerminalGateway.Desktop.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.SetApiKeyAndInitializeLogging();
            Log.Information("Application Started");
        }
    }

}
