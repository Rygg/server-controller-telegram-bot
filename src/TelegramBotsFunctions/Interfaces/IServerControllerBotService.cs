using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotsFunctions.Interfaces
{
    /// <summary>
    /// Interface for ServerControllerBotService implementations.
    /// </summary>
    public interface IServerControllerBotService
    {
        /// <summary>
        /// Task processes the <see cref="Update"/> received from the ServerController bot.
        /// </summary>
        /// <param name="updateObject">The received update object.</param>
        /// <returns>A boolean value representing the result of the operation.</returns>
        Task<bool> ProcessBotUpdateMessageAsync(Update updateObject);
    }
}
