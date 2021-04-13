﻿using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class MessageService
    {
        #region emote                
        private static readonly string _coinEmote = "<a:Coin:637802593413758978>";
        private static readonly string _arrowEmote = "<a:arrow:830799574947463229>";
        private static readonly string _alarmEmote = "<a:alert:637645061764415488>";
        private static readonly string _coeurEmote = "<a:coeur:830788906793828382>";
        private static readonly string _bravoEmote = "<a:bravo:626017180731047977>";
        private static readonly string _checkEmote = "<a:verified:773622374926778380>";        
        private static readonly string _catVibeEmote = "<a:catvibe:792184060054732810>";
        private static readonly string _pikachuEmote = "<a:hiPikachu:637802627345678339>";
        private static readonly string _pepeSmokeEmote = "<a:pepeSmoke:830799658354737178>";
        #endregion
        #region emoji
        private static readonly string _coeurEmoji = "\u2764";
        #endregion

        private DiscordSocketClient _client;
        private readonly JellyfinService _jellyfinService;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public MessageService(DiscordSocketClient client, JellyfinService jellyfinService)
        {
            _client = client;
            _jellyfinService = jellyfinService;

            _client.Ready += Ready;            
            _client.UserLeft += UserLeft;            
            _client.MessageUpdated += MessageUpdated;               
        }


        #region Client
        public async Task MessageUpdated(Cacheable<IMessage, ulong> msgBefore, SocketMessage msgAfter, ISocketMessageChannel channel)
        {
            if (!IsStaffMsg(msgAfter))
            {
                // If the message was not in the cache, downloading it will result in getting a copy of `after`.
                var msg = await msgBefore.GetOrDownloadAsync();

                if(msg.Content != msgAfter.Content)
                    Console.WriteLine($"{msgAfter.Author.Username} edit : \"{msg}\" ---> \"{msgAfter}\" from {channel.Name}");
            }
        }

        /// <summary>
        /// When guild data has finished downloading (+state : Ready)
        /// </summary>
        /// <returns></returns>
        public async Task Ready()
        {
            await SendLatencyAsync();                        
            await CheckBirthday();

            return;
        }


        /// <summary>
        /// When a User left the Guild
        /// </summary>
        /// <param name="guildUser"></param>
        /// <returns></returns>
        private async Task UserLeft(SocketGuildUser guildUser)
        {
            string user = guildUser.Username + '#' + guildUser.Discriminator;
            string joinedAt = Helper.ConvertToSimpleDate(guildUser.JoinedAt.Value);                                    
            string message = $"```{user} left Zderland ! This person joined at {joinedAt}```";

            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, "log");

            if (channel != null)
                await channel.SendMessageAsync(message);

            return;
        }        
        #endregion

        #region Reaction
        public async Task AddReactionVu(SocketUserMessage message)
        {
            // --> 👀
            Emoji vu = new Emoji("\uD83D\uDC40");
            await message.AddReactionAsync(vu);
        }

        public async Task AddReactionRefused(SocketUserMessage message)
        {
            // --> ❌
            Emoji cross = new Emoji("\u274C");
            await message.AddReactionAsync(cross);
        }

        public async Task AddReactionRobot(SocketUserMessage message)
        {
            // --> 🤖
            Emoji robot = new Emoji("\uD83E\uDD16");
            await message.AddReactionAsync(robot);
        }

        public async Task AddReactionAlarm(SocketUserMessage message)
        {
            // --> Alarm (emote)
            var alarm = Emote.Parse(_alarmEmote) ;            
            await message.AddReactionAsync(alarm);
        }

        public async Task AddReactionBirthDay(IMessage message)
        {
            // --> Clap-Clap (emote)
            var bravo = Emote.Parse(_bravoEmote);
            // --> 🎂
            Emoji cake = new Emoji("\uD83C\uDF82");

            await message.AddReactionAsync(cake);
            await message.AddReactionAsync(bravo);
        }

        internal async Task JellyfinDone(SocketUserMessage message)
        {
            await message.RemoveAllReactionsAsync();

            // --> :Verified: (emote)
            var check = Emote.Parse(_checkEmote);
            await message.AddReactionAsync(check);
        }
        #endregion

        #region Message
        public async Task SendLatencyAsync()
        {                       
            string message = $"{Helper.GetGreeting()}```Je suis à {_client.Latency}ms de vous !```";
            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, "log");

            if (channel != null)            
                //await channel.SendMessageAsync(message, isTTS:true);//////////////////////////////////////////////////////////////////////
            
            log.Info($"Latency : {_client.Latency} ms");
        }

        private async Task CheckBirthday()
        {
            Dictionary<string, DateTime> birthsDay = Helper.GetBirthsDay();
            var isSomeoneBD = birthsDay.ContainsValue(DateTime.Today);

            if (isSomeoneBD)
            {
                string id = birthsDay.First(x => x.Value == DateTime.Today).Key;
                string message = $"@everyone {_pikachuEmote} \n" +
                    $"On me souffle dans l'oreille que c'est l'anniversaire de <@{id}> aujourd'hui !\n" +
                    $"*ps : j'ai pas vraiment d'oreille*";

                ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, "general");

                if (channel != null)
                {                    
                    var res = (IMessage) channel.SendMessageAsync(message).Result;
                    await AddReactionBirthDay(res);
                }                    
            }
            return;
        }

        public async Task SendJellyfinNotAuthorize(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync($"```⚠️ Pour des raisons de sécurité l'utilisation de Jellyfin" +
                $" est limité au channel 🌐︱jellyfin ⚠️```");
            await channel.SendMessageAsync($"```Si vous  Vince pour " +
                $"qu'il vous créé un compte```<#816283362478129182>");            
            return;
        }

        public async Task SendJellyfinAlreadyInUse(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync($"{_alarmEmote} Un lien a déjà été généré il y a moins de 2h {_alarmEmote}");
            return;
        }
        #endregion

        #region Get Emoji/Emote
        public string GetCoinEmote() { return _coinEmote; }
        public string GetCoeurEmote() { return _coeurEmote; }
        public string GetCatVibeEmote() { return _catVibeEmote; } 
        public string GetArrowEmote() { return _arrowEmote; }
        public string GetPepeSmokeEmote() { return _pepeSmokeEmote; }

        public string GetCoeurEmoji() { return _coeurEmoji; }
        #endregion

        private static bool IsStaffMsg(SocketMessage msg)
        {            
            return (msg.Author.IsBot || msg.Author.Username.StartsWith("Vince"));
        }
    }
}