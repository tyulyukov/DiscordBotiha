using System;

namespace DiscordBotiha
{
    public static class Debug
    {
        public static void Error(Exception ex)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ex.Message);
            Console.WriteLine($"--------------------\n{ex.StackTrace}\n--------------------");
        }

        public static void Log(String message)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
        }
    }
}
