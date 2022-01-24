using System.Threading.Tasks;

namespace TelegramBotsFunctions.Interfaces
{
    /// <summary>
    /// Interface for GameServerControllerService implementations.
    /// </summary>
    public interface IGameServerControllerService
    {
        /// <summary>
        /// Method requests the list of Assetto Corsa tracks available on the server.
        /// </summary>
        /// <returns>A list of available tracks</returns>
        Task<string[]> GetAssettoCorsaTracks();
        /// <summary>
        /// Method requests the game server to start the Assetto Corsa server with the given parameters.
        /// </summary>
        /// <param name="track">Optional track name</param>
        /// <param name="trackConfiguration">Optional track configuration</param>
        /// <returns>Boolean value indicating the success</returns>
        Task<bool> StartAssettoCorsaServer(string? track, string? trackConfiguration);
        /// <summary>
        /// Method requests the game server to restart the Assetto Corsa server with the given parameters.
        /// </summary>
        /// <param name="track">Optional track name</param>
        /// <param name="trackConfiguration">Optional track configuration</param>
        /// <returns>Boolean value indicating the success</returns>
        Task<bool> RestartAssettoCorsaServer(string? track, string? trackConfiguration);
        /// <summary>
        /// Method stops the Assetto Corsa server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        Task<bool> StopAssettoCorsaServer();
        /// <summary>
        /// Method starts the Counter-Strike server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        Task<bool> StartCsServer();
        /// <summary>
        /// Method stops the Counter-Strike server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        Task<bool> StopCsServer();
        /// <summary>
        /// Method starts the Valheim server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        Task<bool> StartValheimServer();
        /// <summary>
        /// Method stops the Valheim server and returns boolean indicating the operation success.
        /// </summary>
        /// <returns></returns>
        Task<bool> StopValheimServer();
    }
}
