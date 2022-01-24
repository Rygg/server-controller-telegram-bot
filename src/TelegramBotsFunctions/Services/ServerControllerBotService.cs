using System;
using System.Linq;
using System.Text;
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
                        await GetAssettoCorsaTracks(message.Chat);
                        break;
                    case SupportedCommands.StartAcServer:
                        CheckUserAuthorized(message.From);
                        await StartAssettoCorsaServer(message.Chat, parameters);
                        break;
                    case SupportedCommands.RestartAcServer:
                        CheckUserAuthorized(message.From);
                        await RestartAssettoCorsaServer(message.Chat, parameters);
                        break;
                    case SupportedCommands.StopAcServer:
                        CheckUserAuthorized(message.From);
                        await StopAssettoCorsaServer(message.Chat);
                        break;
                    case SupportedCommands.StartCsServer:
                        CheckUserAuthorized(message.From);
                        await StartCounterStrikeServer(message.Chat);
                        break;
                    case SupportedCommands.StopCsServer:
                        CheckUserAuthorized(message.From);
                        await StopCounterStrikeServer(message.Chat);
                        break;
                    case SupportedCommands.StartValheimServer:
                        CheckUserAuthorized(message.From);
                        await StartValheimServer(message.Chat);
                        break;
                    case SupportedCommands.StopValheimServer:
                        CheckUserAuthorized(message.From);
                        await StopValheimServer(message.Chat);
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
        /// Get assetto corsa tracks.
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        private async Task GetAssettoCorsaTracks(Chat chat)
        {
            _logger.LogDebug("Requesting Assetto Corsa tracks");
            var tracks = await _gameServerControllerService.GetAssettoCorsaTracks();
            if (!tracks.Any())
            {
                _logger.LogInformation("No tracks available");
                await _botClient.SendTextMessageAsync(chat.Id, "No tracks available");
                return;
            }
            var responseBuilder = new StringBuilder();
            responseBuilder.Append("*Available tracks on the server:*" + Environment.NewLine + Environment.NewLine + "`");
            foreach (var track in tracks)
            {
                responseBuilder.Append(track + Environment.NewLine); // Add each track to the string.
            }
            responseBuilder.Append("`");
            // Send response to the user.
            await _botClient.SendTextMessageAsync(chat.Id, responseBuilder.ToString(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
            _logger.LogInformation("Responded the list of tracks to user.");
        }

        /// <summary>
        /// Start Assetto Corsa server with optional parameters and respond accordingly to the user.
        /// </summary>
        /// <param name="chat">Chat to send response to</param>
        /// <param name="parameters">Parameters to use when starting the server</param>
        private async Task StartAssettoCorsaServer(Chat chat, string[] parameters)
        {
            var track = parameters.ElementAtOrDefault(0);
            var trackConfiguration = parameters.ElementAtOrDefault(1);
            _logger.LogDebug("Requesting Assetto Corsa server start with parameters: {0}, {1}", track, trackConfiguration);
            var result = await _gameServerControllerService.StartAssettoCorsaServer(track, trackConfiguration);
            if (result)
            {
                _logger.LogInformation("Server started");
                await _botClient.SendTextMessageAsync(chat.Id, "Assetto Corsa server started.");
                return;
            }
            _logger.LogError("Starting server failed");
            await _botClient.SendTextMessageAsync(chat.Id, "Failed to start server.");
        }

        /// <summary>
        /// Restart Assetto Corsa server with optional parameters and respond accordingly to the user.
        /// </summary>
        /// <param name="chat">Chat to send response to</param>
        /// <param name="parameters">Parameters to use when restarting the server</param>
        private async Task RestartAssettoCorsaServer(Chat chat, string[] parameters)
        {
            var track = parameters.ElementAtOrDefault(0);
            var trackConfiguration = parameters.ElementAtOrDefault(1);
            _logger.LogDebug("Requesting Assetto Corsa server restart with parameters: {0}, {1}", track, trackConfiguration);
            var result = await _gameServerControllerService.RestartAssettoCorsaServer(track, trackConfiguration);
            if (result)
            {
                _logger.LogInformation("Server restarted");
                await _botClient.SendTextMessageAsync(chat.Id, "Assetto Corsa server restarted.");
                return;
            }
            _logger.LogError("Restarting server failed");
            await _botClient.SendTextMessageAsync(chat.Id, "Failed to restart server.");
        }
        /// <summary>
        /// Stop Assetto Corsa server and respond accordingly.
        /// </summary>
        /// <param name="chat">Chat to reply to</param>
        private async Task StopAssettoCorsaServer(Chat chat)
        {
            _logger.LogDebug("Stopping Assetto Corsa server");
            var result = await _gameServerControllerService.StopAssettoCorsaServer();
            if (result)
            {
                _logger.LogInformation("Server stopped");
                await _botClient.SendTextMessageAsync(chat.Id, "Assetto Corsa server stopped.");
                return;
            }
            _logger.LogError("Stopping server failed");
            await _botClient.SendTextMessageAsync(chat.Id, "Failed to stop server.");
        }
        /// <summary>
        /// Start Counter-Strike server and respond accordingly.
        /// </summary>
        /// <param name="chat">Chat to reply to</param>
        private async Task StartCounterStrikeServer(Chat chat)
        {
            _logger.LogDebug("Starting Counter-Strike server");
            var result = await _gameServerControllerService.StartCsServer();
            if (result)
            {
                _logger.LogInformation("Server started");
                await _botClient.SendTextMessageAsync(chat.Id, "Counter-Strike server started.");
                return;
            }
            _logger.LogError("Starting server failed");
            await _botClient.SendTextMessageAsync(chat.Id, "Failed to start server.");
        }

        /// <summary>
        /// Stop Counter-Strike server and respond accordingly.
        /// </summary>
        /// <param name="chat">Chat to reply to</param>
        private async Task StopCounterStrikeServer(Chat chat)
        {
            _logger.LogDebug("Stopping Counter-Strike server");
            var result = await _gameServerControllerService.StopCsServer();
            if (result)
            {
                _logger.LogInformation("Server stopped");
                await _botClient.SendTextMessageAsync(chat.Id, "Counter-Strike server stopped.");
                return;
            }
            _logger.LogError("Stopping server failed");
            await _botClient.SendTextMessageAsync(chat.Id, "Failed to stop server.");
        }
        /// <summary>
        /// Start Valheim server and respond accordingly.
        /// </summary>
        /// <param name="chat">Chat to reply to</param>
        private async Task StartValheimServer(Chat chat)
        {
            _logger.LogDebug("Starting Valheim server");
            var result = await _gameServerControllerService.StartValheimServer();
            if (result)
            {
                _logger.LogInformation("Server started");
                await _botClient.SendTextMessageAsync(chat.Id, "Valheim server started.");
                return;
            }
            _logger.LogError("Starting server failed");
            await _botClient.SendTextMessageAsync(chat.Id, "Failed to start server.");
        }

        /// <summary>
        /// Stop Valheim server and respond accordingly.
        /// </summary>
        /// <param name="chat">Chat to reply to</param>
        private async Task StopValheimServer(Chat chat)
        {
            _logger.LogDebug("Stopping Valheim server");
            var result = await _gameServerControllerService.StopValheimServer();
            if (result)
            {
                _logger.LogInformation("Server stopped");
                await _botClient.SendTextMessageAsync(chat.Id, "Valheim server stopped.");
                return;
            }
            _logger.LogError("Stopping server failed");
            await _botClient.SendTextMessageAsync(chat.Id, "Failed to stop server.");
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
