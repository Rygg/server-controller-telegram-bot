using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TelegramBotsFunctionsApp.Interfaces;

namespace TelegramBotsFunctionsApp.APIs
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
        /// <param name="req">Request posted by the bot.</param>
        /// <param name="log">Injected logger.</param>
        /// <returns></returns>
        [FunctionName("ServerControllerBotWebhookEndpoint")]
        public async Task<IActionResult> ServerControllerBotWebhookEndpoint(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bots/serverController/webhookEndpoint")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation($"{nameof(ServerControllerBotWebhookEndpoint)} triggered.");
            try
            {
                Update updateObject;
                try
                {
                    var jsonContent = await req.ReadAsStringAsync(); // Read request content.
                    updateObject = JsonConvert.DeserializeObject<Update>(jsonContent); // Deserialize to the telegram update format.
                    log.LogInformation("Parsing message succeeded.");
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Exception occurred while parsing webhook content.");
                    return new BadRequestResult(); // Respond 400. The request content was invalid. The bot should retry.
                }

                // Process the received update and get the result object.
                var resultObject = await _serverControllerBotService.ProcessBotUpdateMessage(updateObject);
                // TODO: Check result
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
