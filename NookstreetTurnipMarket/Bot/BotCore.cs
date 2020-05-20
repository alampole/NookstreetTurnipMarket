using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using NookstreetTurnipMarket.Commands;
using NookstreetTurnipMarket.Data;

namespace NookstreetTurnipMarket.Bot
{
    class BotCore
    {
        private DiscordClient m_Client;
        private CommandsNextExtension m_CommandConfig;

        public DiscordClient Client { get { return m_Client; } }
        public CommandsNextExtension CommandConfig { get { return m_CommandConfig; } }

        public async Task RunAsync()
        {
            DiscordConfiguration config = new DiscordConfiguration
            {
                //Token goes here
                Token = "",
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            m_Client = new DiscordClient(config);

            m_Client.Ready += ClientReady;
            m_Client.GuildCreated += OnServerJoin;

            CommandsNextConfiguration commandConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "!" },
                EnableMentionPrefix = true,
                EnableDms = true,
                DmHelp = true,
                IgnoreExtraArguments = true,
            };

            m_CommandConfig = m_Client.UseCommandsNext(commandConfig);

            m_CommandConfig.RegisterCommands<TestCommand>();
            m_CommandConfig.RegisterCommands<TurnipCommands>();
            m_CommandConfig.RegisterCommands<UserCommands>();

            await m_Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnServerJoin(GuildCreateEventArgs e)
        {
            DatabaseManager.CreateTable(e.Guild.Id);

            return Task.CompletedTask;
        }

        private Task ClientReady(ReadyEventArgs e)
        {
            Console.WriteLine("Client ready. Updating database.");

            foreach (KeyValuePair<ulong, DiscordGuild> guild in e.Client.Guilds)
            {
                if (!DatabaseManager.TableExists(guild.Key))
                {
                    DatabaseManager.CreateTable(guild.Key);
                }

                DatabaseManager.UpdateTable(guild.Key);
            }

            foreach (KeyValuePair<ulong, DiscordGuild> guild in m_Client.Guilds)
            {
                GuildManager.Guilds.Add(new GuildInfo(guild.Value));
            }

            Console.WriteLine("Update finished.");

            return Task.CompletedTask;
        }
    }
}
