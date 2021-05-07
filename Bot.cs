using DBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            // Читаем файл конфигурации
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration()
            { 
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                
            };

            Client = new DiscordClient(config);
            
            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration
            { 
                Timeout = TimeSpan.FromMinutes(2)
            });

            // Настраиваем бота
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {configJson.Prefix},
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true
            };

            // Инициализируем команды
            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<TeamCommands>();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
