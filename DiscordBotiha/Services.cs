using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBotiha
{
    public abstract class Service
    {
        public static Service Instance { get; }

        protected static object locker = new object();
    }

    public static class ServicesCollection
    {
        private static List<Service> services = new List<Service>();

        public static T GetService<T>() where T : Service
        {
            return services.OfType<T>().FirstOrDefault();
        }

        public static void AddSingleton<T>(T service) where T : Service
        {
            if (services.OfType<T>().Count() > 0)
                return;

            services.Add(service);
        }
    }
}
