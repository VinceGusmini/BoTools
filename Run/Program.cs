using BoTools.Service;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoTools.Run
{
    public class Program
    {
        /*
                             Tips / Good practice

        1.  I_The program  (initialization and command handler) + II_The modules (handle commands)
            + III_The services (persistent storage, pure functions, data manipulation)

        2.  Events are executed synchronously off the gateway task in the same context as the gateway task.
            As a side effect, this makes it possible to deadlock the gateway task and kill a connection.
            Any task that takes longer than three seconds should not be awaited directly in the context of an event
            but should be wrapped in a Task.Run or offloaded to another task.

            This also means that you should not await a task that requests data from Discord's gateway in the same context of an event.
            Since the gateway will wait on all invoked event handlers to finish before processing any additional data from the gateway,
            this will create a deadlock that will be impossible to recover from.

            Exceptions in commands will be swallowed by the gateway and logged out through the client's log method.
        */

        private CommandHandler _commands;
        private DiscordSocketClient _client;
        private readonly string _token = Environment.GetEnvironmentVariable("BoTools_Token");
        private readonly string _link = "https://www.twitch.tv/vince_zder";        
        private static readonly string _ngrokPath = @"D:\Apps\Ngrok\ngrok.exe";
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();


        public Program(DiscordSocketClient client = null)
        {
            // When working with events that have Cacheable<IMessage, ulong> parameters, you must enable
            // the message cache in your config settings if you plan to use the cached message entity.            
            _client = client ?? new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 100 });
            _client.SetGameAsync(name: ": $Jellyfin", streamUrl: _link, type: ActivityType.CustomStatus); 

            _commands ??= new CommandHandler(_client, new CommandService(), BuildServiceProvider());
        }

        public async Task MainAsync()
        {
            LoadLogConfig();

            await _commands.InitializeCommandsAsync();
            Console.WriteLine("[InstallCommandsAsync : done] LET'S GO !");

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(Timeout.Infinite);
        }

        /// <summary>
        /// Inject Services
        /// </summary>
        /// <returns></returns>
        public IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new MessageService(_client))
                .AddSingleton(new LogService(_client))
                .AddSingleton(new RoleService(_client))
                .AddSingleton(new JellyfinService());

            return services.BuildServiceProvider();
        }


        private static void LoadLogConfig()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
    }
}