using Google.GenAI;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Services;

public partial class UpdateHandlerService(
    Client geminiClient,
    ILogger<UpdateHandlerService> logger,
    IPhotoMessageProcessor photoMessageProcessor)
{
    private async Task HandlePhotoAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message is null)
        {
            logger.LogWarning("Photo update arrived without a message payload.");
            return;
        }

        await photoMessageProcessor.HandlePhotoAsync(botClient, message, cancellationToken);
    }
}
