using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Services
{
    public partial class UpdateHandlerService : IUpdateHandler
    {
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var updateHandler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(botClient, update, cancellationToken),
                UpdateType.EditedMessage => HandleEditedMessageAsync(botClient, update, cancellationToken),
                UpdateType.CallbackQuery => HandleCallbackQueryAsync(botClient, update, cancellationToken),
                UpdateType.InlineQuery => HandleInlineQueryAsync(botClient, update, cancellationToken),
                _ => HandleUnknownUpdateAsync(botClient, update, cancellationToken),
            };

            try
            {
                await updateHandler;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed while handling update of type {UpdateType}", update.Type);
                await SendFallbackMessageAsync(botClient, update, cancellationToken);
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            logger.LogError(
                exception,
                "Telegram polling error occurred while handling source {Source}",
                source);

            // Global polling errors do not include the original update/chat context,
            // so we only log here. User-facing fallback messages are sent from the
            // per-update catch blocks where chat context is available.
            await Task.CompletedTask;
        }

        private async Task HandleUnknownUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleInlineQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleEditedMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task SendFallbackMessageAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            var chatId =
                update.Message?.Chat.Id
                ?? update.EditedMessage?.Chat.Id
                ?? update.CallbackQuery?.Message?.Chat.Id;

            if (chatId is null)
            {
                logger.LogWarning("Skipping fallback response because no chat context was available.");
                return;
            }

            try
            {
                await botClient.SendMessage(
                    chatId.Value,
                    "Kechirasiz, kutilmagan xatolik yuz berdi. Iltimos, bir ozdan keyin qayta urinib ko'ring.",
                    cancellationToken: cancellationToken);
            }
            catch (Exception sendEx)
            {
                logger.LogWarning(
                    sendEx,
                    "Failed to send fallback message to chat {ChatId}",
                    chatId.Value);
            }
        }
    }
}
