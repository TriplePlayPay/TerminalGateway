using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;
using GlobalPayments.Api.Terminals;
using TerminalGateway.Desktop.WPF.Communications.Rest;
using TerminalGateway.Desktop.WPF.Communications.Models;
using TerminalGateway.Desktop.WPF.Communications.Database;
using TerminalGateway.Desktop.WPF.Communications.Provider;
using TerminalGateway.Desktop.WPF.Communications.Constants;
using Serilog;

namespace TerminalGateway.Desktop.WPF.Communications.Terminal
{
    public class TerminalManager
    {
        private RestCaller _restCaller;
        private DatabaseManager _databaseManager;

        public TerminalManager(string apiKey, DatabaseManager databaseManager)
        {
            RestConfiguration restConfiguration = new RestConfiguration()
            {
                ApiKey = apiKey,
                ApiUrl = ConstantValues.ApiUrl,
            };
            _restCaller = new RestCaller(restConfiguration);
            _databaseManager = databaseManager;
        }

        private bool SyncTerminalToDatabase(TerminalModel terminalModel)
        {
            try
            {
                bool terminalInDatabase = _databaseManager.LaneIdExists(terminalModel.LaneId);
                if (terminalInDatabase)
                {
                    _databaseManager.UpdateTerminal(terminalModel.LaneId, terminalModel.IpAddress);
                    return true;
                }
                else
                {
                    _databaseManager.AddTerminal(terminalModel.IpAddress, terminalModel.LaneId);
                    return true;
                }
            } catch (Exception ex)
            {
                Log.Error("Exception Syncing Terminal To Database " + ex.ToString());
                return false;
            }
        }

        private async Task<bool> SyncTerminalToApi(TerminalModel terminalModel)
        {
            try
            {
                List<TerminalModel> terminalModels = await _restCaller.GetPaxTerminals();
                bool terminalInApi = terminalModels.Any(t => t.LaneId == terminalModel.LaneId);
                if (terminalInApi)
                {
                   bool updateResult = await _restCaller.UpdateTerminal(terminalModel);
                   return updateResult;
                }
                else
                {
                    bool addResult = await _restCaller.AddTerminal(terminalModel);
                    return addResult;
                }
            } catch (Exception ex)
            {
                Log.Error("Exception getting pax terminals in SyncTerminalToApi: ",  ex.ToString());
                return false;
            }
        }

        public async Task<TerminalSyncUpdates> SyncTerminal(TerminalModel terminalModel)
        {
            bool isApiSyncSuccessful = await SyncTerminalToApi(terminalModel);
            if (!isApiSyncSuccessful)
            {
                return new TerminalSyncUpdates()
                {
                    SuccessfullySyncedToDatabase = false,
                    SuccessfullySyncedToApi = false
                };
            }

            // Sync with Database Locally
            bool isTerminalDatabaseSyncSuccessful = SyncTerminalToDatabase(terminalModel);
            if (!isTerminalDatabaseSyncSuccessful)
            {
                return new TerminalSyncUpdates()
                {
                    SuccessfullySyncedToDatabase = false,
                    SuccessfullySyncedToApi = isApiSyncSuccessful
                };
            }
            return new TerminalSyncUpdates()
            {
                SuccessfullySyncedToDatabase = isTerminalDatabaseSyncSuccessful,
                SuccessfullySyncedToApi = isApiSyncSuccessful
            };
        }
    }
}
