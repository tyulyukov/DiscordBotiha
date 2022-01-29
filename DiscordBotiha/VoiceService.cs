using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBotiha
{
    public class VoiceService
    {
        public readonly MusicService Music;
        
        private LavaNode lavaNode;
        private MessagesService messagesService;

        public VoiceService(DiscordSocketClient client, MessagesService messages)
        {
            LavaConfig cfg = new LavaConfig();
            cfg.SelfDeaf = true;
            cfg.Hostname = "127.0.0.1";
            cfg.Port = 2333;
            cfg.Authorization = "youshallnotpass";

            lavaNode = new LavaNode(client, cfg);
            lavaNode.OnTrackEnded += OnTrackEnded;
            lavaNode.OnLog += Log;

            messagesService = messages;

            Music = new MusicService(lavaNode, this, messagesService);
        }

        public async Task OnReadyAsync()
        {
            if (!lavaNode.IsConnected)
                await lavaNode.ConnectAsync();
        }

        public async Task JoinVoice(ISocketMessageChannel channel, IVoiceState voiceState)
        {
            if (voiceState?.VoiceChannel == null)
            {
                var message = await messagesService.SendEmbedAsync(channel, "**ты че даун ты не подключен к войсу**");
                messagesService.InvokeDeleteMessage(message);

                return;
            }

            if (lavaNode.HasPlayer(voiceState.VoiceChannel.Guild))
            {
                var message = await messagesService.SendEmbedAsync(channel, $"**я уже подключена к войсу...** {new Emoji("\U0001f621")}");
                messagesService.InvokeDeleteMessage(message);

                return;
            }

            try
            {
                await ConnectAsync(voiceState.VoiceChannel, channel as ITextChannel);

                var message = await messagesService.SendEmbedAsync(channel, "**подключилась**");
                messagesService.InvokeDeleteMessage(message);
            }
            catch (Exception ex)
            {
                Debug.Error(ex);

                await messagesService.SendErrorAsync(channel);
            }
        }

        public async Task LeaveVoice(ISocketMessageChannel channel, IVoiceState voiceState)
        {
            var guild = voiceState.VoiceChannel.Guild;

            if (!lavaNode.HasPlayer(guild))
            {
                var message = await messagesService.SendEmbedAsync(channel, "**я не подключена к войсу дуринъ**");
                messagesService.InvokeDeleteMessage(message);

                return;
            }

            var player = lavaNode.GetPlayer(guild);

            if (voiceState?.VoiceChannel != player.VoiceChannel)
            {
                var message = await messagesService.SendEmbedAsync(channel, $"**тебя нет в моём войсе лапух**");
                messagesService.InvokeDeleteMessage(message);

                return;
            }

            try
            {
                await DisconnectAsync(player);
                Music.TrackLists.Remove(player);

                await messagesService.DeleteMessages(guild);

                var message = await messagesService.SendEmbedAsync(channel, "**отключилась**");
                messagesService.InvokeDeleteMessage(message);
            }
            catch (Exception ex)
            {
                Debug.Error(ex);

                await messagesService.SendErrorAsync(channel);
            }
        }

        public async Task ConnectAsync(IVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            if (voiceChannel == null)
                return;

            try
            {
                Debug.Log($"Connecting to channel { voiceChannel.Id }");
                var player = await lavaNode.JoinAsync(voiceChannel, textChannel);
                Debug.Log($"Connected to channel { voiceChannel.Id }");
            }
            catch (Exception ex)
            {
                Debug.Error(ex);
            }
        }

        public async Task DisconnectAsync(LavaPlayer player)
        {
            if (player.VoiceChannel == null)
                return;

            try
            {
                var channel = player.VoiceChannel;
                Debug.Log($"Disconnecting from channel { channel.Id }");
                await lavaNode.LeaveAsync(player.VoiceChannel);
                Debug.Log($"Disconnected from channel { channel.Id }");
            }
            catch (Exception ex)
            {
                Debug.Error(ex);
            }
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task OnTrackEnded(Victoria.EventArgs.TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
                return;

            var nextTrack = Music.TrackLists[args.Player].Next();

            if (nextTrack == null)
                return;

            await args.Player.PlayAsync(nextTrack);

            await messagesService.SendNowPlayingTrackAsync(args.Player.TextChannel as ISocketMessageChannel, nextTrack);
        }
    }
}
