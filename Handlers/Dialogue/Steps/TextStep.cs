using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Handlers.Dialogue.Steps
{
    public class TextStep : DialogueStepBase
    {
        private readonly int? _minLenght;
        private readonly int? _maxLenght;

        private IDialogueStep _nextStep;

        public TextStep(string content, IDialogueStep nextStep, int? minLenght = null, int? maxLenght = null) : base(content)
        {
            _nextStep = nextStep;
            _minLenght = minLenght;
            _maxLenght = maxLenght;
        }

        public Action<string> OnValidResult { get; set; } = delegate { };

        public override IDialogueStep NextStep => _nextStep;

        public void SetNextStep(IDialogueStep nextStep)
        {
            _nextStep = nextStep;
        }

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Пожалуйста введите сообщение",
                Description = $"{user.Mention},{_content}",
            };

            embedBuilder.AddField("Чтобы остановить диалог", "используйте команду -cancle");

            if (_minLenght.HasValue)
            {
                embedBuilder.AddField("Минимальная длина:", $"{_minLenght.Value} символа");
            }
            if (_maxLenght.HasValue)
            {
                embedBuilder.AddField("Максимальная длина:", $"{_maxLenght.Value} символа");
            }

            var iteractivity = client.GetInteractivity();

            while (true)
            {
                var embed = await channel.SendMessageAsync(embed: embedBuilder).ConfigureAwait(false);

                OnMessageAdded(embed);

                var messageResult = await iteractivity.WaitForMessageAsync(x => x.ChannelId == channel.Id && x.Author.Id == user.Id).ConfigureAwait(false);

                OnMessageAdded(messageResult.Result);

                if (messageResult.Result.Content.Equals("-cancel", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (_minLenght.HasValue)
                {
                    if (messageResult.Result.Content.Length < _minLenght.Value)
                    {
                        await TryAgain(channel, $"Ваше сообщение слишком короткое, содержит  {_minLenght.Value - messageResult.Result.Content.Length} символ").ConfigureAwait(false);
                        continue;
                    }
                }

                OnValidResult(messageResult.Result.Content);
                return false;

            }
        }
    }
}
