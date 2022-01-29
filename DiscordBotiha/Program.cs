using System;

namespace DiscordBotiha
{
    public class Program
    {
        private static DiscordBot bot = new DiscordBot();

        private static void Main() => bot.Start().GetAwaiter().GetResult();
    }
}
