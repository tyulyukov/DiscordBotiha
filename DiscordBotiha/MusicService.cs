using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace DiscordBotiha
{
    public class MusicService
    {
        public readonly Dictionary<LavaPlayer, TrackList> TrackLists;

        private LavaNode lavaNode;
        private VoiceService voiceService;
        private MessagesService messagesService;

        public MusicService(LavaNode lava, VoiceService voice, MessagesService messages)
        {
            TrackLists = new Dictionary<LavaPlayer, TrackList>();
            lavaNode = lava;
            voiceService = voice;
            messagesService = messages;
        }

        public async Task Play(String searchQuery, IVoiceState voiceState, ISocketMessageChannel channel)
        {
            if (String.IsNullOrWhiteSpace(searchQuery))
                return;

            var guild = voiceState?.VoiceChannel?.Guild;

            if (!lavaNode.HasPlayer(guild))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    var message = await messagesService.SendEmbedAsync(channel, "**ты че даун ты не подключен к войсу**");
                    messagesService.InvokeDeleteMessage(message);

                    return;
                }

                await voiceService.ConnectAsync(voiceState.VoiceChannel, channel as ITextChannel);
            }

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            try
            {
                var searchResponse = await lavaNode.SearchYouTubeAsync(searchQuery.ToString());

                if (searchResponse.LoadStatus == LoadStatus.LoadFailed || searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await messagesService.SendErrorAsync(channel);

                    return;
                }

                var track = searchResponse.Tracks[0];

                if (!TrackLists.ContainsKey(player))
                    TrackLists.Add(player, new TrackList());

                TrackLists[player].Tracks.Add(track);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    await messagesService.SendEnqueuedTrackAsync(channel, track);
                }
                else
                {
                    await player.PlayAsync(TrackLists[player].Next());

                    await messagesService.SendNowPlayingTrackAsync(channel, track);
                }
            }
            catch (Exception ex)
            {
                Debug.Error(ex);

                await messagesService.SendErrorAsync(channel);
            }
        }

        public async Task NextTrack(IVoiceState voiceState, ISocketMessageChannel channel)
        {
            var guild = voiceState?.VoiceChannel?.Guild;

            if (!await IsPlayerConnected(guild, channel))
                return;

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            var nextTrack = TrackLists[player].Next();

            if (nextTrack == null)
            {
                var message = await messagesService.SendEmbedAsync(channel, "**некуда больше скипать, совсем чоль**", "музика ♪ ♫");
                messagesService.InvokeDeleteMessage(message);

                return;
            }

            await player.PlayAsync(nextTrack);

            await messagesService.SendNowPlayingTrackAsync(channel, nextTrack);
        }

        public async Task PreviousTrack(IVoiceState voiceState, ISocketMessageChannel channel)
        {
            var guild = voiceState?.VoiceChannel?.Guild;

            if (!await IsPlayerConnected(guild, channel))
                return;

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            var prevTrack = TrackLists[player].Previous();

            if (prevTrack == null)
            {
                var message = await messagesService.SendEmbedAsync(channel, "**слыш чепух, нема треков в истории прослушивания**", "музика ♪ ♫");
                messagesService.InvokeDeleteMessage(message);

                return;
            }

            await player.PlayAsync(prevTrack);

            await messagesService.SendNowPlayingTrackAsync(channel, prevTrack);
        }

        public async Task Pause(IVoiceState voiceState, ISocketMessageChannel channel)
        {
            var guild = voiceState?.VoiceChannel?.Guild;

            if (!await IsPlayerConnected(guild, channel))
                return;

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                var message = await messagesService.SendEmbedAsync(channel, $"**и так на паузе дэлбик**");
                messagesService.InvokeDeleteMessage(message);

                return;
            }

            await player.PauseAsync();

            var messagePause = await messagesService.SendEmbedAsync(channel, "**астанавила**", "музика ♪ ♫");
            messagesService.InvokeDeleteMessage(messagePause);
        }

        public async Task Resume(IVoiceState voiceState, ISocketMessageChannel channel)
        {
            var guild = voiceState?.VoiceChannel?.Guild;

            if (!await IsPlayerConnected(guild, channel))
                return;

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            if (player.PlayerState == PlayerState.Playing)
            {
                var message = await messagesService.SendEmbedAsync(channel, "**и так играет чуча**");
                messagesService.InvokeDeleteMessage(message);       
                                                                    
                return;                                             
            }                                                       
                                                                    
            await player.ResumeAsync();                             
                                                                    
            var messagePause = await messagesService.SendEmbedAsync(channel, "**продолжила**", "музика ♪ ♫");
            messagesService.InvokeDeleteMessage(messagePause);
        }

        public async Task ShowQueue(IVoiceState voiceState, ISocketMessageChannel channel)
        {
            var guild = voiceState?.VoiceChannel?.Guild;

            if (!await IsPlayerConnected(guild, channel))
                return;

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            await messagesService.SendEmbedAsync(channel, "**Очередь треков:\n**`" + TrackLists[player].ToString() + "`", "музика ♪ ♫");
        }

        public async Task ShuffleQueue(IVoiceState voiceState, ISocketMessageChannel channel)
        {
            var guild = voiceState?.VoiceChannel?.Guild;

            if (!await IsPlayerConnected(guild, channel))
                return;

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            var track = TrackLists[player].CurrentTrack;

            TrackLists[player].ShuffleQueue();

            if (track != TrackLists[player].CurrentTrack)
                await player.PlayAsync(TrackLists[player].CurrentTrack);

            var message = await messagesService.SendEmbedAsync(channel, "**очередь перемешана только после текущего трека**", "музика ♪ ♫");
            messagesService.InvokeDeleteMessage(message);
        }

        public async Task FullShuffleQueue(IVoiceState voiceState, ISocketMessageChannel channel)
        {
            var guild = voiceState?.VoiceChannel?.Guild;

            if (!await IsPlayerConnected(guild, channel))
                return;

            var player = lavaNode.GetPlayer(guild);

            if (!await IsUserConnected(voiceState?.VoiceChannel, player.VoiceChannel, channel))
                return;

            TrackLists[player].FullShuffle();

            await player.PlayAsync(TrackLists[player].CurrentTrack);

            var message = await messagesService.SendEmbedAsync(channel, "**очередь полностью перемешана**", "музика ♪ ♫");
            messagesService.InvokeDeleteMessage(message);
        }

        private async Task<bool> IsPlayerConnected(IGuild guild, ISocketMessageChannel channel)
        {
            if (!lavaNode.HasPlayer(guild))
            {
                var message = await messagesService.SendEmbedAsync(channel, "**я не подключена к войсу дуринъ**");
                messagesService.InvokeDeleteMessage(message);

                return false;
            }

            return true;
        }

        private async Task<bool> IsUserConnected(IVoiceChannel userVoice, IVoiceChannel playerVoice, ISocketMessageChannel channel)
        {
            if (userVoice != playerVoice)
            {
                var message = await messagesService.SendEmbedAsync(channel, "**тебя нет в моём войсе лапух**");
                messagesService.InvokeDeleteMessage(message);

                return false;
            }

            return true;
        }
    }
}
