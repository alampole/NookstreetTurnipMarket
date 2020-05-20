using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NookstreetTurnipMarket.Data;

namespace NookstreetTurnipMarket.Commands
{
    public class UserCommands : BaseCommandModule
    {
        [Command("Register"), Description("Used to register yourself into the database. This allows you to upload your prices.")]
        public async Task RegisterUser(CommandContext aContext, [Description("The name of your villager.")]string VillagerName, [Description("The name of your island.")]string IslandName)
        {
            if (aContext.Guild == null)
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync("Server only command.");
            }
            else
            {
                if (!DatabaseManager.UserExists(aContext.Guild.Id, aContext.User.Id))
                {
                    DatabaseManager.RegisterUser(aContext.Guild.Id, aContext.User.Id, VillagerName, IslandName);

                    if (DatabaseManager.UserExists(aContext.Guild.Id, aContext.User.Id))
                    {
                        await aContext.Channel.SendMessageAsync(aContext.User.Username + "'s island is now registered!").ConfigureAwait(false);
                    }
                    else
                    {
                        await aContext.Channel.SendMessageAsync("Unknown error registering " + aContext.User.Username + "'s island.").ConfigureAwait(false);
                    }
                }
                else
                {
                    await aContext.Channel.SendMessageAsync("Already registered!").ConfigureAwait(false);
                }
            }
        }

        [Command("edituser"), Description("Used to edit your island name and villager name within the database.")]
        public async Task EditUser(CommandContext aContext, [Description("The name of your villager.")]string aVillagerName, [Description("The name of your island.")]string aIslandName)
        {
            if (aContext.Guild == null)
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync("Server only command.");
            }
            {
                if (DatabaseManager.UserExists(aContext.Guild.Id, aContext.User.Id))
                {
                    DatabaseManager.EditUser(aContext.Guild.Id, aContext.User.Id, aVillagerName, aIslandName);

                    await aContext.Channel.SendMessageAsync("Profile edited!").ConfigureAwait(false);
                }
                else
                {
                    await aContext.Channel.SendMessageAsync("Please register before attempting to edit profile!").ConfigureAwait(false);
                }
            }
        }

        [Command("reportbug"), Description("Used to report a bug. Gives a small sheet to fill out to get information needed to fix the bug.")]
        public async Task ReportBugCommand(CommandContext aContext)
        {
            DiscordMember member = GuildManager.GetMember(aContext.User.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "Bug Report Form";
            builder.WithUrl("https://forms.gle/P6XoweYBs4WrRLJL9");

            await member.SendMessageAsync("Thank you for reporting a bug, please fill out this quick form with as much information as possible.", false, builder);
        }

        [Command("Feedback"), Description("Provides a short form to fill out to give your feedback.")]
        public async Task FeedbackCommand(CommandContext aContext)
        {
            DiscordMember member = GuildManager.GetMember(aContext.User.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "Feedback Form";
            builder.WithUrl("https://forms.gle/fyyFry91qVfFdqpcA");

            await member.SendMessageAsync("Thank you for taking your time to give feedback! Below is the feeback form, all comments welcome!", false, builder);
        }
    }
}