using Microsoft.Extensions.DependencyInjection;
using TelegramBotsFunctionsApp.Interfaces;
using TelegramBotsFunctionsApp.Services;

namespace TelegramBotsFunctionsApp.Extensions
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
        }

        internal static void AddHttpClients(this IServiceCollection serviceCollection)
        {
            // TODO: Add clients.
        }
    }
}
