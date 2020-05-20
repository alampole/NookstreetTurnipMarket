using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NookstreetTurnipMarket.Helper;

namespace NookstreetTurnipMarket.Commands
{
    public class TestCommand : BaseCommandModule
    {
        //[Command("HelloWorld")]
        public async Task HelloWorld(CommandContext aContext, DiscordMember aMember)
        {
            Console.WriteLine(aMember == null);
            await aContext.Channel.SendMessageAsync(aMember.Username + aMember.Discriminator).ConfigureAwait(false);
        }

        //[Command("Helloworld")]
        public async Task HelloWorld(CommandContext aContext)
        {
            await aContext.Channel.SendMessageAsync("Hello world").ConfigureAwait(false);
        }
    }
}
