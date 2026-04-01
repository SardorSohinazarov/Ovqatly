using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace Bot.Services
{
    public partial class UpdateHandlerService
    {
        private async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var messageHandler = update.Message.Type switch
            {
                MessageType.Text => HandleTextMessageAsync(botClient, update, cancellationToken),
                MessageType.Location => HandleLocationAsync(botClient, update, cancellationToken),
                MessageType.Contact => HandleContactAsync(botClient, update, cancellationToken),
                MessageType.Audio => HandleAudioAsync(botClient, update, cancellationToken),
                MessageType.Sticker => HandlerStrikerAsync(botClient, update, cancellationToken),
                MessageType.Photo => HandlePhotoAsync(botClient, update, cancellationToken),
                MessageType.Dice => HandleDiceAsync(botClient, update, cancellationToken),
                MessageType.Document => HandleDocumentAsync(botClient, update, cancellationToken),
                MessageType.Game => HandleGameAsync(botClient, update, cancellationToken),
                MessageType.Invoice => HandleInvoiceAsync(botClient, update, cancellationToken),
                MessageType.Poll => HandlePollAsync(botClient, update, cancellationToken),
                MessageType.Voice => HandleVoiceAsync(botClient, update, cancellationToken),
                MessageType.VideoNote => HandleVideoNote(botClient, update, cancellationToken),
                MessageType.WebAppData => HandleWebAppDataAsync(botClient, update, cancellationToken),
                MessageType.Video => HandleVideoAsync(botClient, update, cancellationToken),
                _ => HandleUnknownMessageAsync(botClient, update, cancellationToken)
            };

            try
            {
                await messageHandler;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed while handling message of type {MessageType}",
                    update.Message?.Type);

                await SendFallbackMessageAsync(botClient, update, cancellationToken);
            }
        }

        private async Task HandleUnknownMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleVideoAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleWebAppDataAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleVideoNote(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleVoiceAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandlePollAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleInvoiceAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleGameAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleDocumentAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleDiceAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandlerStrikerAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleAudioAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleContactAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleLocationAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
