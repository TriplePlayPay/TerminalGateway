using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using TerminalGateway.Desktop.WPF.Communications.Models;
using Serilog;

namespace TerminalGateway.Desktop.WPF.Communications.Database
{
    public class DatabaseManager
    {
        private const string DbFileName = "ClientData.db";

        public DatabaseManager()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(DbFileName))
            {
                SQLiteConnection.CreateFile(DbFileName);
                using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
                {
                    connection.Open();
                    string sql = "create table if not exists client_info (api_key TEXT)";
                    SQLiteCommand command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQuery();

                    string moreSQL = "create table if not exists terminal_info (ip_address TEXT, lane_id TEXT)";
                    SQLiteCommand anotherCommand = new SQLiteCommand(moreSQL, connection);
                    anotherCommand.ExecuteNonQuery();
                }
            }
        }

        public void SaveApiKey(string apiKey)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                string sql = "INSERT INTO client_info (api_key) VALUES (@apiKey)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@apiKey", apiKey);
                command.ExecuteNonQuery();
            }
        }

        public void AddTerminal(string ipAddress, string laneId)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                string sql = "INSERT INTO terminal_info (ip_address, lane_id) VALUES (@ipAddress, @laneId)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@ipAddress", ipAddress);
                command.Parameters.AddWithValue("@laneId", laneId);
                command.ExecuteNonQuery();
            }
        }

        public string GetApiKey()
        {
            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                string sql = "SELECT api_key FROM client_info ORDER BY ROWID DESC LIMIT 1";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["api_key"].ToString();
                    }
                }
            }

            return "";
        }

        public List<WebsocketConnectionModel> GetAllWebsocketConnectionEntities()
        {
            var terminals = new List<WebsocketConnectionModel>();

            var apiKey = GetApiKey();

            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                var sql = "SELECT ip_address, lane_id FROM terminal_info";
                var command = new SQLiteCommand(sql, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        terminals.Add(new WebsocketConnectionModel
                        {
                            ApiKey = apiKey,
                            IpAddress = reader["ip_address"].ToString(),
                            LaneId = reader["lane_id"].ToString()
                        });
                    }
                }
            }

            return terminals;
        }
        public List<TerminalModel> GetTerminals()
        {
            var terminals = new List<TerminalModel>();

            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                var sql = "SELECT ip_address, lane_id FROM terminal_info";
                var command = new SQLiteCommand(sql, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        terminals.Add(new TerminalModel
                        {
                            IpAddress = reader["ip_address"].ToString(),
                            LaneId = reader["lane_id"].ToString()
                        });
                    }
                }
            }

            return terminals;
        }

        public bool LaneIdExists(string laneId)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                string sql = "SELECT COUNT(1) FROM terminal_info WHERE lane_id = @laneId";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@laneId", laneId);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public bool UpdateTerminal(string laneId, string ipAddress)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                string sql = "UPDATE terminal_info SET ip_address = @ipAddress WHERE lane_id = @laneId";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@ipAddress", ipAddress);
                command.Parameters.AddWithValue("@laneId", laneId);

                int result = command.ExecuteNonQuery();
                if (result == 0)
                {
                    Log.Information("No records updated; check if the laneId exists.");
                    return false;
                }
                else
                {
                    Log.Information("Record updated successfully.");
                    return true;
                }
            }
        }

        public bool DeleteTerminal(string laneId)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbFileName};Version=3;"))
            {
                connection.Open();
                string sql = "DELETE FROM terminal_info WHERE lane_id = @laneId";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@laneId", laneId);

                int result = command.ExecuteNonQuery();
                if (result == 0)
                {
                    Log.Information("No records deleted; check if the laneId exists.");
                    return false;
                }
                else
                {
                    Log.Information("Record deleted successfully.");
                    return true;
                }
            }
        }
    }
}
