using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using Serilog;

namespace TerminalGateway.Desktop.WPF.Communications.NewPos
{
    /// <summary>
    /// Client for TPP devices that communicates over TCP/IP.
    /// </summary>
    public class TPPDeviceClient
    {
        private const byte DATA_MESSAGE = 0x01;
        private const byte ACK_MESSAGE = 0x02;
        private const byte NACK_MESSAGE = 0x03;
        private const int TIMEOUT = 5000; // milliseconds
        private const byte PROTOCOL_VERSION = 0x01;
        private const int MAX_RETRIES = 3;

        private readonly string host;
        private readonly int port;
        private TcpClient tcpClient;
        private NetworkStream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="TPPDeviceClient"/> class.
        /// </summary>
        /// <param name="host">The host address of the server.</param>
        /// <param name="port">The port number of the server.</param>
        public TPPDeviceClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <returns>True if the connection is successful; otherwise, false.</returns>
        public bool Connect()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(host, port);
                tcpClient.ReceiveTimeout = TIMEOUT;
                tcpClient.SendTimeout = TIMEOUT;
                stream = tcpClient.GetStream();
                Log.Information($"Connected to server: {host}:{port}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Connection error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
            Log.Information("Disconnected from server");
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the message was successfully sent and acknowledged; otherwise, false.</returns>
        public bool SendMessage(string message)
        {
            if (tcpClient == null || stream == null)
            {
                Log.Error("Not connected to server.");
                return false;
            }

            byte[] payload = Encoding.UTF8.GetBytes(message);
            int payloadLength = payload.Length;
            uint crc = CalculateCRC32(payload);

            try
            {
                for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
                {
                    try
                    {
                        // Pack the message header
                        byte[] packedHeader = new byte[] { DATA_MESSAGE, PROTOCOL_VERSION };

                        // Pack the payload length (big-endian)
                        byte[] packedPayloadLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payloadLength));

                        // Pack the CRC as a 64-bit value (big-endian)
                        long crc64 = (long)crc & 0xFFFFFFFFL; // Ensure CRC is unsigned 32-bit
                        byte[] packedCrc = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(crc64));

                        // Concatenate the packed data
                        byte[] packedMessage = new byte[packedHeader.Length + packedPayloadLength.Length + payload.Length + packedCrc.Length];

                        Buffer.BlockCopy(packedHeader, 0, packedMessage, 0, packedHeader.Length);
                        Buffer.BlockCopy(packedPayloadLength, 0, packedMessage, packedHeader.Length, packedPayloadLength.Length);
                        Buffer.BlockCopy(payload, 0, packedMessage, packedHeader.Length + packedPayloadLength.Length, payload.Length);
                        Buffer.BlockCopy(packedCrc, 0, packedMessage, packedHeader.Length + packedPayloadLength.Length + payload.Length, packedCrc.Length);

                        Log.Information($"Sending binary message (hex): {BitConverter.ToString(packedMessage).Replace("-", "")}");
                        stream.Write(packedMessage, 0, packedMessage.Length);
                        Log.Information($"Message sent: {message}");

                        // Wait for ACK or NACK
                        DateTime startTime = DateTime.Now;
                        while ((DateTime.Now - startTime).TotalMilliseconds < TIMEOUT)
                        {
                            try
                            {
                                if (stream.DataAvailable)
                                {
                                    byte[] response = new byte[1];
                                    int bytesRead = stream.Read(response, 0, 1);
                                    if (bytesRead == 0)
                                    {
                                        Log.Warning("No response received from server");
                                        break;
                                    }

                                    byte responseByte = response[0];
                                    if (responseByte == ACK_MESSAGE)
                                    {
                                        Log.Information("ACK received from server");
                                        return true;
                                    }
                                    else if (responseByte == NACK_MESSAGE)
                                    {
                                        Log.Warning("NACK received from server, retrying...");
                                        break;
                                    }
                                    else
                                    {
                                        Log.Warning($"Unexpected response from server: {responseByte}");
                                        break;
                                    }
                                }
                                Thread.Sleep(10); // Small delay to prevent CPU spinning
                            }
                            catch (IOException ex)
                            {
                                Log.Error($"IO error: {ex.Message}", ex);
                                break;
                            }
                        }
                        Log.Warning("Timeout waiting for server response, retrying...");
                    }
                    catch (SocketException ex)
                    {
                        Log.Error($"Connection error: {ex.Message}", ex);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error sending message: {ex.Message}", ex);
                        return false;
                    }
                }
                Log.Error("Max retries reached, message sending failed.");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error sending message: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Attempts to receive a message from the server if data is available.
        /// </summary>
        /// <returns>The received message, or null if no message was received or an error occurred.</returns>
        public string ReceiveMessage()
        {
            if (tcpClient == null || stream == null)
            {
                Log.Error("Not connected to server.");
                return null;
            }

            try
            {
                // Check if data is available before attempting to read
                if (!stream.DataAvailable)
                {
                    return null;
                }

                // Read the header (message type, protocol version, payload length)
                byte[] header = new byte[6]; // 1 + 1 + 4 = 6 bytes
                int bytesRead = stream.Read(header, 0, header.Length);
                if (bytesRead != header.Length)
                {
                    Log.Warning("Connection closed by server.");
                    return null;
                }

                byte messageType = header[0];
                byte protocolVersion = header[1];
                int payloadLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 2));

                // Read the payload
                byte[] payload = new byte[payloadLength];
                bytesRead = stream.Read(payload, 0, payload.Length);
                if (bytesRead != payload.Length)
                {
                    Log.Warning("Failed to read complete payload.");
                    return null;
                }

                // Read the CRC
                byte[] receivedCrc = new byte[8];
                bytesRead = stream.Read(receivedCrc, 0, receivedCrc.Length);
                if (bytesRead != receivedCrc.Length)
                {
                    Log.Warning("Failed to read complete CRC.");
                    return null;
                }

                // Reconstruct the entire message for logging
                byte[] fullMessage = new byte[header.Length + payload.Length + receivedCrc.Length];
                Buffer.BlockCopy(header, 0, fullMessage, 0, header.Length);
                Buffer.BlockCopy(payload, 0, fullMessage, header.Length, payload.Length);
                Buffer.BlockCopy(receivedCrc, 0, fullMessage, header.Length + payload.Length, receivedCrc.Length);

                Log.Information($"Received binary message (hex): {BitConverter.ToString(fullMessage).Replace("-", "")}");

                if (protocolVersion != PROTOCOL_VERSION)
                {
                    Log.Warning("Incompatible protocol version.");
                    stream.Write(new byte[] { NACK_MESSAGE }, 0, 1);
                    return null;
                }

                uint calculatedCrc = CalculateCRC32(payload);
                long receivedCrcValue = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(receivedCrc, 0));

                if (calculatedCrc == (uint)receivedCrcValue)
                {
                    string decodedMessage = Encoding.UTF8.GetString(payload);
                    Log.Information($"Message received correctly: {decodedMessage}");
                    stream.Write(new byte[] { ACK_MESSAGE }, 0, 1);
                    return decodedMessage;
                }
                else
                {
                    Log.Warning("CRC error, message discarded.");
                    stream.Write(new byte[] { NACK_MESSAGE }, 0, 1);
                    return null;
                }
            }
            catch (IOException ex)
            {
                Log.Warning($"IO error waiting for server message: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Error receiving message: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Polls for a message from the server with a specified timeout. This is ideal for payment transactions
        /// that require waiting for user interaction at the terminal.
        /// </summary>
        /// <param name="timeoutSeconds">Total time in seconds to poll for a message (default 240 seconds = 4 minutes).</param>
        /// <param name="pollingIntervalMs">Interval in milliseconds between polling attempts (default 500ms).</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>The received message, or null if no message was received within the timeout period or if the operation was cancelled.</returns>
        public string PollForPaymentResponse(int timeoutSeconds = 240, int pollingIntervalMs = 500, CancellationToken cancellationToken = default)
        {
            if (tcpClient == null || stream == null)
            {
                Log.Error("Not connected to server.");
                return null;
            }

            DateTime startTime = DateTime.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);

            Log.Information($"Starting to poll for payment response with a {timeoutSeconds} second timeout");

            while (DateTime.Now - startTime < timeout)
            {
                // Check if cancellation was requested
                if (cancellationToken.IsCancellationRequested)
                {
                    Log.Information("Payment polling cancelled by request.");
                    return null;
                }

                try
                {
                    // Try to receive a message
                    string message = ReceiveMessage();
                    if (message != null)
                    {
                        Log.Information($"Payment response received after polling for {(DateTime.Now - startTime).TotalSeconds:F1} seconds");
                        return message;
                    }

                    // No message yet, wait for the polling interval before checking again
                    Thread.Sleep(pollingIntervalMs);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error polling for payment response: {ex.Message}", ex);
                    return null;
                }
            }

            Log.Warning($"Payment timeout reached after {timeoutSeconds} seconds with no response received");
            return null;
        }

        /// <summary>
        /// Calculates the CRC32 checksum for the given data.
        /// </summary>
        /// <param name="data">The data to calculate the checksum for.</param>
        /// <returns>The CRC32 checksum.</returns>
        private uint CalculateCRC32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ 0xEDB88320;
                    else
                        crc >>= 1;
                }
            }
            return ~crc;
        }
    }
}