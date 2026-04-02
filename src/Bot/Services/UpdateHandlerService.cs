using Bot.Services.MessageHandlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Services;

public sealed class UpdateHandlerService(
    ITelegramBotFacade botFacade,
    ITextMessageHandler textMessageHandler,
    IPhotoMessageHandler photoMessageHandler,
    ILogger<UpdateHandlerService> logger) : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            await (update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(update, cancellationToken),
                UpdateType.EditedMessage => HandleSkippedUpdateAsync(update.Type),
                UpdateType.CallbackQuery => HandleSkippedUpdateAsync(update.Type),
                UpdateType.InlineQuery => HandleSkippedUpdateAsync(update.Type),
                _ => HandleSkippedUpdateAsync(update.Type)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed while handling update of type {UpdateType}", update.Type);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Telegram polling error occurred while handling source {Source}",
            source);

        return Task.CompletedTask;
    }

    private Task HandleSkippedUpdateAsync(UpdateType updateType)
    {
        logger.LogDebug("Skipping unsupported update type {UpdateType}", updateType);
        return Task.CompletedTask;
    }

    private Task HandleMessageAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message is null)
        {
            return Task.CompletedTask;
        }

        return message.Type switch
        {
            MessageType.Text => textMessageHandler.HandleAsync(botFacade, update, cancellationToken),
            MessageType.Photo => photoMessageHandler.HandleAsync(botFacade, update, cancellationToken),
            _ => HandleSkippedUpdateAsync(update.Type)
        };
    }
}
