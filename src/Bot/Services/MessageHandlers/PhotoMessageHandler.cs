using Bot.Services;
using Telegram.Bot.Types;

namespace Bot.Services.MessageHandlers;

public interface IPhotoMessageHandler
{
    Task HandleAsync(ITelegramBotFacade botClient, Update update, CancellationToken cancellationToken);
}

public sealed class PhotoMessageHandler(
    IAiResponseService aiResponseService,
    IMediaGroupAggregator mediaGroupAggregator,
    ILogger<PhotoMessageHandler> logger) : IPhotoMessageHandler
{
    public async Task HandleAsync(ITelegramBotFacade botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message?.Photo is null)
        {
            logger.LogWarning("Photo update arrived without a message payload.");
            return;
        }

        var chatId = message.Chat.Id;
        var photo = message.Photo.LastOrDefault();
        if (photo is null)
        {
            logger.LogWarning("Photo message {MessageId} does not contain photo sizes.", message.MessageId);
            return;
        }

        if (message.MediaGroupId is not null)
        {
            if (mediaGroupAggregator.TryRegister(message.MediaGroupId))
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    "Media group yubordingiz. Iltimos, har bir ovqatni alohida rasm qilib yuboring. Shunda men har birini alohida kaloriya bilan tahlil qilib beraman.",
                    cancellationToken);
            }

            return;
        }

        var filePath = await botClient.GetFilePathAsync(photo.FileId, cancellationToken);
        using var ms = new MemoryStream();
        if (filePath is null)
        {
            logger.LogWarning("Telegram returned an empty file path for photo {MessageId}.", message.MessageId);
            return;
        }

        await botClient.DownloadFileAsync(filePath, ms, cancellationToken);

        var response = await aiResponseService.AnalyzeFoodImageAsync(ms.ToArray(), cancellationToken);
        if (string.IsNullOrWhiteSpace(response))
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Kechirasiz, rasmni tahlil qilishda javob olinmadi. Iltimos, qayta urinib ko'ring.",
                cancellationToken);
            return;
        }

        await botClient.SendTextMessageAsync(chatId, response, cancellationToken);
    }
}
