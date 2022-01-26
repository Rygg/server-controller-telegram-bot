# Server Controller: Telegram Bot
Repository contains the source code for an Azure Functions application implementing a webhook endpoint for a telegram bot.

The bot controls a virtual machine hosted in Azure which instead controls multiple game servers. 
The bot is able to start and deallocate the virtual machine, as well as requests its current status. If the virtual machine is running, the bot is able to start and stop multiple different game servers by preconfigured commands.

The virtual machine is controlled by performing requests to a Logic App running in Azure, which is responsible for actually controlling the virtual machine.

The game servers are controlled by a REST api running on the same virtual machine.

The project utilizes the [Telegram.Bot](https://www.nuget.org/packages/Telegram.Bot/) package to provide the bot functionality. Packages [Microsoft.Azure.Functions.Extensions](https://www.nuget.org/packages/Microsoft.Azure.Functions.Extensions/) and [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/) are used to provide support for dependency injections.
