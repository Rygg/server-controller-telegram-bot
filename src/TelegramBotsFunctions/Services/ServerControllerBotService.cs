using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotsFunctions.Interfaces;

namespace TelegramBotsFunctions.Services
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
        /// <returns>A boolean value representing the result of the operation.</returns>
        public async Task<bool> ProcessBotUpdateMessage(Update updateObject)
        {
            var message = updateObject.Message;
            if (message?.Text == null)
            {
                _logger.LogInformation("Update contained no valid message. Not handling.");
                return false;
            }
            _logger.LogInformation("Received message: '{0}' from telegram user. UserId: {1}, UserName: {2}", message.Text, message.From?.Id, message.From?.Username);

            var splitMessageText = message.Text.Split(null); // Split at whitespaces.
            var command = splitMessageText[0]; // Get the command from the first part of the command.
            var parameters = splitMessageText.Skip(1).ToArray(); // Get the rest as parameters.

            // Check for supported command.
            switch (command)
            {
                case SupportedCommands.StartVirtualMachine:
                    break;
                case SupportedCommands.StopVirtualMachine:
                    break;
                case SupportedCommands.GetVirtualMachineStatus:
                    break;
                case SupportedCommands.GetAcTracks:
                    break;
                case SupportedCommands.StartAcServer:
                    break;
                case SupportedCommands.StopAcServer:
                    break;
                case SupportedCommands.RestartAcServer:
                    break;
                case SupportedCommands.StartCsServer:
                    break;
                case SupportedCommands.StopCsServer:
                    break;
                case SupportedCommands.StartValheimServer:
                    break;
                case SupportedCommands.StopValheimServer:
                    break;
                default:
                    _logger.LogError("Unsupported command: {0}.", command);
                    return false;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Private class containing the supported command strings.
        /// </summary>
        private static class SupportedCommands
        {
            /// <summary>
            /// Command for starting the virtual machine.
            /// </summary>
            internal const string StartVirtualMachine = "/vm_start";
            /// <summary>
            /// Command for stopping the virtual machine.
            /// </summary>
            internal const string StopVirtualMachine = "/vm_stop";
            /// <summary>
            /// Command for getting the virtual machine status.
            /// </summary>
            internal const string GetVirtualMachineStatus = "/vm_status";
            /// <summary>
            /// Command for retrieving available tracks from the Assetto Corsa server.
            /// </summary>
            internal const string GetAcTracks = "/tracks_ac";
            /// <summary>
            /// Command for starting the Assetto Corsa server.
            /// </summary>
            internal const string StartAcServer = "/start_ac";
            /// <summary>
            /// Command for restarting the Assetto Corsa server.
            /// </summary>
            internal const string RestartAcServer = "/restart_ac";
            /// <summary>
            /// Command for stopping the Assetto Corsa server.
            /// </summary>
            internal const string StopAcServer = "/stop_ac";
            /// <summary>
            /// Command for starting the Counter-Strike server.
            /// </summary>
            internal const string StartCsServer = "/start_cs";
            /// <summary>
            /// Command for stopping the Counter-Strike server.
            /// </summary>
            internal const string StopCsServer = "/stop_cs";
            /// <summary>
            /// Command for starting the Valheim server.
            /// </summary>
            internal const string StartValheimServer = "/start_valheim";
            /// <summary>
            /// Command for stopping the Valheim server.
            /// </summary>
            internal const string StopValheimServer = "/stop_valheim";
        }
    }
}
