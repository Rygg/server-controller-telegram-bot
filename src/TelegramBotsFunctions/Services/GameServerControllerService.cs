using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TelegramBotsFunctions.Interfaces;

namespace TelegramBotsFunctions.Services
{
    public class GameServerControllerService : IGameServerControllerService
    {
        /// <summary>
        /// Injected logger.
        /// </summary>
        private readonly ILogger<GameServerControllerService> _logger;

        /// <summary>
        /// Inject http client.
        /// </summary>
        private readonly HttpClient _gameServerClient;

        /// <summary>
        /// Constructor for the service.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        /// <param name="httpClientFactory">Injected http client factory</param>
        public GameServerControllerService(ILogger<GameServerControllerService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _gameServerClient = httpClientFactory.CreateClient("GameServerClient");
        }
        /// <summary>
        /// Method requests the list of Assetto Corsa tracks available on the server.
        /// </summary>
        /// <returns>A list of available tracks</returns>
        public async Task<string[]> GetAssettoCorsaTracks()
        {
            const string apiRoute = "api/assetto/tracks";
            try
            {
                _logger.LogDebug("Sending get assetto tracks request");
                var response = await _gameServerClient.GetAsync(apiRoute);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Response content: {0}", content);
                var trackArray = JsonConvert.DeserializeObject<string[]>(content); // Deserialize the string array form the response.
                return trackArray;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return Array.Empty<string>();
            }
        }
        /// <summary>
        /// Method requests the game server to start the Assetto Corsa server with the given parameters.
        /// </summary>
        /// <param name="track">Optional track name</param>
        /// <param name="trackConfiguration">Optional track configuration</param>
        /// <returns>Boolean value indicating the success</returns>
        public async Task<bool> StartAssettoCorsaServer(string? track, string? trackConfiguration)
        {
            const string apiRoute = "api/assetto/start";

            try
            {
                _logger.LogDebug("Sending start assetto corsa server request.");

                HttpContent? content = null;
                // Set content if required. Otherwise send null content.
                if (track != null)
                {
                    var payload = new AssettoCorsaTrackConfiguration(track, trackConfiguration); // Create payload object.
                    var payloadStr = payload.ToJsonString();
                    _logger.LogDebug("Payload: {0}", payloadStr);
                    content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");
                }
                
                var response = await _gameServerClient.PostAsync(apiRoute, content);
                _logger.LogDebug("Response code: {0}", response.StatusCode);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Successful response");
                    return true;
                }

                _logger.LogError("Unsuccessful response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return false;
            }
        
        }
        /// <summary>
        /// Method requests the game server to restart the Assetto Corsa server with the given parameters.
        /// </summary>
        /// <param name="track">Optional track name</param>
        /// <param name="trackConfiguration">Optional track configuration</param>
        /// <returns>Boolean value indicating the success</returns>
        public async Task<bool> RestartAssettoCorsaServer(string? track, string? trackConfiguration)
        {
            const string apiRoute = "api/assetto/restart";

            try
            {
                _logger.LogDebug("Sending restart assetto corsa server request.");

                HttpContent? content = null;
                // Set content if required. Otherwise send null content.
                if (track != null)
                {
                    var payload = new AssettoCorsaTrackConfiguration(track, trackConfiguration); // Create payload object.
                    var payloadStr = payload.ToJsonString();
                    _logger.LogDebug("Payload: {0}", payloadStr);
                    content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");
                }

                var response = await _gameServerClient.PostAsync(apiRoute, content);
                _logger.LogDebug("Response code: {0}", response.StatusCode);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Successful response");
                    return true;
                }

                _logger.LogError("Unsuccessful response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return false;
            }
        }

        /// <summary>
        /// Method stops the Assetto Corsa server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopAssettoCorsaServer()
        {
            const string apiRoute = "api/assetto/stop";

            try
            {
                _logger.LogDebug("Sending stop assetto corsa server request.");
                var response = await _gameServerClient.PostAsync(apiRoute, null);
                _logger.LogDebug("Response code: {0}", response.StatusCode);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Successful response");
                    return true;
                }

                _logger.LogError("Unsuccessful response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return false;
            }
        }

        /// <summary>
        /// Method starts the Counter-Strike server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartCsServer()
        {
            const string apiRoute = "api/cs/start";

            try
            {
                _logger.LogDebug("Sending start cs server request.");
                var response = await _gameServerClient.PostAsync(apiRoute, null);
                _logger.LogDebug("Response code: {0}", response.StatusCode);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Successful response");
                    return true;
                }

                _logger.LogError("Unsuccessful response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return false;
            }
        }
        /// <summary>
        /// Method stops the Counter-Strike server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopCsServer()
        {
            const string apiRoute = "api/cs/stop";

            try
            {
                _logger.LogDebug("Sending stop cs server request.");
                var response = await _gameServerClient.PostAsync(apiRoute, null);
                _logger.LogDebug("Response code: {0}", response.StatusCode);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Successful response");
                    return true;
                }

                _logger.LogError("Unsuccessful response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return false;
            }
        }

        /// <summary>
        /// Method starts the Valheim server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartValheimServer()
        {
            const string apiRoute = "api/valheim/start";

            try
            {
                _logger.LogDebug("Sending start valheim server request.");
                var response = await _gameServerClient.PostAsync(apiRoute, null);
                _logger.LogDebug("Response code: {0}", response.StatusCode);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Successful response");
                    return true;
                }

                _logger.LogError("Unsuccessful response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return false;
            }
        }

        /// <summary>
        /// Method stops the Valheim server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopValheimServer()
        {
            const string apiRoute = "api/valheim/stop";

            try
            {
                _logger.LogDebug("Sending stop valheim server request.");
                var response = await _gameServerClient.PostAsync(apiRoute, null);
                _logger.LogDebug("Response code: {0}", response.StatusCode);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("Successful response");
                    return true;
                }

                _logger.LogError("Unsuccessful response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while performing request");
                return false;
            }
        }


        /// <summary>
        /// Class for TrackConfiguration body parameter.
        /// </summary>
        private class AssettoCorsaTrackConfiguration
        {
            [JsonProperty("track")]
            private readonly string _track;
            [JsonProperty("configuration")]
            private readonly string? _configuration;

            public AssettoCorsaTrackConfiguration(string track, string? configuration)
            {
                _track = track;
                _configuration = configuration;
            }

            public string ToJsonString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}
