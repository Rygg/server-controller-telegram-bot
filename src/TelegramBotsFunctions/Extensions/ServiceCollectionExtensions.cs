using Microsoft.Extensions.DependencyInjection;
using TelegramBotsFunctions.Interfaces;
using TelegramBotsFunctions.Services;

namespace TelegramBotsFunctions.Extensions
{
    /// <summary>
    /// Extensions for ServiceCollection.
    /// </summary>
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add services.
        /// </summary>
        /// <param name="serviceCollection"></param>
        internal static void AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IServerControllerBotService, ServerControllerBotService>();
            serviceCollection.AddTransient<IVirtualMachineService, VirtualMachineService>();
        }

        internal static void AddHttpClients(this IServiceCollection serviceCollection)
        {
            // TODO: Add clients.
        }
    }
}
