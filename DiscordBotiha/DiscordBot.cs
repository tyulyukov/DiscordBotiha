using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Rest;

namespace DiscordBotiha
{
    public class DiscordBot
    {
        private const String settingsFileName = "settings.json";
        private const int messageDeletingDelay = 5000;

        private MessagesService messages;
        private DiscordSocketClient client;
        private DiscordClientSettings settings;

        private LavaNode lavaNode;

        private Dictionary<LavaPlayer, TrackList> trackList = new();

        private Random random = new Random();

        public async Task Start()
        {
            settings = DiscordClientSettings.Deserialize(settingsFileName);

            messages = new MessagesService();

            if (settings == null)
                return;

            Console.ForegroundColor = ConsoleColor.White;

            ConfigureClient();

            await client.LoginAsync(TokenType.Bot, settings.Token);
            await client.StartAsync();

            Console.ReadKey(false);
        }

        private void ConfigureClient()
        {
            client = new DiscordSocketClient();
            client.MessageReceived += MessageHandle;
            client.Log += Log;
            client.Ready += OnReadyAsync;

            LavaConfig cfg = new LavaConfig();
            cfg.SelfDeaf = true;
            cfg.Hostname = "127.0.0.1";
            cfg.Port = 2333;
            cfg.Authorization = "youshallnotpass";

            lavaNode = new LavaNode(client, cfg);
            lavaNode.OnTrackEnded += OnTrackEnded;
            lavaNode.OnLog += Log;
        }

        private async Task OnTrackEnded(Victoria.EventArgs.TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
                return;

            var nextTrack = trackList[args.Player].Next();

            if (nextTrack == null)
                return;

            await args.Player.PlayAsync(nextTrack);

            await messages.SendNowPlayingAsync(args.Player.TextChannel as ISocketMessageChannel, nextTrack);
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task MessageHandle(SocketMessage message)
        {
            if (!IsMyMessage(message.Author))
                Task.Run(async () => await CommandHandleAsync(message));

            return Task.CompletedTask;
        }

        private async Task CommandHandleAsync(SocketMessage command)
        {
            var channel = command.Channel as SocketGuildChannel;

            if (channel == null)
                return;

            var guild = channel.Guild;

            if (StartsWithPrefix(command.Content))
            {
                String[] words = command.Content.Trim().Substring(settings.Prefix.Length).Split(' ');
                String commandWord = words[0].ToLower();

                if (commandWord == "help")
                {
                    var embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = "Личные приказы ♪ ♫",
                            IconUrl = "https://t3.ftcdn.net/jpg/04/54/66/12/360_F_454661277_NtQYM8oJq2wOzY1X9Y81FlFa06DVipVD.jpg"
                        },
                        Color = Color.Purple,
                    };

                    embed.AddField(x =>
                    {
                        x.Name = "**Войс**";
                        x.Value = $"`{settings.Prefix}join`\n" +
                                  $"`{settings.Prefix}leave`";
                        x.IsInline = true;
                    });

                    embed.AddField(x =>
                    {
                        x.Name = "**музика**";
                        x.Value = $"`{settings.Prefix}play *track*`\n" +
                                  $"`{settings.Prefix}skip`\n" +
                                  $"`{settings.Prefix}prev`\n" +
                                  $"`{settings.Prefix}queue`\n" +
                                  $"`{settings.Prefix}pause`\n" +
                                  $"`{settings.Prefix}continue`\n" +
                                  $"`{settings.Prefix}shuffle`\n" +
                                  $"`{settings.Prefix}fullshuffle`\n";
                        x.IsInline = true;
                    });

                    embed.AddField(x =>
                    {
                        x.Name = "**Приколюха**";
                        x.Value = $"`{settings.Prefix}avatar *@user*`\n" +
                                  $"`{settings.Prefix}huy`";
                        x.IsInline = true;
                    });

                    await messages.SendEmbedAsync(command.Channel, embed.Build());
                }
                else if (commandWord == "join" || commandWord == "connect")
                {
                    if (lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, $"**я уже подключена к войсу...** {new Emoji("\U0001f621")}");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;

                    if (voiceState?.VoiceChannel == null)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**ты че даун ты не подключен к войсу**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    try
                    {
                        await ConnectAsync(voiceState.VoiceChannel, command.Channel as ITextChannel);

                        var message = await messages.SendEmbedAsync(command.Channel, "**подключилась**");
                        InvokeDeleteMessage(message, messageDeletingDelay);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ex.Message);
                        Console.WriteLine($"--------------------\n{ex.StackTrace}\n--------------------");

                        await messages.SendErrorAsync(command.Channel);
                    }
                }
                else if (commandWord == "leave")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**я не подключена к войсу дуринъ**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, $"**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    try
                    {
                        await DisconnectAsync(player);
                        trackList.Remove(player);

                        await messages.DeleteMessages(guild); 

                        var message = await messages.SendEmbedAsync(command.Channel, "**отключилась**");
                        InvokeDeleteMessage(message, messageDeletingDelay);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ex.Message);
                        Console.WriteLine($"--------------------\n{ex.StackTrace}\n--------------------");

                        await messages.SendErrorAsync(command.Channel);
                    }
                }
                else if (commandWord == "play" || commandWord == "p")
                {
                    if (words.Length <= 1)
                        return;

                    var voiceState = command.Author as IVoiceState;

                    if (!lavaNode.HasPlayer(guild))
                    {
                        if (voiceState?.VoiceChannel == null)
                        {
                            var message = await messages.SendEmbedAsync(command.Channel, "**ты че даун ты не подключен к войсу**");
                            InvokeDeleteMessage(message, messageDeletingDelay);

                            return;
                        }

                        await ConnectAsync(voiceState.VoiceChannel, command.Channel as ITextChannel);
                    }

                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    try
                    {
                        StringBuilder searchQuery = new StringBuilder();

                        for (int i = 1; i < words.Length; i++)
                            searchQuery.Append(words[i] + ' ');

                        SearchResponse searchResponse = await lavaNode.SearchYouTubeAsync(searchQuery.ToString());
                        if (searchResponse.LoadStatus == LoadStatus.LoadFailed || searchResponse.LoadStatus == LoadStatus.NoMatches)
                        {
                            await messages.SendErrorAsync(command.Channel);                             

                            return;
                        }

                        var track = searchResponse.Tracks[0];

                        if (!trackList.ContainsKey(player))
                            trackList.Add(player, new TrackList());

                        trackList[player].Tracks.Add(track);

                        if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                        {
                            await messages.SendEnqueuedAsync(command.Channel, track);
                        }
                        else
                        {
                            await player.PlayAsync(trackList[player].Next());

                            await messages.SendNowPlayingAsync(command.Channel, track);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ex.Message);
                        Console.WriteLine($"--------------------\n{ex.StackTrace}\n--------------------");

                        await messages.SendErrorAsync(command.Channel);
                    }
                }
                else if (commandWord == "skip" || commandWord == "next")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**я не подключена к войсу дуринъ**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var nextTrack = trackList[player].Next();

                    if (nextTrack == null)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**некуда больше скипать, совсем чоль**", "музика ♪ ♫");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    await player.PlayAsync(nextTrack);

                    await messages.SendNowPlayingAsync(command.Channel, nextTrack);
                }
                else if (commandWord == "prev" || commandWord == "previous" || commandWord == "back")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, $"**я не подключена к войсу дуринъ**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var prevTrack = trackList[player].Previous();

                    if (prevTrack == null)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**слыш чепух, нема треков в истории прослушивания**", "музика ♪ ♫");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    await player.PlayAsync(prevTrack);

                    await messages.SendNowPlayingAsync(command.Channel, prevTrack);
                }
                else if (commandWord == "pause" || commandWord == "stop")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, $"**и так на паузе дэлбик**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    await player.PauseAsync();

                    var messagePause = await messages.SendEmbedAsync(command.Channel, "**астанавила**", "музика ♪ ♫");
                    InvokeDeleteMessage(messagePause, messageDeletingDelay);
                }
                else if (commandWord == "resume" || commandWord == "unpause" || commandWord == "continue")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**я не подключена к войсу дуринъ**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    if (player.PlayerState == PlayerState.Playing)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**и так играет чуча**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    await player.ResumeAsync();

                    var messagePause = await messages.SendEmbedAsync(command.Channel, "**продолжила**", "музика ♪ ♫");
                    InvokeDeleteMessage(messagePause, messageDeletingDelay);
                }
                else if (commandWord == "queue")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**я не подключена к войсу дуринъ**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    await messages.SendEmbedAsync(command.Channel, "**Очередь треков:\n**`" + trackList[player].ToString() + "`", "музика ♪ ♫");
                }
                else if (commandWord == "shuffle")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**я не подключена к войсу дуринъ**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var track = trackList[player].CurrentTrack;

                    trackList[player].ShuffleQueue();

                    if (track != trackList[player].CurrentTrack)
                        await player.PlayAsync(trackList[player].CurrentTrack);

                    var messageShuffled = await messages.SendEmbedAsync(command.Channel, "**очередь перемешана только после текущего трека**", "музика ♪ ♫");
                    InvokeDeleteMessage(messageShuffled, messageDeletingDelay);
                }
                else if (commandWord == "fullshuffle")
                {
                    if (!lavaNode.HasPlayer(guild))
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**я не подключена к войсу дуринъ**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    var voiceState = command.Author as IVoiceState;
                    var player = lavaNode.GetPlayer(guild);

                    if (voiceState?.VoiceChannel != player.VoiceChannel)
                    {
                        var message = await messages.SendEmbedAsync(command.Channel, "**тебя нет в моём войсе лапух**");
                        InvokeDeleteMessage(message, messageDeletingDelay);

                        return;
                    }

                    trackList[player].FullShuffle();

                    await player.PlayAsync(trackList[player].CurrentTrack);

                    var messageShuffled = await messages.SendEmbedAsync(command.Channel, "**очередь полностью перемешана**", "музика ♪ ♫");
                    InvokeDeleteMessage(messageShuffled, messageDeletingDelay);
                }
                else if (commandWord == "avatar")
                {
                    if (command.MentionedUsers.Count == 0)
                        return;

                    var user = command.MentionedUsers.ElementAt(0);

                    var embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = "Аватарка",
                            IconUrl = "https://t3.ftcdn.net/jpg/04/54/66/12/360_F_454661277_NtQYM8oJq2wOzY1X9Y81FlFa06DVipVD.jpg"
                        },
                        Color = Color.Purple,
                        Title = user.Username,
                        ThumbnailUrl = user.GetAvatarUrl()
                    };

                    await messages.SendEmbedAsync(command.Channel, embed.Build());
                }
                else if (commandWord == "huy")
                {
                    String description = String.Empty;
                    int huy = random.Next(-5, 26);

                    if (huy < 0) description = QuotesForHuy.RandomLessThanZero;
                    else if (huy == 0) description = QuotesForHuy.RandomIsZero;
                    else if (huy < 5) description = QuotesForHuy.RandomLessThenFive;
                    else if (huy == 8) description = QuotesForHuy.RandomIsEight;
                    else if (huy < 8) description = QuotesForHuy.RandomLessThenEight;
                    else if (huy < 11) description = QuotesForHuy.RandomLessThenEleven;
                    else if (huy < 15) description = QuotesForHuy.RandomLessThenFifteen;
                    else if (huy < 20) description = QuotesForHuy.RandomLessThenTwenty;
                    else if (huy < 25) description = QuotesForHuy.RandomLessThenTwentyFive;
                    else if (huy == 25) description = QuotesForHuy.RandomIsTwentyFive;

                    await messages.SendEmbedAsync(command.Channel, $"**хуй {command.Author.Username}: `{huy}см`\n" + description + " **", "Меряемся хуями");
                }
                else if (commandWord == "ping")
                {
                    await messages.SendEmbedAsync(command.Channel, "ping: " + client.Latency + "ms");
                }
            }
        }

        private async Task OnReadyAsync()
        {
            if (!lavaNode.IsConnected)
                await lavaNode.ConnectAsync();
        }

        private void InvokeDeleteMessage(IUserMessage message, int ms)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(ms);
                await message.DeleteAsync();
            });
        }

        private async Task ConnectAsync(IVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            if (voiceChannel == null)
                return;

            try
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + $"Connecting to channel {voiceChannel.Id}");
                var player = await lavaNode.JoinAsync(voiceChannel, textChannel);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + $"Connected to channel {voiceChannel.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ex.Message);
                Console.WriteLine($"--------------------\n{ex.StackTrace}\n--------------------");
            }
        }

        private async Task DisconnectAsync(LavaPlayer player)
        {
            if (player.VoiceChannel == null)
                return;

            try
            {
                var channel = player.VoiceChannel;
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + $"Disconnecting from channel {channel.Id}");
                await lavaNode.LeaveAsync(player.VoiceChannel);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + $"Disconnected from channel {channel.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ex.Message);
                Console.WriteLine($"--------------------\n{ex.StackTrace}\n--------------------");
            }
        }

        private bool IsMyMessage(SocketUser author)
        {
            return author.Discriminator == client.CurrentUser.Discriminator;
        }

        private bool StartsWithPrefix(String message)
        {
            return message.TrimStart().StartsWith(settings.Prefix);
        }
    }
}
