using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotsFunctionsApp.Interfaces;

namespace TelegramBotsFunctionsApp.Services
{
    /// <summary>
    /// Service-class implementing the <see cref="IServerControllerBotService"/>-interface.
    /// </summary>
    public class ServerControllerBotService : IServerControllerBotService
    {
        /// <summary>
        /// Injected logger.
        /// </summary>
        private readonly ILogger<ServerControllerBotService> _logger;
        /// <summary>
        /// ServerController telegram bot client.
        /// </summary>
        private readonly ITelegramBotClient _botClient;
        /// <summary>
        /// Constructor for the service.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        /// <exception cref="ArgumentNullException">A required application setting was not located.</exception>
        public ServerControllerBotService(ILogger<ServerControllerBotService> logger)
        {
            _logger = logger;

            var serverControllerBotToken = Environment.GetEnvironmentVariable("ServerControllerBotToken");
            if (serverControllerBotToken == null)
            {
                throw new ArgumentNullException(nameof(serverControllerBotToken), "Token not found from application settings.");
            }

            _botClient = new TelegramBotClient(serverControllerBotToken);
        }
        /// <summary>
        /// Task processes the <see cref="Update"/> received from the ServerController bot.
        /// </summary>
        /// <param name="updateObject">The received update object.</param>
        /// <returns>An object representing the result of the operation.</returns>
        public Task<object> ProcessBotUpdateMessage(Update updateObject)
        {
            throw new NotImplementedException();
        }
    }
}
