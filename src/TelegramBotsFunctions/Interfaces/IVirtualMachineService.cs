using System.Threading.Tasks;

namespace TelegramBotsFunctions.Interfaces
{
    /// <summary>
    /// Interface for methods controlling the virtual machine.
    /// </summary>
    public interface IVirtualMachineService
    {
        /// <summary>
        /// Checks if the virtual machine is running.
        /// </summary>
        /// <returns>Virtual machine state.</returns>
        Task<bool> IsVirtualMachineRunningAsync();
        /// <summary>
        /// Starts the virtual machine. 
        /// </summary>
        /// <returns>A boolean value representing the success of the operation.</returns>
        Task<bool> StartVirtualMachineAsync();
        /// <summary>
        /// Stops the virtual machine.
        /// </summary>
        /// <returns>A boolean value representing the success of the operation.</returns>
        Task<bool> StopVirtualMachineAsync();
    }
}
