using System;
using System.Threading.Tasks;

namespace DiscordBotiha
{
    public class Program
    {
        private static DiscordBot bot = new DiscordBot();

        private async static Task Main() => await bot.Start();
    }
}
