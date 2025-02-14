using GlobalPayments.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TerminalGateway.Desktop.WPF.Communications.Models;
using Serilog;

namespace TerminalGateway.Desktop.WPF.Communications.Rest
{
    public class RestCaller
    {
        private RestClient _restClient;

        public RestCaller(RestConfiguration restConfiguration) 
        { 
            _restClient = new RestClient(restConfiguration);
        }

        public async Task<bool> IsApiKeyValid()
        {
            // Example POST request with authorization
            string pingResult = await _restClient.GetAsync("/ping");
            try
            {
                TriplePlayPayResponse<PingResult> ping = JsonSerializer.Deserialize<TriplePlayPayResponse<PingResult>>(pingResult);
                if (ping.Message.Result == "pong")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } catch (Exception ex)
            {
                Log.Error("Exception with deserializing ping response: " + ex.Message);
                return false;
            }
        }

        public async Task<List<TerminalModel>> GetPaxTerminals()
        {
            List<TerminalModel> results = new List<TerminalModel>();
            string terminalsResult = await _restClient.GetAsync("/terminal/pax-terminals");
            try
            {
                TriplePlayPayResponse<List<TerminalResponseModel>> terminals = JsonSerializer.Deserialize<TriplePlayPayResponse<List<TerminalResponseModel>>>(terminalsResult);
                if (!terminals.Status)
                {
                    throw new Exception("Invalid Response Status");
                }
                else
                {
                    terminals.Message.ForEach(terminal =>
                    {
                        var newTerminal = new TerminalModel()
                        {
                            IpAddress = terminal.IpAddress,
                            LaneId = terminal.LaneId

                        };
                        results.Add(newTerminal);
                    });
                    return results;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception thrown in GetPaxTerminals: " + ex.Message);
                throw;
            }
        }

        public async Task<TerminalModel> GetPaxTerminalByLaneId(string laneId)
        {
            List<TerminalModel> results = new List<TerminalModel>();
            string terminalsResult = await _restClient.GetAsync("/terminal/get-pax/" + laneId);
            try
            {
                TriplePlayPayResponse<TerminalResponseModel> terminal = JsonSerializer.Deserialize<TriplePlayPayResponse<TerminalResponseModel>>(terminalsResult);
                if (!terminal.Status)
                {
                    throw new Exception("Invalid Response Status");
                }
                else
                {
                    var newTerminal = new TerminalModel()
                    {
                        IpAddress = terminal.Message.IpAddress,
                        LaneId = terminal.Message.LaneId

                    };
                    return newTerminal;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> AddTerminal(TerminalModel terminalModel)
        {
            try
            {
                string paxResult = await _restClient.PostAsync("/terminal/add-pax", terminalModel);
                Log.Information("Here is the paxResult: " + paxResult);
                TriplePlayPayResponse<TerminalResponseModel> newPaxTerminal = JsonSerializer.Deserialize<TriplePlayPayResponse<TerminalResponseModel>>(paxResult);
                return newPaxTerminal.Status;
            }
            catch (Exception ex)
            {
                Log.Information("Exception with deserializing pax response: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateTerminal(TerminalModel terminalModel)
        {
            // Example POST request with authorization
            var paxTerminal = JsonSerializer.Serialize(terminalModel);
            try
            {
                string paxResult = await _restClient.PostAsync("/terminal/add-pax", paxTerminal);
                Log.Information("Here is the paxResult: " + paxResult);
                TriplePlayPayResponse<TerminalResponseModel> newPaxTerminal = JsonSerializer.Deserialize<TriplePlayPayResponse<TerminalResponseModel>>(paxResult);
                return newPaxTerminal.Status;
            }
            catch (Exception ex)
            {
                Log.Error("Exception with deserializing pax response from add-pax: " + ex.Message);
                return false;
            }
        }

        public async Task<string> DeleteTerminalByLaneId(string laneId)
        {
            string terminalsResult = await _restClient.DeleteAsync("/terminal/delete-pax/" + laneId);
            try
            {
                TriplePlayPayResponse<TerminalDeleteResponseModel> terminal = JsonSerializer.Deserialize<TriplePlayPayResponse<TerminalDeleteResponseModel>>(terminalsResult);
                if (!terminal.Status)
                {
                    throw new Exception("Invalid Response Status");
                }
                else
                {
                    return terminal.Message.DeletedId;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
