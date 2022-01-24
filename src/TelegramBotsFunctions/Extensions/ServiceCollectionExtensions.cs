using System;
using System.Net.Http.Headers;
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
            serviceCollection.AddScoped<IServerControllerBotService, ServerControllerBotService>();
            serviceCollection.AddScoped<IVirtualMachineService, VirtualMachineService>(); // Scoped or Transient required.
        }
        /// <summary>
        /// Add http clients.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal static void AddHttpClients(this IServiceCollection serviceCollection)
        {
            // Add logic app.
            var logicAppUri = Environment.GetEnvironmentVariable("LogicAppUri");
            if (string.IsNullOrWhiteSpace(logicAppUri))
            {
                throw new ArgumentNullException(nameof(logicAppUri), "URI for Logic App missing from application settings.");
            }
            serviceCollection.AddHttpClient("LogicAppClient", c =>
            {
                c.BaseAddress = new Uri(logicAppUri);
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // TODO: Add virtual machine client.
        }
    }
}
