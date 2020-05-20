using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NookstreetTurnipMarket.Data;
using NookstreetTurnipMarket.Helper;

namespace NookstreetTurnipMarket.Commands
{
    class TurnipCommands : BaseCommandModule
    {
        [Command("CurrentPrice"), Description("Sets your current price so that it can be used to find the top prices.")]
        public async Task CurrentPrice
            (
                CommandContext aContext, 
                [Description("If the price is for buying or sell. Arguments are buy or sell.")]string PriceType, 
                [Description("The current price.")]int Price, 
                [Description("Your current island time. Ex: 11:11am")]string CurrentIslandTime
            )
        {
            if (aContext.Guild != null)
            {
                if (DatabaseManager.UserExists(aContext.Guild.Id, aContext.User.Id))
                {
                    if (Price < 0 || Price > 700)
                    {
                        await aContext.Channel.SendMessageAsync("Invalid price.");
                        return;
                    }

                    int hours = 0;
                    int minutes = 0;

                    string error = TimeHelper.SanitizeTime(CurrentIslandTime, out hours, out minutes);

                    if (error == string.Empty)
                    {
                        DateTime islandTime = DateTime.Now.Date + new TimeSpan(hours, minutes, 0);
                        if (PriceType.ToLower() == "buy")
                            DatabaseManager.EditCurrentTurnipPrices(aContext.Guild.Id, aContext.User.Id, Price, 0, islandTime);
                        if (PriceType.ToLower() == "sell")
                            DatabaseManager.EditCurrentTurnipPrices(aContext.Guild.Id, aContext.User.Id, 0, Price, islandTime);

                        await aContext.Channel.SendMessageAsync(string.Format("Turnip {0} price set to {1}", PriceType.ToLower(), Price)).ConfigureAwait(false);
                    }
                    else
                    {
                        await aContext.Channel.SendMessageAsync(error);
                    }
                }
                else
                {
                    await aContext.Channel.SendMessageAsync("Use !Register command before trying to add turnip buy price!").ConfigureAwait(false);
                }
            }
            else
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync("Server only command.");
            }
        }

        [Command("Top"), Description("Used to get the top prices for either buying or selling.")]
        public async Task TopPrices
            (
                CommandContext aContext,
                [Description("If the price is for buying or sell. Arguments are buy or sell.")]string PriceType, 
                [Description("The length of the list.")]int Length
            )
        {
            if (aContext.Guild == null)
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync("Server only command.");
            }
            else
            { 
                PriceType = PriceType.ToLower();

                if (PriceType == "buy" || PriceType == "sell")
                {
                    string topList = DatabaseManager.GetTop(aContext.Guild.Id, PriceType, Length);

                    if (topList == string.Empty)
                    {
                        await aContext.Channel.SendMessageAsync("No current " + PriceType + " set.").ConfigureAwait(false);
                    }
                    else
                    {
                        await aContext.Channel.SendMessageAsync("Current Top Prices \n```" + topList + "```").ConfigureAwait(false);
                    }
                }
                else
                {
                    await aContext.Channel.SendMessageAsync("Price Type must be either buy or sell.").ConfigureAwait(false);
                }
            }
        }

        [Command("Price"), Description("Sets the price for the specific time period. Will reset weekly prices if day is earlier than another price entered. Example: if you have a price on a Thursday and you use this command to set a price on Wednesday it will reset the prices since it assumes its a new week. To avoid resetting prices use !editprice.")]
        public async Task PriceCommand
            (
                CommandContext aContext,
                [Description("The current price.")]int Price, 
                [Description("The current day of the week. Ex: Monday, Tuesday, etc")]string CurrentDay,
                [Description("Your current island time. Ex: 11:11am")]string CurrentIslandTime
            )
        {
            if (aContext.Guild == null)
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync("Server only command.");
            }
            else
            {
                if (DatabaseManager.UserExists(aContext.Guild.Id, aContext.User.Id))
                {
                    if (Price < 0 || Price > 700)
                    {
                        await aContext.Channel.SendMessageAsync("Invalid price.");
                        return;
                    }

                    int hours = 0;
                    int minutes = 0;

                    string error = TimeHelper.SanitizeTime(CurrentIslandTime, out hours, out minutes);

                    if (error == string.Empty)
                    {
                        DateTime islandTime = DateTime.Now.Date + new TimeSpan(hours, minutes, 0);

                        Day day = EnumHelper.StringToDay(CurrentDay);
                        DayPeriod meridiem = DayPeriod.Unknown;

                        if ((CurrentIslandTime.ToLower().Contains("am") || CurrentIslandTime.ToLower().Contains("pm")) && CurrentIslandTime.Length > 5)
                        {
                            meridiem = EnumHelper.StringToDayPeriod(CurrentIslandTime.Substring(CurrentIslandTime.Length - 2));
                        }

                        if (meridiem == DayPeriod.AM)
                        {
                            if (hours < 8 || hours > 11)
                            {
                                await aContext.Channel.SendMessageAsync("Morning hours are only between 8 and 12.");
                                return;
                            }
                        }

                        if (meridiem == DayPeriod.PM)
                        {
                            if (hours > 9 && hours != 12)
                            {
                                await aContext.Channel.SendMessageAsync("Afternoon hours are only between 12 and 10.");
                                return;
                            }
                        }

                        if (day != Day.Unknown && meridiem != DayPeriod.Unknown)
                        {
                            DatabaseManager.SetDayPrice(aContext.Guild.Id, aContext.User.Id, Price, day, meridiem, islandTime);

                            await aContext.Channel.SendMessageAsync("Price for " + day.ToString() + " " + (meridiem == DayPeriod.AM ? "morning" : "afternoon") + " set to " + Price);
                        }
                        else
                        {
                            string errorOutput = string.Empty;

                            if (day == Day.Unknown)
                            {
                                errorOutput += "Incorrect day name. Examples of days: Monday, Tuesday, Wednesday, etc. ";
                            }

                            if (meridiem == DayPeriod.Unknown)
                            {
                                errorOutput += "Incorrect time, time must be formatted as shown: 11:11AM";
                            }

                            await aContext.Channel.SendMessageAsync(errorOutput);
                        }
                    }
                    else
                    {
                        await aContext.Channel.SendMessageAsync(error);
                    }
                }
                else
                {
                    await aContext.Channel.SendMessageAsync("Use !Register command before trying to add turnip buy price!").ConfigureAwait(false);
                }
            }
        }

        [Command("EditPrice"), Description("Edits the price for a specific time period. Will not reset the week's prices.")]
        public async Task EditPriceCommand
            (
                CommandContext aContext,
                [Description("The current price.")]int Price,
                [Description("The current day of the week. Ex: Monday, Tuesday, etc")]string CurrentDay,
                [Description("Your current island time. Ex: 11:11am")]string CurrentIslandTime
            )
        {
            if (aContext.Guild == null)
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync("Server only command.");
            }
            else
            {
                if (Price < 0 || Price > 700)
                {
                    await aContext.Channel.SendMessageAsync("Invalid price.");
                    return;
                }

                if (DatabaseManager.UserExists(aContext.Guild.Id, aContext.User.Id))
                {
                    int hours = 0;
                    int minutes = 0;

                    string error = TimeHelper.SanitizeTime(CurrentIslandTime, out hours, out minutes);

                    if (error == string.Empty)
                    {
                        DateTime islandTime = DateTime.Now.Date + new TimeSpan(hours, minutes, 0);

                        Day day = EnumHelper.StringToDay(CurrentDay);
                        DayPeriod meridiem = DayPeriod.Unknown;

                        if ((CurrentIslandTime.ToLower().Contains("am") || CurrentIslandTime.ToLower().Contains("pm")) && CurrentIslandTime.Length > 5)
                        {
                            meridiem = EnumHelper.StringToDayPeriod(CurrentIslandTime.Substring(CurrentIslandTime.Length - 2));
                        }

                        if (meridiem == DayPeriod.AM)
                        {
                            if (hours < 8 || hours > 11)
                            {
                                await aContext.Channel.SendMessageAsync("Morning hours are only between 8 and 12.");
                                return;
                            }
                        }

                        if (meridiem == DayPeriod.PM)
                        {
                            if (hours > 9 && hours != 12)
                            {
                                await aContext.Channel.SendMessageAsync("Afternoon hours are only between 12 and 10.");
                                return;
                            }
                        }

                        if (day != Day.Unknown && meridiem != DayPeriod.Unknown)
                        {
                            DatabaseManager.EditDayPrice(aContext.Guild.Id, aContext.User.Id, Price, day, meridiem, islandTime);

                            await aContext.Channel.SendMessageAsync("Price for " + day.ToString() + " " + (meridiem == DayPeriod.AM ? "morning" : "afternoon") + " set to " + Price);
                        }
                        else
                        {
                            string errorOutput = string.Empty;

                            if (day == Day.Unknown)
                            {
                                errorOutput += "Incorrect day name. Examples of days: Monday, Tuesday, Wednesday, etc. ";
                            }

                            if (meridiem == DayPeriod.Unknown)
                            {
                                errorOutput += "Incorrect time, time must be formatted as shown: 11:11AM";
                            }

                            await aContext.Channel.SendMessageAsync(errorOutput);
                        }
                    }
                    else
                    {
                        await aContext.Channel.SendMessageAsync(error);
                    }
                }
                else
                {
                    await aContext.Channel.SendMessageAsync("Use !Register command before trying to add turnip buy price!").ConfigureAwait(false);
                }
            }
        }

        [Command("WeekPrices"), Description("Gets your current prices and shows you your price predictions.")]
        public async Task WeekPricesCommand(CommandContext aContext)
        {
            DiscordGuild guild = aContext.Guild ?? GuildManager.GetGuildsUserBelongsTo(aContext.User.Id)[0];

            int[] prices = DatabaseManager.GetDayPrices(guild.Id, aContext.User.Id);

            string url = "https://ac-turnip.com/p-";

            for (int i = 0; i < prices.Length; i++)
            {
                url += "-" + (prices[i] == 0 ? string.Empty : prices[i].ToString());
            }

            url += ".png";

            if (aContext.Guild == null)
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync(url);
            }
            else
            {
                await aContext.Channel.SendMessageAsync(url);
            }
        }

        [Command("WeekPrices"), Description("Gets current prices for yourself or a user and shows you the predictions.")]
        public async Task WeekPricesCommands
            (
                CommandContext aContext, 
                [Description("The user you want to get prices for.")]DiscordMember aMember
            )
        {
            if (aContext.Guild == null)
            {
                DiscordMember member = GuildManager.GetMember(aContext.User.Id);
                await member.SendMessageAsync("Server only command.");
            }
            else
            {
                int[] prices = DatabaseManager.GetDayPrices(aContext.Guild.Id, aMember.Id);

                string url = "https://ac-turnip.com/p-";

                for (int i = 0; i < prices.Length; i++)
                {
                    url += "-" + (prices[i] == 0 ? string.Empty : prices[i].ToString());
                }

                url += ".png";

                await aContext.Channel.SendMessageAsync("<@!" + aMember.Id + ">" + "'s current prices:\n" + url);
            }
        }

        [Command("resetweekprices"), Description("Resets your prices for the week.")]
        public async Task ResetWeekPrices(CommandContext aContext)
        {
            DatabaseManager.ResetDayPrices(aContext.Guild.Id, aContext.User.Id);

            await aContext.Channel.SendMessageAsync("Reset complete.");
        }
    }
}