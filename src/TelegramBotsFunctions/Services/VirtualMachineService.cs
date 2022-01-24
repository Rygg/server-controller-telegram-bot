using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TelegramBotsFunctions.Interfaces;

namespace TelegramBotsFunctions.Services
{
    /// <summary>
    /// Class implementing the functionality controlling the virtual machines. Requires transient or scoped.
    /// Virtual machine is being controlled by a Logic App created to Azure, which itself uses the Azure Management API.
    /// </summary>
    public class VirtualMachineService : IVirtualMachineService
    {
        /// <summary>
        /// Injected logger.
        /// </summary>
        private readonly ILogger<VirtualMachineService> _logger;
        /// <summary>
        /// Inject http client.
        /// </summary>
        private readonly HttpClient _logicAppClient;
        /// <summary>
        /// TaskCompletionSource for getting asynchronous operation results. 
        /// </summary>
        private TaskCompletionSource<HttpResponseMessage>? _asynchronousResponseTaskCompletionSource;
        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource? _asynchronousResponseTaskCancellationTokenSource;
        
        /// <summary>
        /// Constructor for the service.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        /// <param name="httpClientFactory">Injected http client factory</param>
        public VirtualMachineService(ILogger<VirtualMachineService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _logicAppClient = httpClientFactory.CreateClient("LogicAppClient");
        }

        /// <summary>
        /// Checks the status of the virtual machine. Sends a request to the virtual machine.
        /// </summary>
        /// <returns>A tuple with the first value indicating the operation status. <br/>
        /// If the operation status returned is false, operation was only started. Use <see cref="GetAsynchronousOperationResultAsync"/> to get actual result. <br/>
        /// If the operation status returned is true, the operation result is stored in the second item of the tuple.
        /// </returns>
        /// <remarks>Note: This will cancel all other asynchronous operation tasks if asynchronous handling is required.</remarks>
        public async Task<Tuple<bool, string?>> GetVirtualMachineStatusAsync()
        {
            const string queryOperation = "instanceview";
            const string operationMethod = "GET";
            var response = await ProcessLogicAppRequestAsync(queryOperation, operationMethod);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // Success.
                {
                    var responseContentString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<VirtualMachineInstanceView>(responseContentString);
                    var status = responseObject.Statuses.Last().DisplayStatus; // Get the last status object as that's what we're interested in.
                    return new Tuple<bool, string?>(true, status); // Return true and status.
                }
                // Return false with null, Asynchronous result is required.
                case HttpStatusCode.Accepted:
                    return new Tuple<bool, string?>(false, null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), "Invalid status code"); // This should never happen.
            }
        }
        /// <summary>
        /// Starts the virtual machine. Sends a request to the virtual machine, and returns a boolean indicating the operation status.
        /// </summary>
        /// <returns>A boolean value representing the if the operation succeeded. <br/>
        /// If the value returned is false, operation was only started. Use <see cref="GetAsynchronousOperationResultAsync"/> to get actual result.</returns>
        /// <remarks>Note: This will cancel all other asynchronous operation tasks if asynchronous handling is required.</remarks>
        public async Task<bool> StartVirtualMachineAsync()
        {
            const string startOperation = "start";
            const string operationMethod = "POST";
            var response = await ProcessLogicAppRequestAsync(startOperation, operationMethod);
            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.Accepted => false,
                _ => throw new ArgumentOutOfRangeException(nameof(response), "Invalid status code") // This should never happen.
            };
        }
        /// <summary>
        /// Stops the virtual machine. Sends a request to the virtual machine, and returns a boolean indicating the operation status.
        /// </summary>
        /// <returns>A boolean value representing the if the operation succeeded. <br/>
        /// If the value returned is false, operation was only started. Use <see cref="GetAsynchronousOperationResultAsync"/> to get actual result.</returns>
        /// <remarks>Note: This will cancel all other asynchronous operation tasks if asynchronous handling is required.</remarks>
        public async Task<bool> StopVirtualMachineAsync()
        {
            const string stopOperation = "deAllocate";
            const string operationMethod = "POST";
            var response = await ProcessLogicAppRequestAsync(stopOperation, operationMethod);
            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.Accepted => false,
                _ => throw new ArgumentOutOfRangeException(nameof(response), "Invalid status code") // This should never happen.
            };
        }

        /// <summary>
        /// Task returns the status of an asynchronously started operation. Operation must already be started.
        /// </summary>
        /// <returns>The actual result of the operation.</returns>
        public async Task<object> GetAsynchronousOperationResultAsync(EVirtualMachineRequestType type)
        {
            if (_asynchronousResponseTaskCompletionSource != null)
            {
                var response = await _asynchronousResponseTaskCompletionSource.Task;
                switch (type)
                {
                    case EVirtualMachineRequestType.StartRequest:
                        return true;
                    case EVirtualMachineRequestType.StopRequest:
                        return true;
                    case EVirtualMachineRequestType.StatusRequest:
                        var responseContentString = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<VirtualMachineInstanceView>(responseContentString);
                        var status = responseObject.Statuses.Last().DisplayStatus; // Get the last status object as that's what we're interested in.
                        return status;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), "Invalid request type.");
                }
                
            }

            throw new InvalidOperationException("Asynchronous result is not being waited.");
        }


        /// <summary>
        /// Sends a Post-request to the LogicAppUri and returns if the request succeeded.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="method"></param>
        /// <returns>The response message to the request.</returns>
        private async Task<HttpResponseMessage> ProcessLogicAppRequestAsync(string action, string method)
        {
            try
            {
                _logger.LogDebug("Sending request to LogicApp: Method: {0}, Action: {1}", method, action);

                var payload = new LogicAppRequest(action, method);

                var response = await _logicAppClient.PostAsync(string.Empty, new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"));

                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted)
                {
                    _logger.LogDebug("Successful response from Logic App. StatusCode: {0}", response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        _ = AsynchronousOperationResponseLoop(response); // Start asynchronous operation response loop.
                    }
                    return response; // Completed on first try.
                }

                _logger.LogError("Unknown status code while performing a LogicApp request: {0}", response.StatusCode);
                throw new HttpRequestException(response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing a request to the Logic App");
                throw;
            }
        }

        /// <summary>
        /// Background loop started if a request is not processed synchronously at the logic app. Starting this will cancel all old operation result tasks.
        /// </summary>
        /// <param name="response">Original response.</param>
        /// <returns>Successful request response.</returns>
        private async Task AsynchronousOperationResponseLoop(HttpResponseMessage response)
        {
            const int asynchronousRequestTimeoutMinutes = 5;

            _logger.LogDebug("Request is still processing. Start querying operation results. Timeout {0} minutes", asynchronousRequestTimeoutMinutes);

            _asynchronousResponseTaskCompletionSource?.TrySetException(new OperationCanceledException("Another asynchronous operation overrode the task"));
            _asynchronousResponseTaskCancellationTokenSource?.Cancel();
            _asynchronousResponseTaskCancellationTokenSource?.Dispose();
            _logger.LogTrace("Reset old tasks.");

            _asynchronousResponseTaskCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(asynchronousRequestTimeoutMinutes));
            _asynchronousResponseTaskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
            try
            {
                var responseLocation = response.Headers.Location; // Get asynchronous response location from the header.
                var waitTime = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(10); // Get retry timer from the header. Default to 10 seconds if not present.
                while (!_asynchronousResponseTaskCancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(waitTime, _asynchronousResponseTaskCancellationTokenSource.Token); // Wait the retry timer.
                    var operationStatusResponse = await _logicAppClient.GetAsync(responseLocation, _asynchronousResponseTaskCancellationTokenSource.Token); // Get operation status.
                    switch (operationStatusResponse.StatusCode)
                    {
                        case HttpStatusCode.Accepted: // Request is still processing. Continue.
                            continue;
                        case HttpStatusCode.OK: // Request has been completed successfully.
                            _logger.LogDebug("Asynchronous LogicApp requested completed successfully."); // Request processed.
                            _asynchronousResponseTaskCancellationTokenSource.Cancel(); // Cancel the cancellation token source to break out of the loop.
                            _asynchronousResponseTaskCompletionSource?.TrySetResult(operationStatusResponse);
                            return;
                        default:
                            _logger.LogError("Unknown status code while requesting Logic App asynchronous operation results: {0}", operationStatusResponse.StatusCode);
                            _asynchronousResponseTaskCompletionSource?.TrySetException(new HttpRequestException(operationStatusResponse.ReasonPhrase));
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while querying asynchronous operation result.");
                _asynchronousResponseTaskCompletionSource?.TrySetException(ex);
            }
        }

        /// <summary>
        /// Class for LogicAppRequest bodies.
        /// </summary>
        private class LogicAppRequest
        {
            [JsonProperty("action")] 
            private readonly string _action;

            [JsonProperty("method")] 
            private readonly string _method;
            public LogicAppRequest(string act, string met)
            {
                _action = act;
                _method = met;
            }

            public string ToJsonString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}
