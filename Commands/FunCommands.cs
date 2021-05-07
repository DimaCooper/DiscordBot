using DBot.Attributes;
using DBot.Handlers.Dialogue;
using DBot.Handlers.Dialogue.Steps;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Commands
{
    public class FunCommands : BaseCommandModule
    {
        // Команда для проверки работоспособности бота
        [Command("ping")]
        [Description("Бот отвечает pong")]
        // Даем разрешение использовать данную команду в определенных каналах 
        [RequireCategories(ChannelCheckMode.Any, "Текстовые каналы")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        // Складываем 2 числа, по образу и подобию можно сделать полноценный калькулятор при необходимости
        [Command("add")]
        [Description("Сложить 2 числа")]
        // Даем разрешение на использование команды пользователям с определенными ролями 
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Owner")]
        public async Task Add(CommandContext ctx,
            [Description("Первое число")] int numberOne,
            [Description("Второе число")] int numberTwo)
        {
            await ctx.Channel.SendMessageAsync((numberOne + numberTwo).ToString()).ConfigureAwait(false);
        }

        // Команда повторяет сообщение пользователя 
        [Command("response")]
        public async Task Response(CommandContext ctx)
        {

            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }

        // Команда повторяет реакцию пользователя 
        [Command("respondreaction")]
        public async Task ResponseReaction(CommandContext ctx)
        {

            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Emoji);
        }

        // Создание голосовалки в чате
        [Command("poll")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions)
        {

            var interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = string.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);

        }

        // Отправка анонимных сообщений через бота и возможность создать пошаговый диалог
        [Command("d")]
        public async Task Dialogue(CommandContext ctx)
        {

            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            var inputStep = new TextStep("Введите анонимное сообщение!", null, 2);
            var funnyStep = new IntStep("ну привет!", null, maxValue: 100);

            string input = string.Empty;

            int value = 0;

            inputStep.OnValidResult += (result) => {
                input = result;

                if (result == "привет")
                {
                    inputStep.SetNextStep(funnyStep);
                }
            };

            funnyStep.OnValidResult += (result) => value = result;

            var userChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);

            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, inputStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }

            await ctx.Channel.SendMessageAsync(input).ConfigureAwait(false);

            //await ctx.Channel.SendMessageAsync(value.ToString()).ConfigureAwait(false);

        }

        // Команда для создания интерактивного пошагового диалога, взаимодействует с TextStep
        [Command("ed")]
        public async Task EmojiDialogue(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            var yesStep = new TextStep("You chose yes", null);
            var noStep = new IntStep("You chose no", null);

            var emojiStep = new ReactionStep("Yes Or No?", new Dictionary<DiscordEmoji, ReactionStepData>
            {
                {DiscordEmoji.FromName(ctx.Client,":thumbsup:"), new ReactionStepData {Content = "Yes", NextStep = yesStep} },
                {DiscordEmoji.FromName(ctx.Client,":thumbsdown:"), new ReactionStepData {Content = "No", NextStep = noStep} },
            });

            var userChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);

            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, emojiStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }
        } 

    }
}
