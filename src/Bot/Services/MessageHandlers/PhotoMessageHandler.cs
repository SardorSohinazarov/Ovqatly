using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Services.MessageHandlers;

public interface IPhotoMessageHandler
{
    Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}

public sealed class PhotoMessageHandler(
    IAiResponseService aiResponseService,
    IMediaGroupAggregator mediaGroupAggregator,
    ILogger<PhotoMessageHandler> logger) : IPhotoMessageHandler
{
    public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                await botClient.SendMessage(
                    chatId,
                    "Media group yubordingiz. Iltimos, har bir ovqatni alohida rasm qilib yuboring. Shunda men har birini alohida kaloriya bilan tahlil qilib beraman.",
                    cancellationToken: cancellationToken);
            }

            return;
        }

        var file = await botClient.GetFile(photo.FileId, cancellationToken);
        using var ms = new MemoryStream();
        await botClient.DownloadFile(file.FilePath!, ms, cancellationToken);

        var response = await aiResponseService.AnalyzeFoodImageAsync(ms.ToArray(), cancellationToken);
        if (string.IsNullOrWhiteSpace(response))
        {
            await botClient.SendMessage(
                chatId,
                "Kechirasiz, rasmni tahlil qilishda javob olinmadi. Iltimos, qayta urinib ko'ring.",
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
    }
}
