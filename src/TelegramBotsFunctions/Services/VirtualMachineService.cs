using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TelegramBotsFunctions.Interfaces;

namespace TelegramBotsFunctions.Services
{
    /// <summary>
    /// Class implementing the functionality controlling the virtual machines.
    /// </summary>
    public class VirtualMachineService : IVirtualMachineService
    {
        /// <summary>
        /// Injected logger.
        /// </summary>
        private readonly ILogger<VirtualMachineService> _logger;
        /// <summary>
        /// Constructor for the service.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        public VirtualMachineService(ILogger<VirtualMachineService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsVirtualMachineRunningAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StartVirtualMachineAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StopVirtualMachineAsync()
        {
            throw new NotImplementedException();
        }
    }
}
