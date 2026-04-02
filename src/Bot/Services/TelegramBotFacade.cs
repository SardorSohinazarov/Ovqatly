using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Services;

public interface ITelegramBotFacade
{
    Task SendTextMessageAsync(long chatId, string text, CancellationToken cancellationToken);
    Task<string?> GetFilePathAsync(string fileId, CancellationToken cancellationToken);
    Task DownloadFileAsync(string filePath, Stream destination, CancellationToken cancellationToken);
}

public sealed class TelegramBotFacade(ITelegramBotClient botClient) : ITelegramBotFacade
{
    public Task SendTextMessageAsync(long chatId, string text, CancellationToken cancellationToken)
        => botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);

    public async Task<string?> GetFilePathAsync(string fileId, CancellationToken cancellationToken)
        => (await botClient.GetFile(fileId, cancellationToken)).FilePath;

    public Task DownloadFileAsync(string filePath, Stream destination, CancellationToken cancellationToken)
        => botClient.DownloadFile(filePath, destination, cancellationToken);
}
