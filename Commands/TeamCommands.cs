using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Commands
{
    public class TeamCommands : BaseCommandModule
    {
        //Создаем команду для выдачи роли
        [Command("join")]

        public async Task Join(CommandContext ctx)
        {

            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            // Сообщение, которые будет выводиться при выполнении команды
            var joinEmbed = new DiscordEmbedBuilder
            {
                Title = "Would you like to join? :+1: to join and :-1: to remove role.",
                Color = DiscordColor.Blue
            };

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);

            // Добавляем к сообщению эмоции, клик по которым добавляет или убирает роль
            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDownEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
            await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
            await joinMessage.CreateReactionAsync(thumbsDownEmoji).ConfigureAwait(false);

            // Взаимодействие с эмоциями.
            var interactivity = ctx.Client.GetInteractivity();
            var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == joinMessage && x.User == ctx.User && (x.Emoji == thumbsUpEmoji || x.Emoji == thumbsDownEmoji)).ConfigureAwait(false);
            var role = ctx.Guild.GetRole(796695943918780436);
            if (reactionResult.Result.Emoji == thumbsUpEmoji)
            {
                // Выдаем роль по клику на эмоцию thumbsUp

                await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
            }
            else if (reactionResult.Result.Emoji == thumbsDownEmoji)
            {
                // Забираем роль по клику на эмоцию thumbsDown

                await ctx.Member.RevokeRoleAsync(role).ConfigureAwait(false);

                // Отправляем сообщение
                await ctx.Channel.SendMessageAsync("Meeeeeh").ConfigureAwait(false);
            }
            else
            {
                // Можно придумать что-нибудь свое
            }
            // Удаляем форму выдачи роли
            await joinMessage.DeleteAsync().ConfigureAwait(false);
        }


    }
}
