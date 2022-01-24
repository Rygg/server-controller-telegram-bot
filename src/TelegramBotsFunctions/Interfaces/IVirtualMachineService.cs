using System;
using System.Threading.Tasks;

namespace TelegramBotsFunctions.Interfaces
{
    /// <summary>
    /// Interface for methods controlling the virtual machine. Requires Transient or Scoped.
    /// </summary>
    public interface IVirtualMachineService
    {
        /// <summary>
        /// Checks the status of the virtual machine. Sends a request to the virtual machine.
        /// </summary>
        /// <returns>A tuple with the first value indicating the operation status. <br/>
        /// If the operation status returned is false, operation was only started. Use <see cref="GetAsynchronousOperationResultAsync"/> to get actual result. <br/>
        /// If the operation status returned is true, the operation result is stored in the second item of the tuple.
        /// </returns>
        /// <remarks>Note: This will cancel all other asynchronous operation tasks if asynchronous handling is required.</remarks>
        Task<Tuple<bool,string?>> GetVirtualMachineStatusAsync();
        /// <summary>
        /// Starts the virtual machine. Sends a request to the virtual machine, and returns a boolean indicating the operation status.
        /// </summary>
        /// <returns>A boolean value representing the if the operation succeeded. <br/>
        /// If the value returned is false, operation was only started. Use <see cref="GetAsynchronousOperationResultAsync"/> to get actual result.</returns>
        /// <remarks>Note: This will cancel all other asynchronous operation tasks if asynchronous handling is required.</remarks>
        Task<bool> StartVirtualMachineAsync();
        /// <summary>
        /// Stops the virtual machine. Sends a request to the virtual machine, and returns a boolean indicating the operation status.
        /// </summary>
        /// <returns>A boolean value representing the if the operation succeeded. <br/>
        /// If the value returned is false, operation was only started. Use <see cref="GetAsynchronousOperationResultAsync"/> to get actual result.</returns>
        /// <remarks>Note: This will cancel all other asynchronous operation tasks if asynchronous handling is required.</remarks>
        Task<bool> StopVirtualMachineAsync();
        /// <summary>
        /// Task returns the status of an asynchronously started operation. Operation must already be started.
        /// </summary>
        /// <returns>The actual result of the operation.</returns>
        Task<object> GetAsynchronousOperationResultAsync(EVirtualMachineRequestType type);
    }

    /// <summary>
    /// Enum containing possible values for asynchronous operations.
    /// </summary>
    public enum EVirtualMachineRequestType
    {
        StartRequest,
        StopRequest,
        StatusRequest
    }
}
