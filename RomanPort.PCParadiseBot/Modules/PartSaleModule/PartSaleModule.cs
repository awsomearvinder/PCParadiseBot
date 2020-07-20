﻿using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using RomanPort.PCParadiseBot.Addons.Reddit;

namespace RomanPort.PCParadiseBot.Modules.PartSaleModule
{
    /// <summary>
    /// WRITTEN BY: awesomearvinder
    /// </summary>
    public class PartSaleModule : PCModule
    {
        public RedditClient reddit;

        public DiscordChannel salesChannel;

        public override async Task OnInit()
        {
            //Validate
            if (Program.config.part_sale_module_update_interval_seconds == 0)
                throw new Exception("PC Part Sale Module Update Interval is not set in the config file!");
            
            //Create the reddit client.
            reddit = await RedditClient.init(PCStatics.enviornment.reddit_secret, "PCParadiseBotv1");

            //Fetch channel
            salesChannel = await Program.discord.GetChannelAsync(PCStatics.enviornment.channel_sales);

            //Begin the loop to fetch the list
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await UpdateList();
                        await Task.Delay(Program.config.part_sale_module_update_interval_seconds * 1000);
                    }
                    catch (Exception ex)
                    {
                        await LogToServer("Failed to Update", "Failed to update sales message! Message may have been deleted or failed network request.", null);
                    }
                }
            });
        }

        public async Task UpdateList()
        {
            //Prepare Reddit client
            var sub = await reddit.GetSub("buildapcsales");

            //Create the embed
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor(245, 84, 66))
            .WithTitle("Current Part Sales!")
            .WithFooter("Powered by r/buildapcsales • Created by John Benber#9876", "https://www.redditinc.com/assets/images/site/reddit-logo.png")
            .WithTimestamp(DateTime.UtcNow);
            for (int i = 1; i < 6; i++)
            {
                if (!sub.posts.Current.stickied)
                    builder.AddField($"Score {sub.posts.Current.score} (submitted by u/{sub.posts.Current.author}):", $"[{sub.posts.Current.name}]({sub.posts.Current.url})");
                else
                    i--; //we still want 5 posts, so don't increment this time.
                await sub.posts.MoveNextAsync();
            }

            //Build and send message
            DiscordEmbed embed = builder.Build();
            await salesChannel.SendMessageAsync(content: "", embed: embed);
        }
    }
}
