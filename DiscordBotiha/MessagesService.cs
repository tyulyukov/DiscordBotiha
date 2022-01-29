using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBotiha
{
    public class MessagesService
    {
        private const int messageDeletingDelay = 5000;

        private Dictionary<IGuild, List<IUserMessage>> messagesToDelete = new();

        private readonly EmbedBuilder error = new EmbedBuilder()
        {
            Color = Color.Red,
            Title = "Произошла непредвиденная ошибка"
        };

        public async Task<RestUserMessage> SendEmbedAsync(ISocketMessageChannel channel, String description, String author = null, bool addThumbnail = false)
        {
            var embed = new EmbedBuilder()
            {
                Color = Color.Purple,
                Description = description
            };

            if (String.IsNullOrEmpty(author))
                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = author,
                    IconUrl = "https://t3.ftcdn.net/jpg/04/54/66/12/360_F_454661277_NtQYM8oJq2wOzY1X9Y81FlFa06DVipVD.jpg"
                };

            if (addThumbnail)
                embed.ThumbnailUrl = "https://t3.ftcdn.net/jpg/04/54/66/12/360_F_454661277_NtQYM8oJq2wOzY1X9Y81FlFa06DVipVD.jpg";

            return await channel.SendMessageAsync(String.Empty, false, embed.Build());
        }

        public async Task<RestUserMessage> SendNowPlayingTrackAsync(ISocketMessageChannel channel, LavaTrack track)
        {
            var message = await SendEmbedAsync(channel, $"**Сейчас играет: `{track.Title}`**\n{track.Url}", "музика ♪ ♫");
            await EnqueueDeletingMessage(message);

            return message;
        }

        public async Task<RestUserMessage> SendEnqueuedTrackAsync(ISocketMessageChannel channel, LavaTrack track)
        {
            var message = await SendEmbedAsync(channel, $"**Добавлен в очередь: `{track.Title}`**\n{track.Url}", "музика ♪ ♫");
            await EnqueueDeletingMessage(message);

            return message;
        }

        public async Task<RestUserMessage> SendEmbedAsync(ISocketMessageChannel channel, Embed embed)
        {
            return await channel.SendMessageAsync(String.Empty, false, embed);
        }

        public async Task SendErrorAsync(ISocketMessageChannel channel)
        {
            await DeleteMessages((channel as SocketGuildChannel).Guild);
            await channel.SendMessageAsync(String.Empty, false, error.Build());
        }

        public async Task DeleteMessages(IGuild guild)
        {
            foreach (var msg in messagesToDelete[guild])
                await msg.DeleteAsync();

            messagesToDelete[guild].Clear();
        }

        public void InvokeDeleteMessage(IUserMessage message)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(messageDeletingDelay);
                await message.DeleteAsync();
            });
        }

        public async Task EnqueueDeletingMessage(RestUserMessage message)
        {
            var guild = (message.Channel as SocketGuildChannel).Guild;

            if (!messagesToDelete.ContainsKey(guild))
                messagesToDelete.Add(guild, new List<IUserMessage>());

            await DeleteMessages(guild);

            messagesToDelete[guild].Add(message);
        }
    }
}
