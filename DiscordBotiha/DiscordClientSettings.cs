using System;
using System.IO;
using System.Text.Json;

namespace DiscordBotiha
{
    public class DiscordClientSettings
    {
        public String Prefix { get; set; }
        public String Token { get; set; }

        public bool Serialize(String fileName)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                String jsonString = JsonSerializer.Serialize(this, options);
                File.WriteAllText(fileName, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Error(ex);
                return false;
            }
        }

        public static DiscordClientSettings Deserialize(String fileName)
        {
            try
            {
                return JsonSerializer.Deserialize<DiscordClientSettings>(File.ReadAllText(fileName));
            }
            catch (Exception ex)
            {
                Debug.Error(ex);
                return null;
            }
        }
    }
}
