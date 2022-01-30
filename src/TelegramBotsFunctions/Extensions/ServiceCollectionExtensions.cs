using System;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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
            serviceCollection.AddScoped<IGameServerControllerService, GameServerControllerService>();
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

            var gameServerUri = Environment.GetEnvironmentVariable("GameServerUri");
            if (string.IsNullOrWhiteSpace(gameServerUri))
            {
                throw new ArgumentNullException(nameof(gameServerUri), "URI for game server missing from application settings.");
            }
            var gameServerAuthKey = Environment.GetEnvironmentVariable("GameServerAuthKey");
            if (string.IsNullOrWhiteSpace(gameServerAuthKey))
            {
                throw new ArgumentNullException(nameof(gameServerAuthKey), "Authorization key for game server missing from application settings.");
            }

            // Hash without salt for now.
            var hashedAuthKeyBytes = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(gameServerAuthKey), new byte[16], 15000);
            var hashedAuthKey = Convert.ToBase64String(hashedAuthKeyBytes.GetBytes(32));
            
            serviceCollection.AddHttpClient("GameServerClient", c =>
            {
                c.BaseAddress = new Uri(gameServerUri);
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                c.DefaultRequestHeaders.Add("ApiKey", hashedAuthKey);
            });
        }
    }
}
