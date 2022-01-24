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
        /// Injected virtual machine service.
        /// </summary>
        private readonly IVirtualMachineService _virtualMachineService;
        /// <summary>
        /// ServerController telegram bot client.
        /// </summary>
        private readonly ITelegramBotClient _botClient;

        /// <summary>
        /// Constructor for the service.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        /// <param name="virtualMachineService">Injected virtual machine service.</param>
        /// <exception cref="ArgumentNullException">A required application setting was not located.</exception>
        public ServerControllerBotService(ILogger<ServerControllerBotService> logger, IVirtualMachineService virtualMachineService)
        {
            _logger = logger;
            _virtualMachineService = virtualMachineService;

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
        public async Task<bool> ProcessBotUpdateMessageAsync(Update updateObject)
        {
            try
            {
                var message = updateObject.Message;
                if (message?.Text == null)
                {
                    _logger.LogInformation("Update contained no valid message. Not handling.");
                    return false;
                }

                if (message.From == null)
                {
                    _logger.LogInformation("Message contained no sender. Not continuing.");
                    return false;
                }

                _logger.LogInformation("Received message: '{0}' from telegram user. UserId: {1}, UserName: {2}", message.Text, message.From.Id, message.From.Username);

                var splitMessageText = message.Text.Split(null); // Split at whitespaces.
                var command = splitMessageText[0]; // Get the command from the first part of the command.
                var parameters = splitMessageText.Skip(1).ToArray(); // Get the rest as parameters.

                // Check for supported command.
                switch (command)
                {
                    case SupportedCommands.StartVirtualMachine:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.StopVirtualMachine:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.GetVirtualMachineStatus: // Status supported without authorization.
                        var result = await _virtualMachineService.IsVirtualMachineRunningAsync();
                        break;
                    case SupportedCommands.GetAcTracks: // Track listing supported without authorization.
                        break;
                    case SupportedCommands.StartAcServer:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.StopAcServer:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.RestartAcServer:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.StartCsServer:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.StopCsServer:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.StartValheimServer:
                        CheckUserAuthorized(message.From);
                        break;
                    case SupportedCommands.StopValheimServer:
                        CheckUserAuthorized(message.From);
                        break;
                    default:
                        _logger.LogError("Unsupported command: {0}.", command);
                        return false;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing bot update message.");
                return false;
            }
        }

        /// <summary>
        /// Validate the message sender.
        /// </summary>
        /// <param name="user">User who sent the message.</param>
        /// <returns>Nothing if successful.</returns>
        /// <exception cref="ArgumentException">User was not authorized.</exception>
        /// <exception cref="ArgumentNullException">Required configurations not found.</exception>
        private void CheckUserAuthorized(User user)
        {
            var acceptedUserIds = Environment.GetEnvironmentVariable("AcceptedUserIds"); // Get acceptable userIds for restricted commands.
            if (acceptedUserIds == null)
            {
                throw new ArgumentNullException(nameof(acceptedUserIds), "Accepted User Ids missing from application settings");
            }

            var ids = acceptedUserIds.Split(';'); // Get accepted ids in an array.

            if (Array.Exists(ids, id => id.Equals(user.Id.ToString()))) // Check if parameter userId matches to an acceptable user id.
            {
                _logger.LogDebug("User authorized.");
                return;
            }

            _logger.LogWarning("User '{0}' not authorized.", user.Id);
            throw new ArgumentException("User not authorized.", nameof(user.Id));
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
