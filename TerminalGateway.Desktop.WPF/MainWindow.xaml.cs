using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TerminalGateway.Desktop.WPF.Communications;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Database;
using TerminalGateway.Desktop.WPF.Communications.ApiKey;
using TerminalGateway.Desktop.WPF.Communications.Terminal;
using TerminalGateway.Desktop.WPF.Communications.Validators;
using TerminalGateway.Desktop.WPF.Communications.Websocket;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Serilog;
using TerminalGateway.Desktop.WPF.Communications.Writers;

namespace TerminalGateway.Desktop.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseManager _databaseManager;
        private WebsocketsManager _websocketsManager;

        public MainWindow()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
            _websocketsManager = new WebsocketsManager(_databaseManager);

            string apiKey = _databaseManager.GetApiKey();
            if (!string.IsNullOrEmpty(apiKey))
            {
                ApiKeyTextBox.Text = apiKey;
                ((MainViewModel)DataContext).ApiKey = apiKey;
            }

            if (DataContext is MainViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.ApiKeyChanged += ViewModel_ApiKeyChanged;
                viewModel.WebsocketChanged += ViewModel_WebsocketChanged;
            }

            // populate the terminals in a way that'll connect them to websockets
            List<TerminalModel> storedTerminals = _databaseManager.GetTerminals();
            ObservableCollection<TerminalModel> shownTerminals = new ObservableCollection<TerminalModel>();
            if (storedTerminals.Count > 0)
            {
                storedTerminals.ForEach(terminal =>
                {
                    shownTerminals.Add(new TerminalModel { LaneId = terminal.LaneId, IpAddress = terminal.IpAddress });
                });
            }
            ((MainViewModel)DataContext).Terminals = shownTerminals;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // React to property changes in general
            if (e.PropertyName == nameof(MainViewModel.Terminals) && ((MainViewModel)DataContext).Terminals.Count > 0)
            {
                _websocketsManager.SyncWebsocketConnections();
                ((MainViewModel)DataContext).IsWebSocketConnected = _websocketsManager.IsConnected;
            }
        }

        private async void ViewModel_ApiKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            // React to changes in the ApiKey
            if (e.PropertyName == "ApiKey")
            {
                string apiKey = ApiKeyTextBox.Text;
                bool isApiKeyValid = await ApiKeyManager.IsApiKeyValid(apiKey);
                if (!isApiKeyValid)
                {
                    // Update logic here
                    MessageBox.Show("Api Key Event Error - Check and Retry. Should look something like: 8f1e01b7-a6e6-441a-b50c-cf9ce09aa0e3");
                }
                else
                {
                    MessageBox.Show("Api Key Successfully Added!");
                    _databaseManager.SaveApiKey(apiKey);
                    Logger.SetApiKeyAndInitializeLogging();
                    Log.Information("API Key Succeessfully Added!");
                }
            }
        }

        private void ViewModel_WebsocketChanged(object sender, PropertyChangedEventArgs e)
        {
            // React to changes in the WebSocket connection status
            if (e.PropertyName == "IsWebSocketConnected")
            {
                Log.Debug("WebSocket connection status has changed.");
            }
        }

        private async void AddTerminalButton_Click(object sender, RoutedEventArgs e)
        {
            // Example logic for handling Add/Update api_key
            var apiKey = ApiKeyTextBox.Text;
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("An API Key is required for adding a terminal, please add one.");
                return;
            }

            // Check if api_key exists in your database
            // If it does, update it. If not, add it to the database.
            bool isApiKeyValid = await ApiKeyManager.IsApiKeyValid(apiKey); // Replace with actual check
            if (!isApiKeyValid)
            {
                // Update logic here
                MessageBox.Show("Api Key Error - Check and Retry. Should look something like: 8f1e01b7-a6e6-441a-b50c-cf9ce09aa0e3");
                MessageBox.Show("An API Key is required for adding a terminal, please add one.");
                return;
            }

            // Example logic for adding a new terminal entry
            var laneId = LaneIdTextBox.Text;
            var ipAddress = IpAddressTextBox.Text;
            if (string.IsNullOrEmpty(laneId) || string.IsNullOrEmpty(ipAddress))
            {
                MessageBox.Show("Please enter both Lane ID and IP Address.");
                return;
            }

            if (!IpAddressValidator.IsValidIPAddress(ipAddress))
            {
                MessageBox.Show("IP Address is invalid, should look something like: 192.168.1.204");
                return;
            }

            var terminal = new TerminalModel { LaneId = LaneIdTextBox.Text, IpAddress = IpAddressTextBox.Text };
            TerminalManager terminalManager = new TerminalManager(apiKey, _databaseManager);
            TerminalSyncUpdates terminalSyncUpdates = await terminalManager.SyncTerminal(terminal);

            if (!terminalSyncUpdates.SuccessfullySyncedToApi)
            {
                MessageBox.Show("Unable to sync to the central API. Double check your lane id to ensure it is not duplicated, and try again.");
                Log.Warning("Unable to Sync New Terminal to Central API");
                return;
            }
            else if (!terminalSyncUpdates.SuccessfullySyncedToApi)
            {
                MessageBox.Show("Unable to save to the local database. Please restart the app and try again.");
                Log.Warning("Unable to Sync New Terminal to Local SQLite Database");
                return;
            }
            else
            {
                var theTerminals = ((MainViewModel)DataContext).Terminals;
                theTerminals.Add(terminal);
                ((MainViewModel)DataContext).Terminals = theTerminals;

                // Clear input fields
                LaneIdTextBox.Clear();
                IpAddressTextBox.Clear();
                Log.Information("A terminal was successfully added laneId: " + terminal.LaneId);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Example logic for handling Add/Update api_key
            var apiKey = ApiKeyTextBox.Text;
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Please enter an API Key.");
                return;
            }
            ((MainViewModel)DataContext).ApiKey = apiKey;
            // Check if api_key exists in your database
            // If it does, update it. If not, add it to the database.
            //bool isApiKeyValid = await ApiKeyManager.IsApiKeyValid(apiKey); // Replace with actual check
            //if (isApiKeyValid)
            //{
            //    // Update logic here
            //    _databaseManager.SaveApiKey(apiKey);
            //    MessageBox.Show("Api Key updated successfully.");
            //    ((MainViewModel)DataContext).ApiKey = apiKey;
               
            //}
            //else
            //{
            //    // Add logic here
            //    MessageBox.Show("Api Key Error - Check and Retry. Should look something like: 8f1e01b7-a6e6-441a-b50c-cf9ce09aa0e3");
            //}
        }
    }
}