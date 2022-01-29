using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotiha
{
    public class DiscordBot
    {
        private const String settingsFileName = "settings.json";

        private MessagesService messagesService;
        private VoiceService voiceService;

        private DiscordSocketClient client;
        private DiscordClientSettings settings;

        private Random random = new Random();

        public async Task Start()
        {
            settings = DiscordClientSettings.Deserialize(settingsFileName);
            
            if (settings == null)
                return;

            client = new DiscordSocketClient();
            client.MessageReceived += MessageHandle;
            client.Log += Log;
            
            messagesService = new MessagesService();
            voiceService = new VoiceService(client, messagesService);

            client.Ready += voiceService.OnReadyAsync;

            Console.ForegroundColor = ConsoleColor.White;

            await client.LoginAsync(TokenType.Bot, settings.Token);
            await client.StartAsync();

            Console.ReadKey(false);
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
            if (!StartsWithPrefix(command.Content))
                return;

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

                await messagesService.SendEmbedAsync(command.Channel, embed.Build());
            }
            else if (commandWord == "join" || commandWord == "connect")
            {
                await voiceService.JoinVoice(command.Channel, command.Author as IVoiceState);
            }
            else if (commandWord == "leave")
            {
                await voiceService.LeaveVoice(command.Channel, command.Author as IVoiceState);
            }
            else if (commandWord == "play" || commandWord == "p")
            {
                if (words.Length <= 1)
                    return;

                StringBuilder searchQuery = new StringBuilder();

                for (int i = 1; i < words.Length; i++)
                    searchQuery.Append(words[i] + ' ');

                await voiceService.Music.Play(searchQuery.ToString(), command.Author as IVoiceState, command.Channel);
            }
            else if (commandWord == "skip" || commandWord == "next")
            {
                await voiceService.Music.NextTrack(command.Author as IVoiceState, command.Channel);
            }
            else if (commandWord == "prev" || commandWord == "previous" || commandWord == "back")
            {
                await voiceService.Music.PreviousTrack(command.Author as IVoiceState, command.Channel);
            }
            else if (commandWord == "pause" || commandWord == "stop")
            {
                await voiceService.Music.Pause(command.Author as IVoiceState, command.Channel);
            }
            else if (commandWord == "resume" || commandWord == "unpause" || commandWord == "continue")
            {
                await voiceService.Music.Resume(command.Author as IVoiceState, command.Channel);
            }
            else if (commandWord == "queue")
            {
                await voiceService.Music.ShowQueue(command.Author as IVoiceState, command.Channel);
            }
            else if (commandWord == "shuffle")
            {
                await voiceService.Music.ShuffleQueue(command.Author as IVoiceState, command.Channel);
            }
            else if (commandWord == "fullshuffle")
            {
                await voiceService.Music.FullShuffleQueue(command.Author as IVoiceState, command.Channel);
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

                await messagesService.SendEmbedAsync(command.Channel, embed.Build());
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

                await messagesService.SendEmbedAsync(command.Channel, $"**хуй {command.Author.Username}: `{huy}см`\n" + description + " **", "Меряемся хуями");
            }
            else if (commandWord == "ping")
            {
                await messagesService.SendEmbedAsync(command.Channel, "ping: " + client.Latency + "ms");
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
