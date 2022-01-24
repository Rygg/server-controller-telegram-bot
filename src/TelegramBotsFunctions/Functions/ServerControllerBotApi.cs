using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TelegramBotsFunctions.Interfaces;

namespace TelegramBotsFunctions.Functions
{
    /// <summary>
    /// Class contains endpoints for ServerControllerBot requests.
    /// </summary>
    public class ServerControllerBotApi
    {
        /// <summary>
        /// Injected ServerControllerBotService.
        /// </summary>
        private readonly IServerControllerBotService _serverControllerBotService;

        /// <summary>
        /// Constructor for dependency injections.
        /// </summary>
        /// <param name="serverControllerBotService"></param>
        public ServerControllerBotApi(IServerControllerBotService serverControllerBotService)
        {
            _serverControllerBotService = serverControllerBotService;
        }

        /// <summary>
        /// Endpoint triggered by the ServerController bot.
        /// </summary>
        /// <param name="request">Request posted by the bot.</param>
        /// <param name="log">Injected logger.</param>
        /// <returns></returns>
        [FunctionName("ServerControllerBotWebhookEndpoint")]
        public async Task<IActionResult> ServerControllerBotWebhookEndpoint(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bots/serverController/webhookEndpoint")]
            HttpRequest request, ILogger log)
        {
            log.LogInformation($"{nameof(ServerControllerBotWebhookEndpoint)} triggered.");
            try
            {
                Update updateObject;
                try
                {
                    var jsonContent = await request.ReadAsStringAsync(); // Read request content.
                    updateObject = JsonConvert.DeserializeObject<Update>(jsonContent); // Deserialize to the telegram update format.
                    if (updateObject == null)
                    {
                        throw new ArgumentNullException(nameof(request), "Request payload was null.");
                    }
                    log.LogInformation("Request parsing succeeded.");
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Exception occurred while parsing webhook content.");
                    return new BadRequestResult(); // Respond 400. The request content was invalid. The bot should retry.
                }

                // Process the received update and get the result object.
                var operationSuccess = await _serverControllerBotService.ProcessBotUpdateMessageAsync(updateObject);
                // TODO: Handle result.
                return new OkResult(); // Respond 200.
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Exception occurred while processing request.");
                return new OkResult(); // Respond 200. Otherwise the bot will retry this message.
            }
        }
    }
}
