using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using TelegramBotsFunctionsApp;
using TelegramBotsFunctionsApp.Extensions;

[assembly: FunctionsStartup(typeof(Startup))]
namespace TelegramBotsFunctionsApp
{
    /// <summary>
    /// Startup configuration to support dependency injections.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configure the Functions App on start up.
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            services.AddHttpClients(); // Register http clients
            services.AddServices(); // Register services to interfaces.
        }
    }
}
