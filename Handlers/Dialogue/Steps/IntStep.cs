using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Handlers.Dialogue.Steps
{
    public class IntStep : DialogueStepBase
    {
        private readonly int? _minValue;
        private readonly int? _maxValue;

        private IDialogueStep _nextStep;

        public IntStep(string content, IDialogueStep nextStep, int? minValue = null, int? maxValue = null) : base(content)
        {
            _nextStep = nextStep;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public Action<int> OnValidResult { get; set; } = delegate { };

        public override IDialogueStep NextStep => _nextStep;

        public void SetNextStep(IDialogueStep nextStep)
        {
            _nextStep = nextStep;
        }

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Пожалуйста следуйте указаниям",
                Description = $"{user.Mention},{_content}",
            };

            embedBuilder.AddField("Чтобы остановить диалог", "Введите команду -cancle ");

            // Устанавливаем допустимую длину сообщений
            if (_minValue.HasValue)
            {
                embedBuilder.AddField("Минимальное длина:", $"{_minValue.Value} ");
            }
            if (_maxValue.HasValue)
            {
                embedBuilder.AddField("Максимальная длина:", $"{_maxValue.Value}");
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

                if (!int.TryParse(messageResult.Result.Content, out int inputValue))
                {

                    await TryAgain(channel, $"Your input is not an integer").ConfigureAwait(false);
                    continue;
                }

                if (_minValue.HasValue)
                {
                    if (inputValue < _minValue.Value)
                    {
                        await TryAgain(channel, $"Количество символов: {inputValue} меньше чем: {_minValue}").ConfigureAwait(false);
                        continue;
                    }
                }

                if (_maxValue.HasValue)
                {
                    if (inputValue > _maxValue.Value)
                    {
                        await TryAgain(channel, $"Количество символо: {inputValue} больше чем: {_maxValue}").ConfigureAwait(false);
                        continue;
                    }
                }

                OnValidResult(inputValue);
                return false;

            }
        }
    }
}
