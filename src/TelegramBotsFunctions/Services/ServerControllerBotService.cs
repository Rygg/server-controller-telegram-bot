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
        /// Injected game server controller service.
        /// </summary>
        private readonly IGameServerControllerService _gameServerControllerService;
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
        public ServerControllerBotService(ILogger<ServerControllerBotService> logger, IVirtualMachineService virtualMachineService, IGameServerControllerService gameServerService)
        {
            _logger = logger;
            _virtualMachineService = virtualMachineService;
            _gameServerControllerService = gameServerService;

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
                        await StartVirtualMachine(message.Chat);
                        break;
                    case SupportedCommands.StopVirtualMachine:
                        CheckUserAuthorized(message.From);
                        await StopVirtualMachine(message.Chat);
                        break;
                    case SupportedCommands.GetVirtualMachineStatus: // Status supported without authorization.
                        await GetVirtualMachineStatus(message.Chat);
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

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing bot update message.");
                return false;
            }
        }

        /// <summary>
        /// Method handles starting the virtual machine.
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task StartVirtualMachine(Chat chat)
        {
            // Currently no need for parameters.
            _logger.LogDebug("Sending request to start virtual machine");
            var operationCompleted = await _virtualMachineService.StartVirtualMachineAsync();
            if (operationCompleted)
            {
                _logger.LogInformation("Virtual machine started.");
                await _botClient.SendTextMessageAsync(chat.Id, "VM started");
                return;
            }
            // Otherwise, operation started asynchronously. Start task with callback to send message to the user.
            // Use callback because telegram will keep retrying the same message with a short timeout until it gets a reply.
            _ = _virtualMachineService.GetAsynchronousOperationResultAsync(EVirtualMachineRequestType.StartRequest).ContinueWith(async t =>
            {
                try
                {
                    await t; // NOTE: This can cause lots of computation time. For a project of this scale it shouldn't matter.
                    _logger.LogInformation("Virtual machine started.");
                    await _botClient.SendTextMessageAsync(chat.Id, "VM started");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while waiting for asynchronous operation result.");
                    await _botClient.SendTextMessageAsync(chat.Id, "Operation failed.");
                }
            });
            _logger.LogDebug("Virtual machine starting..  ");
            await _botClient.SendTextMessageAsync(chat.Id, "Operation started. Please wait for confirmation.");
            
        }

        /// <summary>
        /// Stop the virtual machine.
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        private async Task StopVirtualMachine(Chat chat)
        {
            // Currently no need for parameters.
            _logger.LogDebug("Sending request to stop the virtual machine");
            var operationCompleted = await _virtualMachineService.StopVirtualMachineAsync();
            if (operationCompleted)
            {
                _logger.LogInformation("Virtual machine stopped.");
                await _botClient.SendTextMessageAsync(chat.Id, "VM stopped");
                return;
            }
            // Otherwise, operation started asynchronously. Start task with callback to send message to the user.
            // Use callback because telegram will keep retrying the same message with a short timeout until it gets a reply.
            _ = _virtualMachineService.GetAsynchronousOperationResultAsync(EVirtualMachineRequestType.StopRequest).ContinueWith(async t =>
            {
                try
                {
                    await t; // NOTE: This can cause lots of computation time. For a project of this scale it shouldn't matter.
                    _logger.LogInformation("Virtual machine stopped.");
                    await _botClient.SendTextMessageAsync(chat.Id, "VM stopped.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while waiting for asynchronous operation result.");
                    await _botClient.SendTextMessageAsync(chat.Id, "Operation failed.");
                }
            });
            _logger.LogDebug("Virtual machine stopping..  ");
            await _botClient.SendTextMessageAsync(chat.Id, "Operation started. Please wait for confirmation.");
        }

        /// <summary>
        /// Get virtual machine status.
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        private async Task GetVirtualMachineStatus(Chat chat)
        {
            _logger.LogDebug("Requesting virtual machine status");
            var (operationCompleted, status) = await _virtualMachineService.GetVirtualMachineStatusAsync();
            if (operationCompleted)
            {
                _logger.LogInformation("VM status retrieved. Status: {0}", status);
                await _botClient.SendTextMessageAsync(chat.Id, $"Status: {status}");
                return;
            }
            // Otherwise, operation started asynchronously. Start task with callback to send message to the user.
            // Use callback because telegram will keep retrying the same message with a short timeout until it gets a reply.
            _ = _virtualMachineService.GetAsynchronousOperationResultAsync(EVirtualMachineRequestType.StatusRequest).ContinueWith(async t =>
            {
                try
                {
                    status = (string)await t; // NOTE: This can cause lots of computation time. For a project of this scale it shouldn't matter.
                    _logger.LogInformation("VM status retrieved. Status: {0}", status);
                    await _botClient.SendTextMessageAsync(chat.Id, $"Status: {status}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while waiting for asynchronous operation result.");
                    await _botClient.SendTextMessageAsync(chat.Id, "Operation failed.");
                }
            }); 
            _logger.LogDebug("Retrieving virtual machine status..");
            await _botClient.SendTextMessageAsync(chat.Id, "Retrieving status..");

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
