using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace TerminalGateway.Desktop.WPF.Communications.NewPos
{
    /// <summary>
    /// Handles communication with TPP payment terminals
    /// </summary>
    public class TPPDeviceCaller
    {
        /// <summary>
        /// Sends a message to the payment terminal and waits for a response
        /// </summary>
        /// <param name="tppDeviceInput">Input parameters for the terminal call</param>
        /// <returns>The response message from the terminal or error details</returns>
        public static TPPDeviceCallerResult SendTerminalMessage(TPPDeviceCallerInput tppDeviceInput)
        {
            var result = new TPPDeviceCallerResult();

            // Input validation
            if (string.IsNullOrEmpty(tppDeviceInput.Host))
            {
                result.ErrorMessage = "Host address cannot be empty";
                return result;
            }

            TPPDeviceClient client = new TPPDeviceClient(tppDeviceInput.Host, tppDeviceInput.Port);

            try
            {
                // Try to connect to the terminal
                if (!client.Connect())
                {
                    result.ErrorMessage = $"Unable to connect to device at {tppDeviceInput.Host}:{tppDeviceInput.Port}";
                    return result;
                }

                // Create the message payload
                var messageInput = new TppDeviceCallerMessageInput
                {
                    MerchantKey = tppDeviceInput.MerchantKey,
                    Amount = tppDeviceInput.Amount,
                    PaymentType = tppDeviceInput.PaymentType
                };

                // Serialize the message payload
                string message = JsonSerializer.Serialize(messageInput);

                // Create a cancellation token source
                using (var cts = new CancellationTokenSource())
                {
                    // Send the message to the terminal
                    if (client.SendMessage(message))
                    {
                        // Poll for a response with the specified timeout
                        string response = client.PollForPaymentResponse(
                            tppDeviceInput.TimeoutSeconds,
                            500, // Check every 500ms
                            cts.Token);

                        if (response != null)
                        {
                            // We received a valid response
                            result.Success = true;
                            result.Response = response;
                        }
                        else if (cts.Token.IsCancellationRequested)
                        {
                            // The operation was cancelled
                            result.WasCancelled = true;
                            result.ErrorMessage = "Payment process was cancelled";
                        }
                        else
                        {
                            // The operation timed out
                            result.TimedOut = true;
                            result.ErrorMessage = $"Payment timed out after {tppDeviceInput.TimeoutSeconds} seconds";
                        }
                    }
                    else
                    {
                        result.ErrorMessage = "Failed to send message to terminal";
                    }
                }
            }
            catch (TPPConnectionException ex)
            {
                result.ErrorMessage = $"Connection error: {ex.Message}";
            }
            catch (TPPProtocolException ex)
            {
                result.ErrorMessage = $"Protocol error: {ex.Message}";
            }
            catch (TPPDeviceException ex)
            {
                result.ErrorMessage = $"Device error: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                // Always disconnect the client
                client.Disconnect();
            }

            return result;
        }

        /// <summary>
        /// Asynchronously sends a message to the payment terminal and waits for a response
        /// </summary>
        /// <param name="tppDeviceInput">Input parameters for the terminal call</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The response message from the terminal or error details</returns>
        public static async Task<TPPDeviceCallerResult> SendTerminalMessageAsync(
            TPPDeviceCallerInput tppDeviceInput,
            CancellationToken cancellationToken = default)
        {
            // Execute the operation on a background thread
            return await Task.Run(() => {
                // Create a linked token source that combines the external token and our internal one
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    // We'll use the original method but provide our linked token to the client
                    var input = tppDeviceInput;

                    TPPDeviceClient client = new TPPDeviceClient(input.Host, input.Port);
                    var result = new TPPDeviceCallerResult();

                    try
                    {
                        // Try to connect to the terminal
                        if (!client.Connect())
                        {
                            result.ErrorMessage = $"Unable to connect to device at {input.Host}:{input.Port}";
                            return result;
                        }

                        // Create the message payload
                        var messageInput = new TppDeviceCallerMessageInput
                        {
                            MerchantKey = input.MerchantKey,
                            Amount = input.Amount,
                            PaymentType = input.PaymentType
                        };

                        // Serialize the message payload
                        string message = JsonSerializer.Serialize(messageInput);

                        // Send the message to the terminal
                        if (client.SendMessage(message))
                        {
                            // Poll for a response with the specified timeout
                            string response = client.PollForPaymentResponse(
                                input.TimeoutSeconds,
                                500, // Check every 500ms
                                cts.Token);

                            if (response != null)
                            {
                                // We received a valid response
                                result.Success = true;
                                result.Response = response;
                            }
                            else if (cts.Token.IsCancellationRequested)
                            {
                                // The operation was cancelled
                                result.WasCancelled = true;
                                result.ErrorMessage = "Payment process was cancelled";
                            }
                            else
                            {
                                // The operation timed out
                                result.TimedOut = true;
                                result.ErrorMessage = $"Payment timed out after {input.TimeoutSeconds} seconds";
                            }
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to send message to terminal";
                        }
                    }
                    catch (TPPConnectionException ex)
                    {
                        result.ErrorMessage = $"Connection error: {ex.Message}";
                    }
                    catch (TPPProtocolException ex)
                    {
                        result.ErrorMessage = $"Protocol error: {ex.Message}";
                    }
                    catch (TPPDeviceException ex)
                    {
                        result.ErrorMessage = $"Device error: {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        result.ErrorMessage = $"Unexpected error: {ex.Message}";
                    }
                    finally
                    {
                        // Always disconnect the client
                        client.Disconnect();
                    }

                    return result;
                }
            }, cancellationToken);
        }
    }
}
