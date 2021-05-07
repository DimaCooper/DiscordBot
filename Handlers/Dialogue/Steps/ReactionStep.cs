using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Handlers.Dialogue.Steps
{
    public class ReactionStep : DialogueStepBase
    {
        private readonly Dictionary<DiscordEmoji, ReactionStepData> _options;

        private DiscordEmoji _selectEmoji;

        public ReactionStep(string content, Dictionary<DiscordEmoji, ReactionStepData> options) : base(content)
        {
            _options = options;
        }

        public override IDialogueStep NextStep => _options[_selectEmoji].NextStep;

        public Action<DiscordEmoji> OnValidResult { get; set; } = delegate { };

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var cancelEmoji = DiscordEmoji.FromName(client, ":x:");

            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Пожалуйста ответьте нажав на эмоцию",
                Description = $"{user.Mention}, {_content}",
            };
            embedBuilder.AddField("Чтобы закрыть диалог", "нажмите на эмоцию :x:");

            var interactivity = client.GetInteractivity();

            while (true)
            {
                var embed = await channel.SendMessageAsync(embed: embedBuilder).ConfigureAwait(false);

                OnMessageAdded(embed);

                foreach (var emoji in _options.Keys)
                {
                    await embed.CreateReactionAsync(emoji).ConfigureAwait(false);
                }

                await embed.CreateReactionAsync(cancelEmoji).ConfigureAwait(false);

                var reactionResult = await interactivity.WaitForReactionAsync(x => _options.ContainsKey(x.Emoji) || x.Emoji == cancelEmoji, embed, user).ConfigureAwait(false);

                if (reactionResult.Result.Emoji == cancelEmoji)
                {
                    return true;
                }

                _selectEmoji = reactionResult.Result.Emoji;

                OnValidResult(_selectEmoji);

                return false;
            }
        }
    }

    public class ReactionStepData
    {
        public string Content { get; set; }

        public IDialogueStep NextStep { get; set; }
    }
}
