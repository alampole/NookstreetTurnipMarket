using System;
using System.Threading.Tasks;
using NookstreetTurnipMarket.Bot;
using NookstreetTurnipMarket.Data;

namespace NookstreetTurnipMarket
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            DatabaseManager.Connect();
            BotCore bot = new BotCore();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
